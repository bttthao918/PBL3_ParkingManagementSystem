using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Entities;

public class ParkingArea
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int Capacity { get; set; }

    [Required]
    public int AvailableSlots { get; set; }

    // Navigation property
    public ICollection<Ticket> Tickets { get; set; } = [];
}
