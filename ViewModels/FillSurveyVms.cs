using DevelPrueba.Models;


namespace DevelPrueba.ViewModels
{
    public class FillSurveyVm
    {
        public int EncuestaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public List<Campo> Campos { get; set; } = new();
        public Dictionary<string, string> Valores { get; set; } = new();
        public FillSurveyVm() { }
        public FillSurveyVm(Encuesta e)
        {
            EncuestaId = e.Id; Titulo = e.Nombre; Descripcion = e.Descripcion;
            Campos = e.Campos.OrderBy(c => c.Orden).ToList();
        }
    }


    public class FillSurveyPostVm
    {
        public Dictionary<string, string> Valores { get; set; } = new();
    }


    public class ResultsVm
    {
        public string EncuestaNombre { get; set; } = string.Empty;
        public List<string> Columnas { get; set; } = new();
        public List<List<string>> Filas { get; set; } = new();
        public static ResultsVm From(Encuesta e, IEnumerable<(int Id, DateTime Fecha, IEnumerable<(int CampoId, string? Valor)> Valores)> data)
        {
            var vm = new ResultsVm
            {
                EncuestaNombre = e.Nombre,
                Columnas = e.Campos.OrderBy(c => c.Orden).Select(c => c.Titulo).ToList(),
                Filas = new List<List<string>>()
            };
            var campos = e.Campos.OrderBy(c => c.Orden).ToList();
            foreach (var r in data)
            {
                var fila = new List<string>();
                foreach (var c in campos)
                {
                    var val = r.Valores.FirstOrDefault(v => v.CampoId == c.Id).Valor ?? string.Empty;
                    fila.Add(val);
                }
                vm.Filas.Add(fila);
            }
            return vm;
        }
    }
}