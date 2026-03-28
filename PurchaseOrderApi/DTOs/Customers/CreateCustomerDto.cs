using System.ComponentModel.DataAnnotations;

namespace PurchaseOrderApi.DTOs.Customers;

public class CreateCustomerDto
{
    [Required]
    [MaxLength(300)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(300)]
    public string Email { get; set; } = string.Empty;
}
