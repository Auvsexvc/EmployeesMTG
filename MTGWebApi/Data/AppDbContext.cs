using MTGWebApi.Entities;
using MTGWebApi.Enums;
using MTGWebApi.Interfaces;

namespace MTGWebApi.Data
{
    public class AppDbContext : IAppDbContext
    {
        protected readonly string _dbFullPath;
        protected readonly string _tempFullPath;
        protected readonly IAppDbFileHandler _appDbFileHandler;

        public AppDbContext(IConfiguration configuration, IAppDbFileHandler appDbFileHandler)
        {
            var dbFile = configuration.GetSection("DbInfo:DbFile").Get<string>();
            var tempFile = configuration.GetSection("DbInfo:TempFile").Get<string>();
            _dbFullPath = Path.Combine(Environment.CurrentDirectory, dbFile);
            _tempFullPath = Path.Combine(Environment.CurrentDirectory, tempFile);
            _appDbFileHandler = appDbFileHandler;
        }

        public async Task StageAddAsync(Employee employee)
        {
            await _appDbFileHandler.AppendToFile(employee, _tempFullPath);
        }

        public async Task StageDeleteAsync(Employee employee)
        {
            if (!_appDbFileHandler.DoesFileExists(_tempFullPath))
            {
                await _appDbFileHandler.CreateFile(_tempFullPath);
            }

            if ((await _appDbFileHandler.GetEmployeesFromFileAsync(_tempFullPath)).Any(x => x.Id == employee.Id))
            {
                var lines = await File.ReadAllLinesAsync(_tempFullPath);
                var lineNumbersToDelete = Enumerable.Range(0, lines.Length).Where(x => lines[x].Split(';')[0] == employee.Id.ToString()).ToArray();
                var newLines = _appDbFileHandler.LineRemover(lines, lineNumbersToDelete);
                await File.WriteAllLinesAsync(_tempFullPath, newLines);
            }

            if ((await _appDbFileHandler.GetEmployeesFromFileAsync(_dbFullPath)).Any(x => x.Id == employee.Id))
            {
                await _appDbFileHandler.AppendToFile(employee, _tempFullPath);
            }
        }

        public async Task StageUpdateAsync(Employee employee)
        {
            if (!_appDbFileHandler.DoesFileExists(_tempFullPath))
            {
                await _appDbFileHandler.CreateFile(_tempFullPath);
            }

            if ((await _appDbFileHandler.GetEmployeesFromFileAsync(_tempFullPath)).Any(x => x.Id == employee.Id && x.State != Operation.Delete))
            {
                var lines = await File.ReadAllLinesAsync(_tempFullPath);
                var lineNumbersToDelete = Enumerable.Range(0, lines.Length).Where(x => lines[x].Split(';')[0] == employee.Id.ToString() && Enum.Parse<Operation>(lines[x].Split(';')[10]) != Operation.Delete).ToArray();
                if (lineNumbersToDelete.Any(x => Enum.Parse<Operation>(lines[x].Split(';')[10]) == Operation.Create))
                {
                    employee.State = Operation.Create;
                }
                var newLines = _appDbFileHandler.LineRemover(lines, lineNumbersToDelete);
                await File.WriteAllLinesAsync(_tempFullPath, newLines);
            }

            await _appDbFileHandler.AppendToFile(employee, _tempFullPath);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesAsync()
        {
            if (!_appDbFileHandler.DoesFileExists(_tempFullPath))
            {
                return await _appDbFileHandler.GetEmployeesFromFileAsync(_dbFullPath);
            }

            return await GetEmployeesAccomodateStaged();
        }

        public async Task CancelChangesAsync()
        {
            if (_appDbFileHandler.DoesFileExists(_tempFullPath))
            {
                await Task.Factory.StartNew(() => File.Delete(_tempFullPath));
            }
        }

        public async Task<IEnumerable<Employee>> GetStagedChangesAsync()
        {
            return await _appDbFileHandler.GetEmployeesFromFileAsync(_tempFullPath);
        }

        public async Task CommitChangesAsync()
        {
            if (_appDbFileHandler.DoesFileExists(_tempFullPath))
            {
                await CommitStagedOperationsAsync();
                await Task.Run(() => File.Delete(_tempFullPath));
            }
        }

        private async Task CommitStagedOperationsAsync()
        {
            var employees = (await _appDbFileHandler.GetEmployeesFromFileAsync(_tempFullPath));

            foreach (var employee in employees)
            {
                await _appDbFileHandler.CommitOperationToFile(employee, _dbFullPath);
            }
        }

        private async Task<IEnumerable<Employee>> GetEmployeesAccomodateStaged()
        {
            var dbEmployees = await _appDbFileHandler.GetEmployeesFromFileAsync(_dbFullPath);
            var tempEmployees = await _appDbFileHandler.GetEmployeesFromFileAsync(_tempFullPath);

            var tempEmployeesFiltered = tempEmployees.Where(e => e.State != Operation.Delete).GroupBy(x => x.Id).Select(g => g.Last());

            var dbEmployeesFiltered = dbEmployees.ExceptBy(tempEmployees.Select(te => te.Id), x => x.Id);

            return dbEmployeesFiltered.Concat(tempEmployeesFiltered);
        }
    }
}