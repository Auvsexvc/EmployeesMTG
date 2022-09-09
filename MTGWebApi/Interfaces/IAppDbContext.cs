using MTGWebApi.Entities;

namespace MTGWebApi.Interfaces
{
    public interface IAppDbContext
    {
        Task StageAddAsync(Employee employee);
        Task CancelChangesAsync();
        Task StageDeleteAsync(Employee employee);
        Task<IEnumerable<Employee>> GetEmployeesAsync();
        Task<IEnumerable<Employee>> GetStagedChangesAsync();
        Task CommitChangesAsync();
        Task StageUpdateAsync(Employee employee);
    }
}