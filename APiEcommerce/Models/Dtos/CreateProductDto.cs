using System;

namespace APiEcommerce.Models.Dtos;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Descripton { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImgUrl { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int Stock { get; set; }
    public DateTime? UpdateDate { get; set; } = null;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

}
