using System.Diagnostics;
using FinanceTracker.Database;
using FinanceTracker.Entities;
using Microsoft.AspNetCore.Mvc;
using FinanceTracker.Models;

namespace FinanceTracker.Controllers.WEB;

public class HomeViewController(AppDbContext dbContext) : Controller
{

    public IActionResult Index()
    {
        var bills = new List<Bill>();
        try
        {
            bills = dbContext.Bills.Where(b => b.archived == false).OrderBy(b => b.dueDate.Date).ToList();
        }
        catch (Exception e)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: e.Message
            );
        }
    
        var viewModel = new HomeViewModel
        {
            Bills = bills.OrderBy(b => b.amount).ToList()
        };
    
        return View(viewModel);
    }
    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}