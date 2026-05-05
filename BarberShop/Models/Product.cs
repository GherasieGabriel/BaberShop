namespace BarberShop.Models;

public class Product
{
    public int ProductId { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.StringLength(500)]
    public string? Description { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "999999")]
    public decimal Price { get; set; }

    [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }
}