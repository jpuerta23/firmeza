using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using AdminRazer.Data;
using AdminRazer.Models;
using AdminRazer.ViewModels;
using System.Reflection; // added for reflection
using System.Globalization;
using System.Text;
using System.Diagnostics;

namespace AdminRazer.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProductoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Producto
        public IActionResult Index()
        {
            var productos = _context.Productos.ToList();
            return View(productos);
        }

        // POST: Producto/CargarExcel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CargarExcel(IFormFile? archivoExcel)
        {
            if (archivoExcel == null || archivoExcel.Length == 0)
            {
                TempData["Error"] = "Debe seleccionar un archivo Excel o CSV válido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Intentar configurar la licencia de EPPlus de forma compatible con EPPlus 8+ y versiones anteriores
                try
                {
                    // EPPlus 8+ recomienda usar la propiedad estática ExcelPackage.License
                    var licenseProp = typeof(ExcelPackage).GetProperty("License", BindingFlags.Static | BindingFlags.Public);
                    if (licenseProp != null)
                    {
                        var enumType = licenseProp.PropertyType;
                        object? enumValue = null;

                        // Intentar varios nombres comunes para el valor de licencia 'NonCommercial'
                        string[] candidates = new[] { "NonCommercial", "NonCommercialLicense", "NonCommercialUsage" };
                        foreach (var c in candidates)
                        {
                            try
                            {
                                enumValue = Enum.Parse(enumType, c);
                                break;
                            }
                            catch
                            {
                                // ignorar
                            }
                        }

                        if (enumValue != null)
                        {
                            licenseProp.SetValue(null, enumValue);
                        }
                    }

                    // Fallback para versiones anteriores que usan LicenseContext (evitar referencia directa porque está obsoleta)
                    var licenseContextType = typeof(ExcelPackage).Assembly.GetType("OfficeOpenXml.LicenseContext");
                    if (licenseContextType != null)
                    {
                        // buscar la propiedad o campo estático 'LicenseContext' en ExcelPackage
                        var lcProp = typeof(ExcelPackage).GetProperty("LicenseContext", BindingFlags.Static | BindingFlags.Public);
                        var lcField = typeof(ExcelPackage).GetField("LicenseContext", BindingFlags.Static | BindingFlags.Public);
                        try
                        {
                            object? nonCommercialValue = null;
                            try
                            {
                                nonCommercialValue = Enum.Parse(licenseContextType, "NonCommercial");
                            }
                            catch (Exception innerEx)
                            {
                                // intentar otros nombres comunes
                                foreach (var name in new[] { "NonCommercial", "NonCommercialUsage" })
                                {
                                    try
                                    {
                                        nonCommercialValue = Enum.Parse(licenseContextType, name);
                                        break;
                                    }
                                    catch (Exception ex2)
                                    {
                                        Debug.WriteLine($"Intento de parse '{name}' falló: {ex2.Message}");
                                    }
                                }
                                // Registrar el error inicial de parse para diagnóstico
                                Debug.WriteLine($"Error al parsear licencia inicial: {innerEx.Message}");
                            }

                            if (nonCommercialValue != null)
                            {
                                if (lcProp != null && lcProp.CanWrite)
                                {
                                    lcProp.SetValue(null, nonCommercialValue);
                                }
                                else if (lcField != null)
                                {
                                    lcField.SetValue(null, nonCommercialValue);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error al intentar setear LicenseContext: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Registrar y continuar; si es crítico EPPlus lanzará su propia excepción en uso
                    Debug.WriteLine($"Error al configurar licencia EPPlus (ignored): {ex.Message}");
                }

                // Detectar tipo de archivo por extensión
                // Tras la comprobación inicial, podemos usar la variable directamente (ya validada)
                var archivo = archivoExcel;
                var fileName = archivo.FileName;
                var ext = Path.GetExtension(fileName).ToLowerInvariant();

                int productosAgregados = 0;

                if (ext == ".csv")
                {
                    // Procesar CSV
                    using var ms = new MemoryStream();
                    await archivo.CopyToAsync(ms);
                    ms.Position = 0;
                    using var reader = new StreamReader(ms, Encoding.UTF8);
                    _ = await reader.ReadLineAsync(); // leer encabezados y descartarlos
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var parts = line.Split(',');
                        var nombre = parts.Length > 0 ? parts[0].Trim() : string.Empty;
                        var categoria = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                        var precioTexto = parts.Length > 2 ? parts[2].Trim() : "0";
                        var stockTexto = parts.Length > 3 ? parts[3].Trim() : "0";

                        if (string.IsNullOrWhiteSpace(nombre)) continue;

                        if (!_context.Productos.Any(p => p.Nombre == nombre))
                        {
                            var producto = new Producto
                            {
                                Nombre = nombre,
                                Categoria = categoria,
                                Precio = decimal.TryParse(precioTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out var pr) ? pr : 0,
                                Stock = int.TryParse(stockTexto, out var st) ? st : 0
                            };
                            _context.Productos.Add(producto);
                            productosAgregados++;
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Se cargaron {productosAgregados} productos desde CSV correctamente.";
                    return RedirectToAction(nameof(Index));
                }

                // Si no es CSV, intentar como Excel con EPPlus
                using var stream = new MemoryStream();
                await archivo.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var hoja = package.Workbook.Worksheets.FirstOrDefault();

                if (hoja == null)
                {
                    TempData["Error"] = "No se encontró ninguna hoja en el archivo Excel.";
                    return RedirectToAction(nameof(Index));
                }

                int fila = 2; // La fila 1 contiene los encabezados
                while (hoja.Cells[fila, 1].Value != null)
                {
                    var nombre = hoja.Cells[fila, 1].Text.Trim();
                    var categoria = hoja.Cells[fila, 2].Text.Trim();
                    var precioTexto = hoja.Cells[fila, 3].Text;
                    var stockTexto = hoja.Cells[fila, 4].Text;

                    if (string.IsNullOrWhiteSpace(nombre))
                    {
                        fila++;
                        continue; // Omitir filas vacías
                    }

                    if (!_context.Productos.Any(p => p.Nombre == nombre))
                    {
                        var producto = new Producto
                        {
                            Nombre = nombre,
                            Categoria = categoria,
                            Precio = decimal.TryParse(precioTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) ? precio : 0,
                            Stock = int.TryParse(stockTexto, out var stock) ? stock : 0
                        };

                        _context.Productos.Add(producto);
                        productosAgregados++;
                    }

                    fila++;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Se cargaron {productosAgregados} productos correctamente.";
            }
            catch (Exception ex)
            {
                // Registrar el error y mostrar mensaje de usuario
                Debug.WriteLine($"Error al procesar el archivo: {ex}");
                TempData["Error"] = $"Error al procesar el archivo: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Producto/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var producto = _context.Productos.Find(id);
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
        public IActionResult Create(ProductoCreateViewModel model)
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
                _context.Productos.Add(producto);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Producto/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var producto = _context.Productos.Find(id);
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
        public IActionResult Edit(ProductoEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var producto = _context.Productos.Find(model.Id);
                if (producto == null) return NotFound();

                // Mapear sólo las propiedades permitidas
                producto.Nombre = model.Nombre;
                producto.Categoria = model.Categoria;
                producto.Precio = model.Precio;
                producto.Stock = model.Stock;

                _context.Productos.Update(producto);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Producto/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var producto = _context.Productos.Find(id);
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
        public IActionResult DeleteConfirmed(int id)
        {
            // El controlador ya protege con [Authorize(Roles = "Administrador")].
            // Hacemos una comprobación de seguridad adicional por si acaso.
            if (!User.IsInRole("Administrador"))
            {
                return RedirectToAction("Index", "Home");
            }

            var producto = _context.Productos.Find(id);
            if (producto == null) return NotFound();

            _context.Productos.Remove(producto);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
}
