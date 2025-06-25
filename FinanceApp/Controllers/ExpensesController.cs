using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using FinanceApp.Data;
using FinanceApp.Data.Service;
using FinanceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly IExpensesService _expensesService;
        public ExpensesController(IExpensesService expensesService)
        {
            _expensesService = expensesService;
        }
        // Index y Search
        public async Task<IActionResult> Index(string? searchTerm)
        {
            IEnumerable<Expense> expenses;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                expenses = await _expensesService.SearchByDescription(searchTerm);
            }
            else
            {
                expenses = await _expensesService.GetAll();
            }

            return View(expenses);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Expense expense)
        {
            if(ModelState.IsValid)
            {
                await _expensesService.Add(expense);

                return RedirectToAction("Index");
            }

            return View(expense);
        }

        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var expense = await _expensesService.GetById(id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        //GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _expensesService.GetById(id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        //POST: Expenses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expense expense)
        {
            if (id != expense.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _expensesService.Update(expense);
                }
                catch (Exception)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(expense); // Si algo falla, se vuelve a mostrar el formulario con errores.
        }
        
        //GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _expensesService.GetById(id);
            if (expense == null) return NotFound();

            return View(expense); 
        }

        //POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _expensesService.Delete(id);
            return RedirectToAction(nameof(Index)); 
        }

        // GetChats (Graficas)
        public IActionResult GetChart()
        {
            var data = _expensesService.GetChartData();
            return Json(data);
        }

        // Export Data in Excel
        public async Task<IActionResult> ExportToExcel()
        {
            var expenses = await _expensesService.GetAll();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Expenses");

            // Header
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Description";
            worksheet.Cell(1, 3).Value = "Amount";
            worksheet.Cell(1, 4).Value = "Category";
            worksheet.Cell(1, 5).Value = "Date";

            // Body
            int row = 2;
            foreach (var exp in expenses)
            {
                worksheet.Cell(row, 1).Value = exp.Id;
                worksheet.Cell(row, 2).Value = exp.Description;
                worksheet.Cell(row, 3).Value = exp.Amount;
                worksheet.Cell(row, 4).Value = exp.Category;
                worksheet.Cell(row, 5).Value = exp.Date.ToString("yyyy-MM-dd HH:mm:ss");
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Expenses.xlsx");
        }

        // Import Excel a View App
        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ImportError"] = "No se ha cargado ningún archivo.";
                return RedirectToAction("Index");
            }

            var errors = new List<string>();
            var expenses = new List<Expense>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

            int rowNumber = 2; // porque saltamos encabezado

            foreach (var row in rows)
            {
                try
                {
                    var description = row.Cell(1).GetValue<string>();
                    var amountStr = row.Cell(2).GetValue<string>();
                    var category = row.Cell(3).GetValue<string>();

                    if (!double.TryParse(amountStr, out var amount))
                    {
                        errors.Add($"Fila {rowNumber}: 'Amount' no es válido ({amountStr}).");
                    }
                    else
                    {
                        var expense = new Expense
                        {
                            Description = description,
                            Amount = amount,
                            Category = category,
                            Date = DateTime.Now
                        };
                        expenses.Add(expense);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Fila {rowNumber}: Error inesperado - {ex.Message}");
                }

                rowNumber++;
            }

            foreach (var exp in expenses)
            {
                await _expensesService.Add(exp);
            }

            if (errors.Any())
                TempData["ImportError"] = string.Join("<br>", errors);
            else
                TempData["ImportSuccess"] = $"{expenses.Count} registros importados correctamente.";

            return RedirectToAction("Index");
        }


    }
}
