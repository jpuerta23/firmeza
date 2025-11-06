using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;

string filePath = Path.Combine(Directory.GetCurrentDirectory(), "test_productos.xlsx");
using (var spreadsheet = SpreadsheetDocument.Create(filePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
{
    var workbookPart = spreadsheet.AddWorkbookPart();
    workbookPart.Workbook = new Workbook();
    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
    var sheetData = new SheetData();
    worksheetPart.Worksheet = new Worksheet(sheetData);

    Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
    Sheet sheet = new Sheet() { Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Productos" };
    sheets.Append(sheet);

    // Header
    var headerRow = new Row();
    headerRow.Append(new Cell() { CellValue = new CellValue("Nombre"), DataType = CellValues.String });
    headerRow.Append(new Cell() { CellValue = new CellValue("Categoria"), DataType = CellValues.String });
    headerRow.Append(new Cell() { CellValue = new CellValue("Precio"), DataType = CellValues.String });
    headerRow.Append(new Cell() { CellValue = new CellValue("Stock"), DataType = CellValues.String });
    sheetData.AppendChild(headerRow);

    // Data row
    var dataRow = new Row();
    dataRow.Append(new Cell() { CellValue = new CellValue("Producto_XLSX_OpenXml_Test"), DataType = CellValues.String });
    dataRow.Append(new Cell() { CellValue = new CellValue("General"), DataType = CellValues.String });
    dataRow.Append(new Cell() { CellValue = new CellValue("39.95"), DataType = CellValues.String });
    dataRow.Append(new Cell() { CellValue = new CellValue("6"), DataType = CellValues.String });
    sheetData.AppendChild(dataRow);

    workbookPart.Workbook.Save();
}

Console.WriteLine(filePath);

