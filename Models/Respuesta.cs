using System.ComponentModel.DataAnnotations;


namespace DevelPrueba.Models
{
    public class Respuesta
    {
        public int Id { get; set; }
        [Required] public int EncuestaId { get; set; }
        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
        [StringLength(64)] public string? RemoteIp { get; set; }
        [StringLength(256)] public string? UserAgent { get; set; }


        public Encuesta? Encuesta { get; set; }
        public ICollection<RespuestaValor> Valores { get; set; } = new List<RespuestaValor>();
    }
}