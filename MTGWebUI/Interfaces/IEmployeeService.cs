using MTGWebUI.Models;

namespace MTGWebUI.Interfaces
{
    public interface IEmployeeService
    {
        Task<bool> AreTherePendingChanges();

        Task<HttpResponseMessage> AddEmployeeAsync(CreateEmployeeDto createEmployeeDto);

        Task<HttpResponseMessage> CancelAsync();

        Task<HttpResponseMessage> DeleteAsync(Guid id);

        Task<HttpResponseMessage> EditEmployeeAsync(Guid id, UpdateEmployeeDto updateEmployeeDto);

        Task<EmployeeVM> GetEmployeeByIdAsync(Guid id);

        Task<IEnumerable<EmployeeVM>> GetEmployeesAsync();

        Task<IEnumerable<EmployeeVM>> GetPendingChangesAsync();

        Task<HttpResponseMessage> SaveAsync();
    }
}