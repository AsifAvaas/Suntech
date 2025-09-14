using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunTech.Data;
using static SunTech.Models.DatabaseModel;

namespace SunTech.Controllers
{
    public class StorageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StorageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Storage/Index
        public async Task<IActionResult> Index()
        {
            // Get all products and create storage records if none exist
            await CreateDemoStorageData();
            
            // Fetch all storage records with product details
            var storages = await _context.Storages
                .Include(s => s.Product)
                .OrderBy(s => s.Location)
                .ToListAsync();
            
            return View(storages);
        }

        private async Task CreateDemoStorageData()
        {
            // Check if storage data already exists
            var existingStorages = await _context.Storages.CountAsync();
            if (existingStorages > 0)
            {
                return; // Storage data already exists
            }

            // Get all products
            var products = await _context.Products.ToListAsync();
            if (products.Count == 0)
            {
                return; // No products to assign
            }

            // Define 4 warehouse locations
            string[] locations = { "Warehouse-A", "Warehouse-B", "Warehouse-C", "Warehouse-D" };
            var random = new Random();

            // Randomly assign each product to a warehouse location
            foreach (var product in products)
            {
                var randomLocation = locations[random.Next(locations.Length)];
                var randomQuantity = random.Next(1, Math.Max(1, product.Quantity / 2)); // Random portion of product quantity

                var storage = new Storage
                {
                    ProductId = product.ProductId,
                    Location = randomLocation,
                    Quantity = randomQuantity
                };

                _context.Storages.Add(storage);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"Demo storage data created for {products.Count} products in 4 warehouses");
        }
    }
}
