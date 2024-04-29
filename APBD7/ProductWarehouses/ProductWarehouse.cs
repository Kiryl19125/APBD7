using System.ComponentModel.DataAnnotations;

namespace APBD7.Models;

public class ProductWarehouse
{
    [Required] public int idProduct { get; set; }
    [Required] public int idWarehouse { get; set; }
    [Required] public int amount { get; set; }
    [Required] public DateTime createdAt { get; set; }
}