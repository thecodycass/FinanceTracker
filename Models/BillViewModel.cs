using FinanceTracker.Entities;
namespace FinanceTracker.Models;

public class BillViewModel
{
    public List<Bill> Bills { get; set; }
    public List<Bill> BillsDueThisWeek  { get; set; }
    public List<Bill> BillsDueNextWeek { get; set; }
    public decimal YearlyAmount { get; set; }
    public decimal WeeklyAmount { get; set; }
    public decimal MonthlyAmount { get; set; }
}