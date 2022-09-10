using MTGWebApi.Models;

namespace MTGWebApi.Interfaces
{
    public interface IEmployeeService
    {
        Task CancelChangesAsync();
        Task<Guid> CreateAsync(CreateEmployeeDto dto);

        Task DeleteAsync(Guid id);

        Task<IEnumerable<EmployeeVM>> GetAllAsync(string order, string searchString, int? pageNumber, int pageSize);

        Task<EmployeeVM> GetByIdAsync(Guid id);

        Task<IEnumerable<EmployeeVM>> GetPendingChangesAsync();

        Task SaveChangesAsync();

        Task UpdateAsync(Guid id, UpdateEmployeeDto dto);
    }
}