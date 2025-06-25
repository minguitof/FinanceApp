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
        public async Task<IActionResult> Index()
        {
            var expenses = await _expensesService.GetAll();
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



        public IActionResult GetChart()
        {
            var data = _expensesService.GetChartData();
            return Json(data);
        }
    }
}
