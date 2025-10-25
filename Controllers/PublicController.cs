using DevelPrueba.Data;
using DevelPrueba.Models;
using DevelPrueba.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DevelPrueba.Controllers
{
    public class PublicController : Controller
    {
        private readonly ApplicationDbContext _db;
        public PublicController(ApplicationDbContext db) { _db = db; }


        [HttpGet("/f/{slug}")]
        public async Task<IActionResult> Fill(string slug)
        {
            var enc = await _db.Encuestas.Include(e => e.Campos)
            .FirstOrDefaultAsync(e => e.SlugPublico == slug && e.Activa);
            if (enc == null) return NotFound();
            return View(new FillSurveyVm(enc));
        }


        [HttpPost("/f/{slug}"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Fill(string slug, FillSurveyPostVm vm)
        {
            var enc = await _db.Encuestas
                .Include(e => e.Campos)
                .FirstOrDefaultAsync(e => e.SlugPublico == slug && e.Activa);
            if (enc == null) return NotFound();

            // OPTIONAL: honeypot (uncomment if you added an input named "hp" to the view)
            // if (!string.IsNullOrEmpty(Request.Form["hp"])) return BadRequest();

            // Normalize: trim posted values
            if (vm.Valores != null)
            {
                // snapshot keys to avoid "collection modified" issues
                foreach (var key in vm.Valores.Keys.ToList())
                {
                    var current = vm.Valores[key];                  // string (possibly null at runtime)
                    vm.Valores[key] = (current ?? string.Empty).Trim();  // <-- never null
                }
            }


            // Validate
            foreach (var campo in enc.Campos.OrderBy(c => c.Orden))
            {
                string? valor = null;
                if (vm.Valores != null)
                    vm.Valores.TryGetValue(campo.Nombre, out valor);


                if (campo.EsRequerido && string.IsNullOrWhiteSpace(valor))
                    ModelState.AddModelError(campo.Nombre, $"{campo.Titulo} es requerido");

                if (!string.IsNullOrWhiteSpace(valor))
                {
                    if (campo.Tipo == TipoCampo.Numero && !decimal.TryParse(valor, out _))
                        ModelState.AddModelError(campo.Nombre, $"{campo.Titulo} debe ser numérico");

                    if (campo.Tipo == TipoCampo.Fecha && !DateTime.TryParse(valor, out _))
                        ModelState.AddModelError(campo.Nombre, $"{campo.Titulo} debe ser una fecha válida");
                }
            }

            if (!ModelState.IsValid)
            {
                // Return same view with the user's data preserved
                var back = new FillSurveyVm(enc) { Valores = vm.Valores ?? new Dictionary<string, string>() };
                return View(back);
            }

            // Persist
            var resp = new Respuesta
            {
                EncuestaId = enc.Id,
                RemoteIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent
            };

            foreach (var campo in enc.Campos)
            {
                string? val = null;
                if (vm.Valores != null)
                    vm.Valores.TryGetValue(campo.Nombre, out val);

                resp.Valores.Add(new RespuestaValor
                {
                    CampoId = campo.Id,
                    Valor = val ?? string.Empty
                });
            }

            _db.Respuestas.Add(resp);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Thanks));
        }



        [HttpGet]
        public IActionResult Thanks() => View();
    }
}