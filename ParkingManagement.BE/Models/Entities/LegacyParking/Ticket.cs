using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Entities;

public enum TicketStatus
{
    Active,
    Completed,
    Cancelled
}

public class Ticket
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public DateTime EntryTime { get; set; }

    public DateTime? ExitTime { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Price { get; set; }

    [Required]
    public TicketStatus Status { get; set; } = TicketStatus.Active;

    // Foreign keys
    [Required]
    public int VehicleId { get; set; }

    [Required]
    public int ParkingAreaId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(VehicleId))]
    public Vehicle Vehicle { get; set; } = null!;

    [ForeignKey(nameof(ParkingAreaId))]
    public ParkingArea ParkingArea { get; set; } = null!;
}
