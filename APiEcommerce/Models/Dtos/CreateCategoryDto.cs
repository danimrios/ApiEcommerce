using System;
using System.ComponentModel.DataAnnotations;

namespace APiEcommerce.Models.Dtos;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "El nombre es Requerido")]
    [MaxLength(50, ErrorMessage = "El maximo de caracteres es de 50")]
    [MinLength(3, ErrorMessage ="El Minimo de caracteres es de 3")]
    public string Name { get; set; } = string.Empty;

}
