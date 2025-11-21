#nullable disable
// csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminRazer.Data;
using Microsoft.AspNetCore.Authorization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Globalization;
using AdminRazer.ViewModels;
using System.IO;

namespace AdminRazer.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ventas
        public async Task<IActionResult> Index()
        {
            // Incluir Cliente y Detalles para que las vistas puedan acceder a sus propiedades
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                .ToListAsync();

            return View(ventas);
        }

        // GET: Ventas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null) return NotFound();

            // Mapear a ViewModel
            var vm = new VentaViewModel
            {
                Id = venta.Id,
                Fecha = venta.Fecha,
                ClienteNombre = venta.Cliente.Nombre,
                MetodoPago = venta.MetodoPago,
                Total = venta.Total,
                Detalles = venta.Detalles.Select(d => new DetalleVentaViewModel
                {
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.Producto.Nombre,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList()
            };

            // Especificar la vista y pasar el VM
            return View("Details", vm);
        }

        // GET: Ventas/Eliminar/5
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null) return NotFound();

            var vm = new VentaViewModel
            {
                Id = venta.Id,
                Fecha = venta.Fecha,
                ClienteNombre = venta.Cliente.Nombre,
                MetodoPago = venta.MetodoPago,
                Total = venta.Total,
                Detalles = venta.Detalles.Select(d => new DetalleVentaViewModel
                {
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.Producto.Nombre,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList()
            };

            return View("Delete", vm);
        }

        // POST: Ventas/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta != null)
            {
                _context.Ventas.Remove(venta);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // -------------------------------------------------------------------
        // Nuevas acciones para descargar PDF usando QuestPDF
        // -------------------------------------------------------------------

        // GET: Ventas/DescargarTodas
        [HttpGet]
        public async Task<IActionResult> DescargarTodas()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .OrderBy(v => v.Fecha)
                .ToListAsync();

            var pdfBytes = BuildVentasPdf(ventas, null);
            var fileName = $"ventas_todas_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // GET: Ventas/DescargarPorDia?fecha=2025-11-05
        [HttpGet]
        public async Task<IActionResult> DescargarPorDia(string fecha)
        {
            if (string.IsNullOrWhiteSpace(fecha))
            {
                // Si no se proporciona fecha, devolver todas las ventas en PDF
                return RedirectToAction(nameof(DescargarTodas));
            }

            if (!DateTime.TryParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                // Intentar parsear formatos generales
                if (!DateTime.TryParse(fecha, out parsed))
                {
                    return BadRequest("Fecha inválida. Use el formato yyyy-MM-dd.");
                }
            }

            // Normalizar las fechas a UTC para evitar problemas con Npgsql/PostgreSQL y timestamp with time zone
            var inicioLocal = parsed.Date; // fecha sin hora
            var inicio = DateTime.SpecifyKind(inicioLocal, DateTimeKind.Utc);

            // Obtener ventas primero y filtrar en memoria para evitar pasar DateTime con Kind=Unspecified a Npgsql
            var ventasTodas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .OrderBy(v => v.Fecha)
                .ToListAsync();

            // Filtrar en memoria comparando fechas en UTC
            var ventas = ventasTodas
                .Where(v => v.Fecha.ToUniversalTime().Date == parsed.Date)
                .ToList();

            var pdfBytes = BuildVentasPdf(ventas, inicio);
            var fileName = $"ventas_{inicio:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // Método privado que usa QuestPDF para construir el PDF a partir de una lista de ventas
        private byte[] BuildVentasPdf(List<AdminRazer.Models.Venta> ventas, DateTime? fechaFiltro)
        {
            // La licencia de QuestPDF se configura en Program.cs al iniciar la aplicación.

            using var ms = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .PaddingBottom(10)
                        .Row(row =>
                        {
                            row.ConstantItem(60).AlignCenter().Text("Firmeza\nVentas").Bold();
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(fechaFiltro.HasValue ? $"Reporte de ventas - {fechaFiltro:yyyy-MM-dd}" : "Reporte de ventas - Todas").FontSize(14).Bold();
                                col.Item().Text($"Generado: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10);
                            });
                        });

                    page.Content().Column(col =>
                    {
                        col.Spacing(5);

                        if (ventas.Count == 0)
                        {
                            col.Item().Text("No hay ventas para el periodo seleccionado.");
                        }
                        else
                        {
                            // Cabecera de tabla
                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Id").Bold();
                                r.RelativeItem().Text("Fecha").Bold();
                                r.RelativeItem().Text("Cliente").Bold();
                                r.RelativeItem().Text("Total").AlignRight().Bold();
                            });

                            foreach (var v in ventas)
                            {
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(v.Id.ToString());
                                    r.RelativeItem().Text(v.Fecha.ToString("yyyy-MM-dd"));
                                    r.RelativeItem().Text(v.Cliente.Nombre);
                                    r.RelativeItem().Text(v.Total.ToString("C", CultureInfo.CurrentCulture)).AlignRight();
                                });

                                // Opcional: detalles de la venta
                                if (v.Detalles.Count > 0)
                                {
                                    col.Item().PaddingLeft(10).Column(detailsCol =>
                                    {
                                        detailsCol.Item().Text("Detalles:").Italic().FontSize(10);
                                        foreach (var d in v.Detalles)
                                        {
                                            var prod = d.Producto.Nombre;
                                            detailsCol.Item().Text($"- {prod} x{d.Cantidad} = {d.Subtotal.ToString("C", CultureInfo.CurrentCulture)}").FontSize(10);
                                        }
                                    });
                                }

                                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            }

                            // Totales generales
                            var suma = ventas.Sum(x => x.Total);
                            col.Item().AlignRight().Text($"Total general: {suma.ToString("C", CultureInfo.CurrentCulture)}").Bold();
                        }
                    });

                    page.Footer().AlignCenter().Text(x => {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            })
            .GeneratePdf(ms);

            return ms.ToArray();
        }


    }
}
