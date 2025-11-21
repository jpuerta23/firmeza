using AdminRazer.Data;
using AdminRazer.Models;
using OfficeOpenXml;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdminRazer.Services
{
    public class ExcelImportService : IExcelImportService
    {
        private readonly ApplicationDbContext _context;

        public ExcelImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(int count, string message)> ImportarProductosAsync(Stream fileStream)
        {
            int productosAgregados = 0;

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage(fileStream);
                var hoja = package.Workbook.Worksheets.FirstOrDefault();

                if (hoja == null)
                {
                    return (0, "No se encontró ninguna hoja en el archivo Excel.");
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
                        continue;
                    }

                    // Lógica "desnormalizada": Si la categoría viene como texto, la guardamos tal cual.
                    // Si en el futuro Categoria fuese una entidad, aquí buscaríamos por nombre o crearíamos una nueva.
                    
                    if (!_context.Productos.Any(p => p.Nombre == nombre))
                    {
                        var producto = new Producto
                        {
                            Nombre = nombre,
                            Categoria = categoria, // Se acepta cualquier valor de texto
                            Precio = decimal.TryParse(precioTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) ? precio : 0,
                            Stock = int.TryParse(stockTexto, out var stock) ? stock : 0
                        };

                        _context.Productos.Add(producto);
                        productosAgregados++;
                    }

                    fila++;
                }

                await _context.SaveChangesAsync();
                return (productosAgregados, $"Se cargaron {productosAgregados} productos correctamente.");
            }
            catch (Exception ex)
            {
                return (0, $"Error al procesar el archivo: {ex.Message}");
            }
        }
    }
}
