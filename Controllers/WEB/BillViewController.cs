using System.Globalization;
using FinanceTracker.Database;
using FinanceTracker.Entities;
using FinanceTracker.Entities.Enums;
using FinanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Controllers.WEB;

[Route("[controller]")]
public class BillViewController(AppDbContext database) : Controller
{
    // GET
    [HttpGet]
    public IActionResult Index()
    {
        var today = DateTime.Today;
        // Weekly dates
        var sundayDiff = (7 + (today.DayOfWeek - DayOfWeek.Sunday)) % 7;
        var saturdayDiff = (7 + (DayOfWeek.Sunday - today.DayOfWeek)) % 7;
        var startOfWeek = today.AddDays(-sundayDiff);
        var endOfWeek = today.AddDays(saturdayDiff);
        
        var bills = new List<Bill>();
        try
        {  
            // Only show bills that have not been archived
            bills = database.Bills.Where(b => b.archived == false).OrderBy(b => b.dueDate.Date).ToList();
        }
        catch (Exception e)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: e.Message
            );
        }
        
        return View(new BillViewModel
        {
            Bills = bills, 
            YearlyAmount = GetYearlyAmount(bills, startOfWeek, endOfWeek),  
            WeeklyAmount = GetWeeklyAmount(bills, startOfWeek, endOfWeek), 
            MonthlyAmount = GetMonthlyAmount(bills, today),
            BillsDueThisWeek = GetBillsDueThisWeek(bills, startOfWeek, endOfWeek),
            BillsDueNextWeek = GetBillsDueNextWeek(bills, startOfWeek, endOfWeek)
        });
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        var bill = new Bill()
        {
            dueDate = DateTime.Today,
        };
        return View(bill);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Bill bill)
    {
        if (!ModelState.IsValid)
        {
            return View(bill);
        }

        database.Bills.Add(bill);
        await database.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var bill = await database.Bills.FindAsync(id);
        if (bill == null)
        {
            return NotFound();
        }
        return View(bill);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Bill bill)
    {
        if (!ModelState.IsValid)
        {
            return View(bill);
        }

        database.Entry(bill).State = EntityState.Modified;
        await database.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var bill = await database.Bills.FindAsync(id);
        if (bill == null)
        {
            return NotFound();
        }
        return View(bill);
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var bill = await database.Bills.FindAsync(id);
        if (bill == null)
        {
            return NotFound();
        }

        database.Bills.Remove(bill);
        await database.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    private List<Bill> GetBillsDueThisWeek(List<Bill> bills, DateTime startOfWeek, DateTime endOfWeek)
    {
        return bills
            .Where(
                b => b.dueDate.Date >= startOfWeek 
                     && b.dueDate.Date < endOfWeek 
                     && !b.status.Equals(BillStatusType.CANCELLED, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private List<Bill> GetBillsDueNextWeek(List<Bill> bills, DateTime startOfWeek, DateTime endOfWeek)
    {
        return bills
            .Where(b => b.dueDate.Date >= startOfWeek.AddDays(7) && b.dueDate.Date < endOfWeek.AddDays(7))
            .ToList();
    }

    private decimal GetYearlyAmount(List<Bill> bills, DateTime startOfWeek, DateTime endOfWeek)
    {
        /*
         * Yearly Amount Math:
         * - If Status is "Cancelled", then we skip adding.
         * - If Status is "Weekly", we need to add (amount * 4) to make it monthly, then multiply by 12 to get yearly.
         * - If Status is "Monthly", we just add (amount * 12) to make it yearly.
         */
        return bills
            .Where(b => !b.status.Equals(BillStatusType.CANCELLED, StringComparison.OrdinalIgnoreCase))
            .Sum(b =>
                b.status.Equals(BillStatusType.WEEKLY, StringComparison.OrdinalIgnoreCase) ? (b.amount * 4) * 12 
                    : b.status.Equals((BillStatusType.PARTIAL)) ? b.amount : b.amount * 12);
    }
    
    private static decimal GetWeeklyAmount(List<Bill> bills, DateTime startOfWeek, DateTime endOfWeek)
    {
        return bills
            .Where(b => b.dueDate.Date > startOfWeek 
                        && b.dueDate.Date < endOfWeek 
                        && !b.status.Equals(BillStatusType.CANCELLED, StringComparison.OrdinalIgnoreCase))
            .Sum(b => b.amount);
    }
    
    private static decimal GetMonthlyAmount(List<Bill> bills,  DateTime today)
    {
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        return bills
            .Where(b => b.dueDate.Date > startOfMonth 
                        && b.dueDate.Date < endOfMonth 
                        && !b.status.Equals(BillStatusType.CANCELLED, StringComparison.OrdinalIgnoreCase))
            // For Weekly bills, we need to multiply those by 4 to get a true Monthly
            .Sum(b => b.status.Equals(BillStatusType.WEEKLY, StringComparison.OrdinalIgnoreCase) ? b.amount * 4 : b.amount);
    }
}
