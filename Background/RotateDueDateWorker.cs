using FinanceTracker.Database;
using FinanceTracker.Entities;
using FinanceTracker.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Background;

/*
 * Rotates the Due Date for Bills with a StatusType labeled as 'Weekly' and 'Monthly'.
 *      Weekly due dates rotate by seven days (one week)
 *      Monthly due dates rotate by 30 days (one month)
 *      Partial due dates get archived as they are only a one time thing.
 * Notifications for each bill that will be rotate will also be removed as it is no longer needed.
 */
public class RotateDueDateWorker(ILogger<NotifierWorker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        var today = DateTime.Today;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var billsDueYesterday =
                    await dbContext.Bills
                        .Where(b => b.dueDate.Date == today.AddDays(-1))
                        .ToListAsync(stoppingToken);
                
                RotateDueDate(billsDueYesterday, dbContext, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }

            
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private static void RotateDueDate(List<Bill> billsDueYesterday, AppDbContext dbContext, CancellationToken stoppingToken)
    {
        foreach (var bill in billsDueYesterday)
        {
            /*
             * We need to delete the notification record associated with the bill when we rotate the dueDate
             * because the notifications table usees billId as a key to 'join()' to find the corresponding bill
             * to action on.
             */
            var notification = dbContext.Notifications.FirstOrDefault(n => n.BillId == bill.id);
            if (notification != null) dbContext.Remove(notification);
            
            // Rotate Weekly Bills
            if (bill.status.Equals(BillStatusType.WEEKLY, StringComparison.InvariantCultureIgnoreCase))
            {
                bill.dueDate = bill.dueDate.Date.AddDays(7);
            }

            // Rotate Monthly Bills
            if (bill.status.Equals(BillStatusType.MONTHLY, StringComparison.InvariantCultureIgnoreCase))
            {
                bill.dueDate = bill.dueDate.Date.AddMonths(1);
            }
                    
            // Archive Partial bills past due date since they do not rotate normally
            if (bill.status.Equals(BillStatusType.PARTIAL, StringComparison.InvariantCultureIgnoreCase))
            {
                bill.archived = true;
            }
            dbContext.Update(bill);
            
            dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}