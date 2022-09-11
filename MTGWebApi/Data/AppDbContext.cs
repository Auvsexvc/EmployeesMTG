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
            var persistingRecords = await _appDbFileHandler.GetEmployeesFromFileAsync(_dbFullPath);
            var stagedRecords = await _appDbFileHandler.GetEmployeesFromFileAsync(_tempFullPath);

            if (persistingRecords.Any(x => x.Id == employee.Id) || stagedRecords.Any(x => x.Id == employee.Id))
            {
                await _appDbFileHandler.CommitOperationToFile(employee, _tempFullPath);
            }

            if (persistingRecords.Any(x => x.Id == employee.Id))
            {
                await _appDbFileHandler.AppendToFile(employee, _tempFullPath);
            }
        }

        public async Task StageUpdateAsync(Employee employee)
        {
            var persistingRecords = await _appDbFileHandler.GetEmployeesFromFileAsync(_dbFullPath);
            var stagedRecords = await _appDbFileHandler.GetEmployeesFromFileAsync(_tempFullPath);

            if (persistingRecords.Any(x => x.Id == employee.Id) || stagedRecords.Any(x => x.Id == employee.Id))
            {
                await _appDbFileHandler.CommitOperationToFile(employee, _tempFullPath);
            }
        }

        public async Task<IEnumerable<Employee>> GetEmployeesAsync()
        {
            return await GetEmployeesAccomodateStaged();
        }

        public async Task CancelChangesAsync()
        {
            if (_appDbFileHandler.DoesFileExists(_tempFullPath))
            {
                await Task.Run(() => File.Delete(_tempFullPath));
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

        public bool CanConnectToDb()
        {
            return _appDbFileHandler.DoesFileExists(_dbFullPath);
        }

        private async Task CommitStagedOperationsAsync()
        {
            var stagedRecords = await _appDbFileHandler.GetEmployeesFromFileAsync(_tempFullPath);

            foreach (var employee in stagedRecords)
            {
                await _appDbFileHandler.CommitOperationToFile(employee, _dbFullPath, true);
            }
        }

        private async Task<IEnumerable<Employee>> GetEmployeesAccomodateStaged()
        {
            var persistingRecords = await _appDbFileHandler.GetEmployeesFromFileAsync(_dbFullPath);
            var stagedRecords = await _appDbFileHandler.GetEmployeesFromFileAsync(_tempFullPath);

            var stagedRecordsFiltered = stagedRecords.Where(e => e.State != Operation.Delete).GroupBy(x => x.Id).Select(g => g.Last());

            var persistingRecordsFiltered = persistingRecords.ExceptBy(stagedRecords.Select(te => te.Id), x => x.Id);

            return persistingRecordsFiltered.Concat(stagedRecordsFiltered);
        }
    }
}