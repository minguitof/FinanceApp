using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Data.Service
{
    public class ExpensesService : IExpensesService
    {
        private readonly FinanceAppContext _context;
        public ExpensesService(FinanceAppContext context) 
        { 
            _context = context;
        }
        public async Task Add(Expense expense)
        {
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
        }
        //Obtener data
        public async Task<Expense?> GetById(int id)
        {
            return await _context.Expenses.FindAsync(id);
        }
        //Updata data
        public async Task Update(Expense expense)
        {
            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();
        }
        //Delete data
        public async Task Delete(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
            }
        }

        // get all the data
        public async Task<IEnumerable<Expense>> GetAll()
        {
            var expenses = await _context.Expenses.ToListAsync();
            return expenses;
        }

        // Chart.js - graphics
        public IQueryable GetChartData()
        {
            var data = _context.Expenses
                              .GroupBy(e => e.Category)
                              .Select(g => new
                              {
                                  Category = g.Key,
                                  Total = g.Sum(e => e.Amount)
                              });
            return data;
        }

        // Search
        public async Task<IEnumerable<Expense>> SearchByDescription(string searchTerm)
        {
            return await _context.Expenses
                                 .Where(e => e.Description.ToLower().Contains(searchTerm.ToLower()))
                                 .ToListAsync();
        }

    }
}
