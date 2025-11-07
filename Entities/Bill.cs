using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Entities;

public class Bill
{
    public int id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100)]
    public string name { get; set; }
    
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue)]
    public decimal amount { get; set; }
    
    [Required(ErrorMessage = "Due date is required")]
    public DateTime dueDate { get; set; }
    
    [StringLength(500)]
    public string description { get; set; }
    
    [Required(ErrorMessage = "Status is required")]
    public string status { get; set; }

    public bool notifyOneDayBefore { get; set; } = false;
    public bool notifyOnDay { get; set; } = false;
    public bool archived { get; set; } = false;
}
