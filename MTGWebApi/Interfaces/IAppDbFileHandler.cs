using MTGWebApi.Entities;

namespace MTGWebApi.Interfaces
{
    public interface IAppDbFileHandler
    {
        Task AppendToFile<T>(T obj, string file);

        Task CommitOperationToFile(Employee employee, string file);

        Task CreateFile(string file);

        Task DeleteFile(string file);

        Task<IEnumerable<Employee>> GetEmployeesFromFileAsync(string file);

        string[] LineRemover(string[] lines, int[] lineNumbers);

        bool DoesFileExists(string file);
    }
}