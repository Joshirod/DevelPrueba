using System.ComponentModel.DataAnnotations;


namespace DevelPrueba.Models
{
    public class RespuestaValor
    {
        public int Id { get; set; }
        [Required] public int RespuestaId { get; set; }
        [Required] public int CampoId { get; set; }
        [Required] public string Valor { get; set; } = string.Empty;


        public Respuesta? Respuesta { get; set; }
        public Campo? Campo { get; set; }
    }
}