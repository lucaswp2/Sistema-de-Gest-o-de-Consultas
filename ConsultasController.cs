using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UVVConsultas.Data;
using UVVConsultas.Models;

namespace UVVConsultas.Controllers
{
    // Apenas usuários autenticados podem acessar qualquer ação deste controller
    [Authorize]
    public class ConsultasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConsultasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Recupera o Id do usuário logado a partir do Claim gravado no login
        private int UsuarioLogadoId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: /Consultas
        public async Task<IActionResult> Index()
        {
            var consultas = await _context.Consultas
                .Where(c => c.UsuarioId == UsuarioLogadoId)
                .OrderBy(c => c.DataHora)
                .ToListAsync();

            return View(consultas);
        }

        // GET: /Consultas/Create
        public IActionResult Create()
        {
            return View(new Consulta { DataHora = DateTime.Now });
        }

        // POST: /Consultas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Especialidade,DataHora,Descricao")] Consulta consulta)
        {
            if (!ModelState.IsValid)
            {
                return View(consulta);
            }

            consulta.UsuarioId = UsuarioLogadoId;

            _context.Add(consulta);
            await _context.SaveChangesAsync();

            TempData["Mensagem"] = "Consulta cadastrada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Consultas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var consulta = await _context.Consultas
                .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioLogadoId);

            if (consulta == null) return NotFound();

            return View(consulta);
        }

        // POST: /Consultas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Especialidade,DataHora,Descricao")] Consulta consulta)
        {
            if (id != consulta.Id) return NotFound();

            // Garante que o usuário só edite consultas que pertencem a ele
            bool pertenceAoUsuario = await _context.Consultas
                .AsNoTracking()
                .AnyAsync(c => c.Id == id && c.UsuarioId == UsuarioLogadoId);

            if (!pertenceAoUsuario) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(consulta);
            }

            consulta.UsuarioId = UsuarioLogadoId;

            try
            {
                _context.Update(consulta);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                bool existe = await _context.Consultas.AnyAsync(c => c.Id == consulta.Id);
                if (!existe) return NotFound();
                throw;
            }

            TempData["Mensagem"] = "Consulta atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Consultas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var consulta = await _context.Consultas
                .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioLogadoId);

            if (consulta == null) return NotFound();

            return View(consulta);
        }

        // POST: /Consultas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var consulta = await _context.Consultas
                .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioLogadoId);

            if (consulta != null)
            {
                _context.Consultas.Remove(consulta);
                await _context.SaveChangesAsync();
                TempData["Mensagem"] = "Consulta excluída com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
