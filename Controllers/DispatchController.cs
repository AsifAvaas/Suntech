using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunTech.Data;
using SunTech.Models;  // Update to match your namespace
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
            // Retrieve the list of dispatches, including the related product information
            var dispatches = await _context.Dispatches
                                           .Include(d => d.Product) // Load product details related to the dispatch
                                           .ToListAsync();

            return View(dispatches); // Pass the dispatch list to the Index view
        }

        // GET: Dispatch/Create
        public IActionResult Create(int productId)
        {
            // Pass the productId to the view to use in the form
            ViewBag.ProductId = productId;
            var product = _context.Products.Find(productId);
            if (product == null)
            {
                return NotFound();
            }

            return View();
        }

        // POST: Dispatch/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, [Bind("Quantity,CustomerId")] Dispatch dispatch)
        {
            if (ModelState.IsValid)
            {
                // Find the product to remove stock
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound();
                }

                // Check if there's enough stock to remove
                if (product.Quantity >= dispatch.Quantity)
                {
                    // Decrease product quantity
                    product.Quantity -= dispatch.Quantity;
                    _context.Update(product);

                    // Set the dispatch details
                    dispatch.ProductId = productId;
                    dispatch.DateDispatched = System.DateTime.Now;

                    _context.Add(dispatch);
                    await _context.SaveChangesAsync();

                    // Redirect to the Products page
                    return RedirectToAction("Index", "Products");
                }
                else
                {
                    // Add model error if stock is insufficient
                    ModelState.AddModelError(string.Empty, "Not enough stock to remove.");
                }
            }

            // Return the same view with validation error if the model is invalid
            return View(dispatch);
        }
    }
}
