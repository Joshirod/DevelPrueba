using DevelPrueba.Data;
using DevelPrueba.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;


namespace DevelPrueba.Controllers
{
    [Authorize]
    public class EncuestasController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        public EncuestasController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        { _db = db; _userManager = userManager; }


        public async Task<IActionResult> Index()
        {
            var list = await _db.Encuestas.Include(e => e.Campos).OrderByDescending(e => e.FechaCreacion).ToListAsync();
            return View(list);
        }


        public IActionResult Create() => View(new Encuesta { Campos = new List<Campo>() });


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Encuesta model)
        {
            // Si SlugPublico y CreadoPorUserId tienen [Required], asígnalos ANTES de validar
            model.SlugPublico = Guid.NewGuid().ToString("N");
            model.CreadoPorUserId = _userManager.GetUserId(User)!;

            // Limpia posibles errores previos de esos keys en el ModelState
            ModelState.Remove(nameof(model.SlugPublico));
            ModelState.Remove(nameof(model.CreadoPorUserId));

            if (!ModelState.IsValid) return View(model);

            // Asegura colección no nula
            if (model.Campos == null) model.Campos = new List<Campo>();

            // ❗ No reasignar c.Orden aquí; viene del hidden Campos[i].Orden (drag & drop)
            // Opcional: si quieres garantizar consistencia, ordena la lista según Orden posteado:
            model.Campos = model.Campos.OrderBy(c => c.Orden).ToList();

            // EF Core seteará EncuestaId en los hijos al agregar el padre con la colección
            _db.Encuestas.Add(model);
            await _db.SaveChangesAsync();

            TempData["Link"] = Url.Action("Fill", "Public", new { slug = model.SlugPublico }, Request.Scheme);
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var enc = await _db.Encuestas.Include(e => e.Campos).FirstOrDefaultAsync(e => e.Id == id);
            if (enc == null) return NotFound();
            return View(enc);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DevelPrueba.Models.Encuesta model)
        {
            if (!ModelState.IsValid) return View(model);

            var enc = await _db.Encuestas
                .Include(e => e.Campos)
                .FirstOrDefaultAsync(e => e.Id == model.Id);
            if (enc == null) return NotFound();

            // Update basic survey props
            enc.Nombre = model.Nombre;
            enc.Descripcion = model.Descripcion;
            enc.Activa = model.Activa;

            // Use the order posted by the form (Campos[i].Orden hidden inputs)
            var incoming = (model.Campos ?? new List<DevelPrueba.Models.Campo>()).ToList();
            // ❗ DO NOT reassign Orden here — keep what came from the client

            // Indexes
            var existingById = enc.Campos.ToDictionary(c => c.Id);
            var incomingIds = incoming.Where(c => c.Id > 0).Select(c => c.Id).ToHashSet();

            // ---- PREVENT DELETE IF THERE ARE RESPUESTAS ----
            var toDelete = enc.Campos.Where(c => !incomingIds.Contains(c.Id)).ToList();
            if (toDelete.Count > 0)
            {
                var delIds = toDelete.Select(c => c.Id).ToList();
                var inUseCampoIds = await _db.RespuestaValores
                    .Where(rv => delIds.Contains(rv.CampoId))
                    .Select(rv => rv.CampoId)
                    .Distinct()
                    .ToListAsync();

                if (inUseCampoIds.Count > 0)
                {
                    var inUseNames = enc.Campos
                        .Where(c => inUseCampoIds.Contains(c.Id))
                        .Select(c => string.IsNullOrWhiteSpace(c.Titulo) ? c.Nombre : c.Titulo)
                        .ToList();

                    ModelState.AddModelError(string.Empty,
                        "No se pueden eliminar los siguientes campos porque ya tienen respuestas: " +
                        string.Join(", ", inUseNames) +
                        ". Cree una nueva versión de la encuesta o elimine primero las respuestas asociadas.");

                    return View(model);
                }
            }

            // ---- APPLY CHANGES ----
            if (toDelete.Count > 0)
                _db.Campos.RemoveRange(toDelete);

            foreach (var cIn in incoming.Where(c => c.Id > 0))
            {
                if (!existingById.TryGetValue(cIn.Id, out var cDb)) continue;
                cDb.Nombre = cIn.Nombre;
                cDb.Titulo = cIn.Titulo;
                cDb.Tipo = cIn.Tipo;
                cDb.EsRequerido = cIn.EsRequerido;
                cDb.Orden = cIn.Orden;  // <-- keep posted order
            }

            foreach (var cIn in incoming.Where(c => c.Id == 0))
            {
                cIn.EncuestaId = enc.Id;
                _db.Campos.Add(cIn);     // cIn.Orden comes from the form
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = enc.Id });
        }




        // using Microsoft.EntityFrameworkCore;
        // using DevelPrueba.Models;

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var enc = await _db.Encuestas
                .Include(e => e.Campos)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (enc == null) return NotFound();

            var respuestasCount = await _db.Respuestas
                .CountAsync(r => r.EncuestaId == id);

            ViewBag.RespuestasCount = respuestasCount;
            ViewBag.LinkPublico = Url.Action("Fill", "Public", new { slug = enc.SlugPublico }, Request.Scheme);

            return View(enc);
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enc = await _db.Encuestas.FirstOrDefaultAsync(e => e.Id == id);
            if (enc == null) return NotFound();

            // ❗ Prevent deleting surveys that already have respuestas
            var respuestasCount = await _db.Respuestas.CountAsync(r => r.EncuestaId == id);
            if (respuestasCount > 0)
            {
                TempData["Error"] = "No se puede eliminar una encuesta que ya tiene respuestas. " +
                                    "Cree una nueva versión o elimine primero las respuestas.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _db.Encuestas.Remove(enc);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Encuesta eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Details(int id)
        {
            var enc = await _db.Encuestas.Include(e => e.Campos).FirstOrDefaultAsync(e => e.Id == id);
            if (enc == null) return NotFound();
            ViewBag.LinkPublico = Url.Action("Fill", "Public", new { slug = enc.SlugPublico }, Request.Scheme);
            return View(enc);
        }


        [HttpGet]
        public async Task<IActionResult> Duplicate(int id)
        {
            var src = await _db.Encuestas
                .Include(e => e.Campos)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (src == null) return NotFound();

            var copy = new Encuesta
            {
                Nombre = src.Nombre + " (Copia)",
                Descripcion = src.Descripcion,
                Activa = false,
                Campos = src.Campos
                    .OrderBy(c => c.Orden)
                    .Select((c, idx) => new Campo
                    {
                        // Id = 0, no EncuestaId aún
                        Nombre = c.Nombre,
                        Titulo = c.Titulo,
                        EsRequerido = c.EsRequerido,
                        Tipo = c.Tipo,
                        Orden = idx
                    })
                    .ToList()
            };

            // Render the Create view with the prefilled model (no save yet)
            return View("Create", copy);
        }

    }
}