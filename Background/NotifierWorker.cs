using System.Text;
using FinanceTracker.Database;
using FinanceTracker.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Background;

/*
 * Sends Bill Notifications to Discords 'Bills Webhook'
 * This will notify based on DayBefore and DayOf.
 *      DayBefore: A message will be sent one day BEFORE the Bill due date.
 *      DayOf: A message will be sent on Bill due date.
 */
public class NotifierWorker(ILogger<NotifierWorker> logger,
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : BackgroundService
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

                var billsDueToday =
                    await dbContext.Bills
                        .Where(b => b.dueDate.Date == today && b.notifyOnDay)
                        .Where(b => !dbContext.Notifications.Any(n => n.BillId == b.id && n.HasDayOfBeenSent))
                        .ToListAsync(stoppingToken);

                var billsDueTomorrow = await dbContext.Bills
                    .Where(b => b.dueDate.Date == today.AddDays(1) && b.notifyOneDayBefore)
                    .Where(b => !dbContext.Notifications.Any(n => n.BillId == b.id && n.HasDayBeforeBeenSent))
                    .ToListAsync(stoppingToken);

                var httpClient = httpClientFactory.CreateClient();

                var webhookUrl = configuration["Discord:WebhookUrl"];

                if (string.IsNullOrEmpty(webhookUrl))
                {
                    logger.LogInformation("Webhook URL not set");
                    return;
                }

                await SendBillNotificationAsync(billsDueToday, "is due TODAY!", true, httpClient, webhookUrl, dbContext,
                    stoppingToken);
                await SendBillNotificationAsync(billsDueTomorrow, "is due TOMORROW!", false, httpClient, webhookUrl,
                    dbContext, stoppingToken);

                logger.LogInformation(
                    "Bill notifications sent for {todayCount} bills due today, {tomorrowCount} bills due tomorrow",
                    billsDueToday.Count, billsDueTomorrow.Count);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Error while sending notifications");
                throw new HttpRequestException("Error while sending notifications", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw new Exception("An error occured while sending Discord notifications");
            }
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    /*
     * Responsible for sending Bill Notification webhook to Discord 'Bills Webhook'.
     * It updates the Notifications table after each Bill Notification has been sent.
     */
    private async Task SendBillNotificationAsync(
        List<Bill> bills,
        string messageSuffix,
        bool isDayOf,
        HttpClient httpClient,
        string webhookUrl,
        AppDbContext dbContext,
        CancellationToken stoppingToken)
    {
        foreach (var bill in bills)
        {
            var content = GetEmbedContent(bill, messageSuffix);
            
            var response = await httpClient.PostAsync(webhookUrl, content, stoppingToken);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Sent Discord notification for bill {billId}: {billName}", bill.id, bill.name);

                var notification = await dbContext.Notifications.FirstOrDefaultAsync(n => n.BillId == bill.id, stoppingToken);
                
                // Create a new Notification if it is null
                if (notification == null)
                {
                    notification = new Notifications { BillId = bill.id, HasDayBeforeBeenSent = false, HasDayOfBeenSent = false };
                    dbContext.Notifications.Add(notification);
                }
                
                // Set DayOf to show it has been sent
                if (isDayOf)
                    notification.HasDayOfBeenSent = true;
                
                // Set DayBefore to show it has been sent
                else
                    notification.HasDayBeforeBeenSent = true;
                await dbContext.SaveChangesAsync(stoppingToken);
            }
            else
            {
                logger.LogError("Failed to send Discord notification for bill {billId}: HTTP {statusCode}",
                    bill.id, response.StatusCode);
            }
        }
    }

    /*
     * Creates a new Embed message for Discord 'Bills Webhook'
     */
    private StringContent GetEmbedContent(Bill bill, string messageSuffix)
    {
        var embed = new
        {
            embeds = new[]
            {
                new
                {
                    title = $"Bill Reminder: {bill.name}",
                    description =
                        $"Amount: ${bill.amount:F2}\nDue: {bill.dueDate:yyyy-MM-dd}\n\n{bill.name} {messageSuffix}",
                    color = 16776960, // Orange/yellow color
                    timestamp = DateTime.UtcNow.ToString("o")
                }
            }
        };
        var json = System.Text.Json.JsonSerializer.Serialize(embed);
        return  new StringContent(json, Encoding.UTF8, "application/json");
    }
}
