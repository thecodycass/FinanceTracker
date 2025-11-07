using System.Threading.Tasks;
using FinanceTracker.Controllers.WEB;
using FinanceTracker.Database;
using FinanceTracker.Entities;
using FinanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinanceTracker.Tests;

public class BillViewControllerTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Index_ReturnsViewWithBillViewModel()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var controller = new BillViewController(context);

        // Add some test data
        context.Bills.Add(new Bill
        {
            id = 1,
            name = "Test Bill",
            amount = 100.00m,
            dueDate = DateTime.Today.AddDays(7),
            description = "Test bill description",
            status = "monthly",
            archived = false
        });
        await context.SaveChangesAsync();

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BillViewModel>(viewResult.Model);
        Assert.NotNull(model.Bills);
        Assert.True(model.Bills.Count >= 1);
    }

    [Fact]
    public async Task Create_Post_ValidBill_RedirectsToIndex()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var controller = new BillViewController(context);

        var bill = new Bill
        {
            name = "New Test Bill",
            amount = 50.00m,
            dueDate = DateTime.Today,
            description = "New test bill description",
            status = "monthly",
            archived = false
        };

        // Act
        var result = await controller.Create(bill);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Single(context.Bills);
    }

    [Fact]
    public async Task Create_Post_InvalidBill_ReturnsView()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();
        var controller = new BillViewController(context);

        var bill = new Bill(); // Invalid - missing required fields
        controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await controller.Create(bill);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(bill, viewResult.Model);
    }
}
