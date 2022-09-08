using MTGWebApi.Entities;
using MTGWebApi.Enums;
using MTGWebApi.Interfaces;

namespace MTGWebApi.Data
{
    public class AppDbContext : IAppDbContext
    {
        protected readonly string _dbFullPath;
        protected readonly string _tempFullPath;

        public AppDbContext(IConfiguration configuration)
        {
            var dbFile = configuration.GetSection("DbInfo:DbFile").Get<string>();
            var tempFile = configuration.GetSection("DbInfo:TempFile").Get<string>();
            _dbFullPath = Path.Combine(Environment.CurrentDirectory, dbFile);
            _tempFullPath = Path.Combine(Environment.CurrentDirectory, tempFile);
        }

        public async Task AddAsync(Employee employee)
        {
            await SaveEmployeeToFile(employee, _tempFullPath);
        }

        public async Task DeleteAsync(Employee employee)
        {
            if (!File.Exists(_tempFullPath))
            {
                await CreateTempFile();
            }

            if ((await GetEmployeesFromFileAsync(_tempFullPath)).Any(x => x.Id == employee.Id))
            {
                var lines = await File.ReadAllLinesAsync(_tempFullPath);
                var lineNumbersToDelete = Enumerable.Range(0, lines.Length).Where(x => lines[x].Split(';')[0] == employee.Id.ToString()).ToArray();

                var newLines = LineRemover(lines, lineNumbersToDelete);
                await File.WriteAllLinesAsync(_tempFullPath, newLines);
            }

            if ((await GetEmployeesFromFileAsync(_dbFullPath)).Any(x => x.Id == employee.Id))
            {
                await SaveEmployeeToFile(employee, _tempFullPath);
            }
        }

        public async Task UpdateAsync(Employee employee)
        {
            if (!File.Exists(_tempFullPath))
            {
                await CreateTempFile();
            }

            if ((await GetEmployeesFromFileAsync(_tempFullPath)).Any(x => x.Id == employee.Id && x.State != Operation.Delete))
            {
                var lines = await File.ReadAllLinesAsync(_tempFullPath);
                var lineNumbersToDelete = Enumerable.Range(0, lines.Length).Where(x => lines[x].Split(';')[0] == employee.Id.ToString() && Enum.Parse<Operation>(lines[x].Split(';')[10]) != Operation.Delete).ToArray();
                if (lineNumbersToDelete.Any(x => Enum.Parse<Operation>(lines[x].Split(';')[10]) == Operation.Create))
                {
                    employee.State = Operation.Create;
                }
                var newLines = LineRemover(lines, lineNumbersToDelete);
                await File.WriteAllLinesAsync(_tempFullPath, newLines);
            }

            await SaveEmployeeToFile(employee, _tempFullPath);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesAsync()
        {
            if (!File.Exists(_tempFullPath))
            {
                return await GetEmployeesFromFileAsync(_dbFullPath);
            }

            return await DbResultsWithTemp();
        }

        public async Task CancelChangesAsync()
        {
            if (File.Exists(_tempFullPath))
            {
                await Task.Factory.StartNew(() => File.Delete(_tempFullPath));
            }
        }

        public async Task<IEnumerable<Employee>> PendingChangesAsync()
        {
            if (!File.Exists(_tempFullPath))
            {
                return Enumerable.Empty<Employee>();
            }

            return await GetEmployeesFromFileAsync(_tempFullPath);
        }

        public async Task SaveChangesAsync()
        {
            if (File.Exists(_tempFullPath))
            {
                await CommitAddAsync();
                await CommitDeleteAsync();
                await CommitUpdateAsync();
                await Task.Factory.StartNew(() => File.Delete(_tempFullPath));
            }
        }

        private static async Task<List<Employee>> GetEmployeesFromFileAsync(string fileName)
        {
            List<Employee> employees = new();

            using (var streamReader = new StreamReader(fileName))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = await streamReader.ReadLineAsync();
                    if (!string.IsNullOrEmpty(line))
                    {
                        var employee = ConvertToEmployee(line);
                        await Task.Factory.StartNew(() => employees.Add(employee));
                    }
                }
            }

            return employees;
        }

        private static Employee ConvertToEmployee(string line)
        {
            var values = line.Split(';');

            return new Employee()
            {
                Id = Guid.Parse(values[0]),
                FirstName = values[1],
                LastName = values[2],
                StreetName = values[3],
                HouseNumber = values[4],
                ApartmentNumber = values[5],
                PostalCode = values[6],
                Town = values[7],
                PhoneNumber = values[8],
                DateOfBirth = DateTime.Parse(values[9]),
                State = Enum.Parse<Operation>(values[10])
            };
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

        private async Task CommitAddAsync()
        {
            var x = await GetEmployeesFromFileAsync(_tempFullPath);
            var toAdd = x.Where(x => x.State == Operation.Create);
            foreach (var employee in toAdd)
            {
                await SaveEmployeeToFile(employee, _dbFullPath);
            }
        }

        private async Task CommitDeleteAsync()
        {
            var x = await GetEmployeesFromFileAsync(_tempFullPath);
            var toDelete = x.Where(x => x.State == Operation.Delete);

            var lines = await File.ReadAllLinesAsync(_dbFullPath);
            var lineNumbersToDelete = Enumerable.Range(0, lines.Length).Where(x => toDelete.Select(x => x.Id.ToString()).Contains(lines[x].Split(';')[0])).ToArray();
            var newLines = LineRemover(lines, lineNumbersToDelete);
            await File.WriteAllLinesAsync(_dbFullPath, newLines);
        }

        private async Task CommitUpdateAsync()
        {
            var x = await GetEmployeesFromFileAsync(_tempFullPath);
            var toUpdate = x.Where(x => x.State == Operation.Update);

            var lines = await File.ReadAllLinesAsync(_dbFullPath);
            var lineNumbersToUpdate = Enumerable.Range(0, lines.Length).Where(x => toUpdate.Select(x => x.Id.ToString()).Contains(lines[x].Split(';')[0])).ToArray();
            var newLines = LineRemover(lines, lineNumbersToUpdate);
            await File.WriteAllLinesAsync(_dbFullPath, newLines);

            foreach (var employee in toUpdate)
            {
                await SaveEmployeeToFile(employee, _dbFullPath);
            }
        }

        private static async Task SaveEmployeeToFile(Employee employee, string fileName)
        {
            using var streamWriter = new StreamWriter(fileName, true);
            var line = string.Join(';', employee.GetType().GetProperties().Select(p => p.GetValue(employee)));
            await streamWriter.WriteLineAsync(line);
            await streamWriter.FlushAsync();
        }

        private async Task CreateTempFile()
        {
            await Task.Run(() =>
            {
                using var fileStream = new FileStream(_tempFullPath, FileMode.CreateNew);
            });
        }

        private async Task<IEnumerable<Employee>> DbResultsWithTemp()
        {
            var dbEmployees = await GetEmployeesFromFileAsync(_dbFullPath);
            var tempEmployees = await GetEmployeesFromFileAsync(_tempFullPath);

            var tempEmployeesFiltered = tempEmployees.Where(e => e.State != Operation.Delete).GroupBy(x => x.Id).Select(g => g.Last());

            var dbEmployeesFiltered = dbEmployees.ExceptBy(tempEmployees.Select(te => te.Id), x => x.Id);

            return dbEmployeesFiltered.Concat(tempEmployeesFiltered);
        }
    }
}