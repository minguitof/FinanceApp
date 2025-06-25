using FinanceApp.Models;

namespace FinanceApp.Data.Service
{
    public interface IExpensesService
    {
        Task<IEnumerable<Expense>> GetAll();
        Task Add(Expense expense);
        Task<Expense?> GetById(int id);
        Task Update(Expense expense);
        Task Delete(int id);
        Task<IEnumerable<Expense>> SearchByDescription(string searchTerm);
        IQueryable GetChartData();

    }
}
