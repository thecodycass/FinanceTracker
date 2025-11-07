using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Entities;

public class Notifications
{
    [Key]
    public int Id { get; set; }

    public bool HasDayBeforeBeenSent { get; set; }
    public bool HasDayOfBeenSent { get; set; }

    [Required(ErrorMessage = "Notification UUID is required")]
    public int BillId{ get; set; }
}
