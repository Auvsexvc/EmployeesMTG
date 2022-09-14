using MTGWebApi.Entities;
using MTGWebApi.Enums;
using MTGWebApi.Extensions;
using MTGWebApi.Interfaces;

namespace MTGWebApi.Data
{
    public class AppDbFileHandler : IAppDbFileHandler
    {
        public async Task<IEnumerable<Employee>> GetEmployeesFromFileAsync(string file)
        {
            if (!DoesFileExists(file))
            {
                return Enumerable.Empty<Employee>();
            }

            List<Employee> employees = new();

            using (var streamReader = new StreamReader(file))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = await streamReader.ReadLineAsync();
                    if (!string.IsNullOrEmpty(line))
                    {
                        var employee = line.ConvertToEmployee();
                        await Task.Run(() => employees.Add(employee));
                    }
                }
            }

            return employees;
        }

        public async Task AppendToFile<T>(T obj, string file)
        {
            if (obj is not null)
            {
                using var streamWriter = new StreamWriter(file, true);
                var line = string.Join(';', obj.GetType().GetProperties().Select(p => p.GetValue(obj)));
                await streamWriter.WriteLineAsync(line);
                await streamWriter.FlushAsync();
            }
        }

        public async Task CommitOperationToFile(Employee employee, string file, bool persist = false)
        {
            if (!DoesFileExists(file))
            {
                await CreateFile(file);
            }

            if (employee.State != Operation.Create)
            {
                var lines = await File.ReadAllLinesAsync(file);
                var lineNumbersToDelete = Enumerable.Range(0, lines.Length).Where(x => employee.Id.ToString() == lines[x].Split(';')[0]).ToArray();
                if (employee.State == Operation.Update && lineNumbersToDelete.Any(x => Enum.Parse<Operation>(lines[x].Split(';')[10]) == Operation.Create))
                {
                    employee.State = Operation.Create;
                }
                var newLines = LineRemover(lines, lineNumbersToDelete);
                await File.WriteAllLinesAsync(file, newLines);
            }

            if (employee.State != Operation.Delete)
            {
                if (persist)
                {
                    employee.State = Operation.Persist;
                }
                await AppendToFile(employee, file);
            }
        }

        public async Task CreateFile(string file)
        {
            await Task.Run(() =>
            {
                using var fileStream = new FileStream(file, FileMode.CreateNew);
            });
        }

        public bool DoesFileExists(string file)
        {
            return File.Exists(file);
        }

        public async Task DeleteFile(string file)
        {
            await Task.Run(() => File.Delete(file));
        }

        private static string[] LineRemover(string[] lines, int[] lineNumbers)
        {
            List<string> arrLine = lines.ToList();
            foreach (var lineNumber in lineNumbers.Reverse())
            {
                arrLine.RemoveAt(lineNumber);
            }

            return arrLine.ToArray();
        }
    }
}