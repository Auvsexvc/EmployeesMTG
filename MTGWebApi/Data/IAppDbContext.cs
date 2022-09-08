using MTGWebApi.Entities;

namespace MTGWebApi.Data
{
    public interface IAppDbContext
    {
        Task AddAsync(Employee employee);
        Task CancelChangesAsync();
        Task DeleteAsync(Employee employee);
        Task<List<Employee>> GetEmployeesAsync();
        Task<IEnumerable<Employee>> PendingChangesAsync();
        Task SaveChangesAsync();
        Task UpdateAsync(Employee employee);
    }
}