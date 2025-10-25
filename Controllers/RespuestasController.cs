using DevelPrueba.Data;
using DevelPrueba.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;


namespace DevelPrueba.Controllers
{
    [Authorize]
    public class RespuestasController : Controller
    {
        private readonly ApplicationDbContext _db;
        public RespuestasController(ApplicationDbContext db) { _db = db; }


        public async Task<IActionResult> Index(int id)
        {
            var enc = await _db.Encuestas
                .Include(e => e.Campos)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (enc == null) return NotFound();

            // Materialize first (so I can shape tuples freely)
            var resps = await _db.Respuestas
                .Where(r => r.EncuestaId == id)
                .Select(r => new
                {
                    r.Id,
                    r.FechaEnvio,
                    Valores = r.Valores.Select(v => new { v.CampoId, v.Valor })
                })
                .ToListAsync();

            // Project to the exact tuple shape expected by ResultsVm.From:
            // IEnumerable<(int Id, DateTime Fecha, IEnumerable<(int CampoId, string? Valor)> Valores)>
            var data = resps.Select(r => (
                Id: r.Id,
                Fecha: r.FechaEnvio,
                Valores: r.Valores.Select(v => (CampoId: v.CampoId, Valor: (string?)v.Valor))
            ));

            var vm = ResultsVm.From(enc, data);
            return View(vm);
        }


        public async Task<FileResult> ExportCsv(int id)
        {
            var enc = await _db.Encuestas.Include(e => e.Campos).FirstOrDefaultAsync(e => e.Id == id);
            if (enc == null) throw new Exception("Encuesta no encontrada");
            var resps = await _db.Respuestas.Include(r => r.Valores).Where(r => r.EncuestaId == id).ToListAsync();


            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", enc.Campos.OrderBy(c => c.Orden).Select(c => Escape(c.Titulo))));
            foreach (var r in resps)
            {
                var fila = enc.Campos.OrderBy(c => c.Orden)
                .Select(c => Escape(r.Valores.FirstOrDefault(v => v.CampoId == c.Id)?.Valor ?? string.Empty));
                sb.AppendLine(string.Join(",", fila));
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"encuesta_{enc.Id}.csv");


            static string Escape(string s) => "\"" + (s ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }
    }
}