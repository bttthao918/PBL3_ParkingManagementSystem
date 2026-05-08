using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingManagement.DAL.Entities;

public enum VehicleType
{
    Motorcycle,
    Car,
    Truck,
    Bicycle
}

public class Vehicle
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    public VehicleType VehicleType { get; set; }

    // Navigation property
    public ICollection<Ticket> Tickets { get; set; } = [];
}
