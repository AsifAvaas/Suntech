using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunTech.Data;
using static SunTech.Models.DatabaseModel;

namespace SunTech.Controllers
{
    public class WasteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WasteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Waste/Index
        public async Task<IActionResult> Index()
        {
            var wastes = await _context.Wastes
                // .Include(w => w.Product) // optional if you decide to show product info
                .ToListAsync();
            return View(wastes);
        }
    }
}
