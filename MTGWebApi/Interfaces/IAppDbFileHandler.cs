using MTGWebApi.Entities;

namespace MTGWebApi.Interfaces
{
    public interface IAppDbFileHandler
    {
        Task AppendToFile<T>(T obj, string file);

        Task CommitOperationToFile(Employee employee, string file, bool persist = false);

        Task CreateFile(string file);

        Task DeleteFile(string file);

        Task<IEnumerable<Employee>> GetEmployeesFromFileAsync(string file);

        bool DoesFileExists(string file);
    }
}