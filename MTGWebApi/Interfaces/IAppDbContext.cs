using MTGWebApi.Entities;

namespace MTGWebApi.Interfaces
{
    public interface IAppDbContext
    {
        Task AddAsync(Employee employee);
        Task CancelChangesAsync();
        Task DeleteAsync(Employee employee);
        Task<IEnumerable<Employee>> GetEmployeesAsync();
        Task<IEnumerable<Employee>> PendingChangesAsync();
        Task SaveChangesAsync();
        Task UpdateAsync(Employee employee);
    }
}