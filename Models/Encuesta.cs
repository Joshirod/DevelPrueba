
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DevelPrueba.Models
{
    public class Encuesta
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [BindNever]
        [ValidateNever]                 
        [Required, StringLength(64)]
        public string SlugPublico { get; set; } = string.Empty;

        [BindNever]
        [ValidateNever]                
        [Required]
        public string CreadoPorUserId { get; set; } = string.Empty;
            
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public bool Activa { get; set; } = true;

        public List<Campo> Campos { get; set; } = new();
    }
}
