using MTGWebUI.Models;

namespace MTGWebUI.Interfaces
{
    public interface IEmployeeService
    {
        Task<HttpResponseMessage> AddEmployeeAsync(CreateEmployeeDto createEmployeeDto);
        Task<HttpResponseMessage> Cancel();
        Task<HttpResponseMessage> DeleteAsync(Guid id);
        Task<HttpResponseMessage> EditEmployeeAsync(Guid id, UpdateEmployeeDto updateEmployeeDto);
        Task<EmployeeVM> GetEmployeeByIdAsync(Guid id);
        Task<IEnumerable<EmployeeVM>> GetEmployeesAsync();
        Task<IEnumerable<EmployeeVM>> GetPendingChanges();
        Task<HttpResponseMessage> Save();
    }
}