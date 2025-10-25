using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DevelPrueba.Models
{
    public class Campo
    {
        public int Id { get; set; }
        [Required] public int EncuestaId { get; set; }
        [Required, StringLength(100)] public string Nombre { get; set; } = string.Empty;
        [Required, StringLength(200)] public string Titulo { get; set; } = string.Empty;
        public bool EsRequerido { get; set; }
        public TipoCampo Tipo { get; set; }  
        public int Orden { get; set; } = 0;


        public Encuesta? Encuesta { get; set; }
    }
}