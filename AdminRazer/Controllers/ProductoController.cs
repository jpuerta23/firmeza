using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using AdminRazer.Models;
using AdminRazer.ViewModels;
using AdminRazer.Repositories.Interfaces;
using System.Reflection; // added for reflection
using System.Globalization;
using System.Text;
using System.Diagnostics;

namespace AdminRazer.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProductoController : Controller
    {
        private readonly IProductoRepository _productoRepository;
        private readonly Services.IExcelImportService _excelImportService;

        public ProductoController(IProductoRepository productoRepository, Services.IExcelImportService excelImportService)
        {
            _productoRepository = productoRepository;
            _excelImportService = excelImportService;
        }

        // GET: Producto
        public async Task<IActionResult> Index()
        {
            var productos = await _productoRepository.GetAllAsync();
            return View(productos);
        }

        // POST: Producto/CargarExcel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CargarExcel(IFormFile? archivoExcel)
        {
            if (archivoExcel == null || archivoExcel.Length == 0)
            {
                TempData["Error"] = "Debe seleccionar un archivo Excel válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var stream = archivoExcel.OpenReadStream();
                var (count, message) = await _excelImportService.ImportarProductosAsync(stream);

                if (count > 0)
                {
                    TempData["Success"] = message;
                }
                else
                {
                    TempData["Error"] = message;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error inesperado: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Producto/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _productoRepository.GetByIdAsync(id.Value);
            if (producto == null) return NotFound();

            // Mapear la entidad Producto al ProductoEditViewModel que espera la vista Details
            var vm = new ProductoEditViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Categoria = producto.Categoria,
                Precio = producto.Precio,
                Stock = producto.Stock
            };

            return View(vm);
        }

        // GET: Producto/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Producto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var producto = new Producto
                {
                    Nombre = model.Nombre,
                    Categoria = model.Categoria,
                    Precio = model.Precio,
                    Stock = model.Stock
                };
                await _productoRepository.AddAsync(producto);
                await _productoRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Producto/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _productoRepository.GetByIdAsync(id.Value);
            if (producto == null) return NotFound();

            var vm = new ProductoEditViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Categoria = producto.Categoria,
                Precio = producto.Precio,
                Stock = producto.Stock
            };

            return View(vm);
        }

        // POST: Producto/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductoEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var producto = await _productoRepository.GetByIdAsync(model.Id);
                if (producto == null) return NotFound();

                // Mapear sólo las propiedades permitidas
                producto.Nombre = model.Nombre;
                producto.Categoria = model.Categoria;
                producto.Precio = model.Precio;
                producto.Stock = model.Stock;

                _productoRepository.Update(producto);
                await _productoRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Producto/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _productoRepository.GetByIdAsync(id.Value);
            if (producto == null) return NotFound();

            var vm = new AdminRazer.ViewModels.ProductoEditViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Categoria = producto.Categoria,
                Precio = producto.Precio,
                Stock = producto.Stock
            };

            return View(vm);
        }

        // POST: Producto/DeleteConfirmed
        // POST: Producto/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // El controlador ya protege con [Authorize(Roles = "Administrador")].
            // Hacemos una comprobación de seguridad adicional por si acaso.
            if (!User.IsInRole("Administrador"))
            {
                return RedirectToAction("Index", "Home");
            }

            var producto = await _productoRepository.GetByIdAsync(id);
            if (producto == null) return NotFound();

            _productoRepository.Remove(producto);
            await _productoRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
