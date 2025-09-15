using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunTech.Data;
using static SunTech.Models.DatabaseModel;

namespace SunTech.Controllers
{
    public class DispatchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DispatchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Dispatch/Index

        public async Task<IActionResult> Index()
        {
            var dispatches = await _context.Dispatches
                .Include(d => d.Product)
                .OrderByDescending(d => d.DateDispatched)  // Sort by most recent first
                .ToListAsync();
            return View(dispatches);
        }

        // GET: Dispatch/Create
        public IActionResult Create(int productId)
        {
            ViewBag.ProductId = productId;
            var product = _context.Products.Find(productId);
            if (product == null)
            {
                return NotFound();
            }
            return View();
        }
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(
    int productId,
    bool isWaste,
    string wasteReason,
    [Bind("Quantity,CustomerId")] Dispatch dispatch)
{
    // LOG what was received
    Console.WriteLine($"POST Create: productId={productId}, isWaste={isWaste}, wasteReason='{wasteReason}', qty={dispatch?.Quantity}");

    if (!ModelState.IsValid)
    {
        Console.WriteLine("ModelState is invalid");
        ViewBag.ProductId = productId;
        return View(dispatch);
    }

    var product = await _context.Products.FindAsync(productId);
    if (product == null)
    {
        Console.WriteLine($"Product {productId} not found");
        return NotFound();
    }

    Console.WriteLine($"Product found: {product.Name}, Current stock: {product.Quantity}");

    if (dispatch.Quantity <= 0)
    {
        ModelState.AddModelError(nameof(dispatch.Quantity), "Quantity must be greater than zero.");
    }

    if (isWaste && string.IsNullOrWhiteSpace(wasteReason))
    {
        ModelState.AddModelError("WasteReason", "Waste reason is required when marking as waste.");
    }

    if (!ModelState.IsValid)
    {
        Console.WriteLine("Validation failed");
        ViewBag.ProductId = productId;
        return View(dispatch);
    }

    if (product.Quantity < dispatch.Quantity)
    {
        Console.WriteLine($"Not enough stock. Need: {dispatch.Quantity}, Have: {product.Quantity}");
        ModelState.AddModelError(string.Empty, "Not enough stock to remove.");
        ViewBag.ProductId = productId;
        return View(dispatch);
    }

    // Decrement stock
    product.Quantity -= dispatch.Quantity;
    _context.Update(product);
    Console.WriteLine($"Stock updated to: {product.Quantity}");

    try
    {
        if (isWaste)
        {
            Console.WriteLine("Creating waste record...");
            var waste = new Waste
            {
                ProductId = productId,
                Quantity = dispatch.Quantity,
                Reason = wasteReason,
                DateLogged = DateTime.UtcNow
            };
            _context.Wastes.Add(waste);
            await _context.SaveChangesAsync();
            Console.WriteLine("Waste record saved successfully");
            return RedirectToAction("Index", "Waste");
        }
        else
        {
            Console.WriteLine("Creating dispatch record...");
            dispatch.ProductId = productId;
            dispatch.DateDispatched = System.DateTime.Now;
            _context.Dispatches.Add(dispatch);
            await _context.SaveChangesAsync();
            Console.WriteLine("Dispatch record saved successfully");
            return RedirectToAction("Index", "Products");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR saving changes: {ex.Message}");
        Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
        ModelState.AddModelError(string.Empty, $"Database error: {ex.Message}");
        ViewBag.ProductId = productId;
        return View(dispatch);
    }
}

    }
}
