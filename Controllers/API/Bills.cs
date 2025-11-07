using FinanceTracker.Database;
using FinanceTracker.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Controllers.API;

[ApiController]
[Route("[controller]")]
[ProducesResponseType<Bills>(StatusCodes.Status200OK)]
public class Bills(AppDbContext dbContext, ILogger<Bills> logger) : ControllerBase
{
    // POST - Create new bill
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Bill bill)
    {
        if (bill == null)
        {
            throw new ArgumentNullException(nameof(bill));
        }
        bill.id = 0; // Ensure new bill
        dbContext.Bills.Add(bill);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Created bill {Id}", bill.id);
        return CreatedAtAction(nameof(Create), new { id = bill.id }, bill);
    }

    // PUT - Update existing bill
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Bill bill)
    {
        if (id != bill.id)
        {
            return BadRequest("ID mismatch");
        }

        var existingBill = await dbContext.Bills.FindAsync(id);
        if (existingBill == null)
        {
            return NotFound();
        }

        existingBill.name = bill.name;
        existingBill.amount = bill.amount;
        existingBill.dueDate = bill.dueDate;
        existingBill.description = bill.description;
        existingBill.status = bill.status;
        existingBill.notifyOneDayBefore = bill.notifyOneDayBefore;
        existingBill.notifyOnDay = bill.notifyOnDay;

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Updated bill {Id}", id);
        return Ok(existingBill);
    }

    // GET {id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Show(int id)
    {
        var bill = await dbContext.Bills.FindAsync(id);
        if (bill == null)
        {
            return NotFound();
        }
        return Ok(bill);
    }

    // GET (all)
    [HttpGet]
    public async Task<IActionResult> GetAllBills()
    {
        var bills = await dbContext.Bills.ToListAsync();
        return Ok(bills);
    }
}
