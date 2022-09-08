using MTGWebApi.Entities;
using MTGWebApi.Enums;

namespace MTGWebApi.Data
{
    public class AppDbContext : IAppDbContext
    {
        private readonly string _dbFullPath;
        private readonly string _backupFullPath;
        private readonly string _tempFullPath;

        public AppDbContext(IConfiguration configuration)
        {
            var dbFile = configuration.GetSection("DbInfo:DbFile").Get<string>();
            var tempFile = configuration.GetSection("DbInfo:TempFile").Get<string>();
            var bkpFile = configuration.GetSection("DbInfo:BackupFile").Get<string>();
            _dbFullPath = Path.Combine(Environment.CurrentDirectory, dbFile);
            _tempFullPath = Path.Combine(Environment.CurrentDirectory, tempFile);
            _backupFullPath = Path.Combine(Environment.CurrentDirectory, bkpFile);
        }

        public async Task AddAsync(Employee employee)
        {
            if (!File.Exists(_tempFullPath))
            {
                await CreateTempFile();
            }

            using var streamWriter = new StreamWriter(_tempFullPath, true);
            var line = string.Join(';', employee.GetType().GetProperties().Select(p => p.GetValue(employee)));
            await streamWriter.WriteLineAsync(line);
            await streamWriter.FlushAsync();
        }

        public async Task CancelChangesAsync()
        {
            if (File.Exists(_tempFullPath))
            {
                await Task.Factory.StartNew(() => File.Delete(_tempFullPath));
            }
        }

        public async Task DeleteAsync(Employee employee)
        {
            if (!File.Exists(_tempFullPath))
            {
                using (new FileStream(_tempFullPath, FileMode.CreateNew)) { }
                await Task.Factory.StartNew(() => File.Copy(_dbFullPath, _tempFullPath, true));
            }

            var lines = await File.ReadAllLinesAsync(_tempFullPath);
            var lineNumberToDelete = Enumerable.Range(0, lines.Length).FirstOrDefault(x => lines[x].Split(';')[0] == employee.Id.ToString());

            await LineRemover(_tempFullPath, lineNumberToDelete);
        }

        public async Task<List<Employee>> GetEmployeesAsync()
        {
            if (!File.Exists(_tempFullPath))
            {
                return await GetEmployeesFromFileAsync(_dbFullPath);
            }

            return await GetEmployeesFromFileAsync(_tempFullPath);
        }

        public async Task<IEnumerable<Employee>> PendingChangesAsync()
        {
            if (!File.Exists(_tempFullPath))
            {
                return Enumerable.Empty<Employee>();
            }

            String[] linesA = await File.ReadAllLinesAsync(_dbFullPath);
            String[] linesB = await File.ReadAllLinesAsync(_tempFullPath);

            var changes = linesB.Except(linesA);
            var removes = linesA.Except(linesB);

            return changes.Union(removes).Select(l => ConvertToEmployee(l));
        }

        public async Task SaveChangesAsync()
        {
            if (!File.Exists(_tempFullPath))
            {
                await Task.Factory.StartNew(() => File.Replace(_tempFullPath, _dbFullPath, _backupFullPath));
                await Task.Factory.StartNew(() => File.Delete(_tempFullPath));
            }
        }

        public async Task UpdateAsync(Employee employee)
        {
            if (!File.Exists(_tempFullPath))
            {
                using (new FileStream(_tempFullPath, FileMode.CreateNew)) { }
                await Task.Factory.StartNew(() => File.Copy(_dbFullPath, _tempFullPath, true));
            }

            var lines = await File.ReadAllLinesAsync(_tempFullPath);
            var lineNumberToUpdate = Enumerable.Range(0, lines.Length).FirstOrDefault(x => lines[x].Split(';')[0] == employee.Id.ToString());
            var lineNewData = string.Join(';', employee.GetType().GetProperties().Select(p => p.GetValue(employee)));

            await LineChanger(lineNewData, _tempFullPath, lineNumberToUpdate);
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

        private static async Task LineChanger(string newText, string fileName, int lineNumber)
        {
            string[] arrLine = await File.ReadAllLinesAsync(fileName);
            arrLine[lineNumber] = newText;
            await File.WriteAllLinesAsync(fileName, arrLine);
        }

        private static async Task LineRemover(string fileName, int lineNumber)
        {
            List<string> arrLine = (await File.ReadAllLinesAsync(fileName)).ToList();
            arrLine.RemoveAt(lineNumber);
            await File.WriteAllLinesAsync(fileName, arrLine);
        }

        private async Task CreateTempFile()
        {
            using (new FileStream(_tempFullPath, FileMode.CreateNew)) { }
            await Task.Factory.StartNew(() => File.Copy(_dbFullPath, _tempFullPath, true));
        }
    }
}