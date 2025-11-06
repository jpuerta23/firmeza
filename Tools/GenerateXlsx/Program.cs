using OfficeOpenXml;
using System.IO;
using System.Reflection;

// Establecer licencia compatible con EPPlus 8+ (ExcelPackage.License) mediante reflection
try
{
    var licenseProp = typeof(ExcelPackage).GetProperty("License", BindingFlags.Static | BindingFlags.Public);
    if (licenseProp != null)
    {
        var enumType = licenseProp.PropertyType;
        try
        {
            var enumValue = Enum.Parse(enumType, "NonCommercial");
            licenseProp.SetValue(null, enumValue);
        }
        catch { }
    }
    else
    {
        // Fallback (antiguo)
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }
}
catch
{
    // ignore
}

var path = Path.Combine(Directory.GetCurrentDirectory(), "test_productos.xlsx");
using var p = new ExcelPackage();
var ws = p.Workbook.Worksheets.Add("Productos");
ws.Cells[1,1].Value = "Nombre";
ws.Cells[1,2].Value = "Categoria";
ws.Cells[1,3].Value = "Precio";
ws.Cells[1,4].Value = "Stock";

ws.Cells[2,1].Value = "Producto_XLSX_Test";
ws.Cells[2,2].Value = "General";
ws.Cells[2,3].Value = 29.99;
ws.Cells[2,4].Value = 4;

p.SaveAs(new FileInfo(path));
Console.WriteLine(path);
