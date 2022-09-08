namespace MTGWebApi.Data
{
    public class AppDbInitializer : IHostedService
    {
        private readonly string _dbFullPath;
        private readonly string _tempFullPath;

        public AppDbInitializer(IConfiguration configuration)
        {
            var dbFile = configuration.GetSection("DbInfo:DbFile").Get<string>();
            var tempFile = configuration.GetSection("DbInfo:TempFile").Get<string>();
            _dbFullPath = Path.Combine(Environment.CurrentDirectory, dbFile);
            _tempFullPath = Path.Combine(Environment.CurrentDirectory, tempFile);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Init();
            await RemoveTempFile();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await RemoveTempFile();
        }

        private static bool CheckIfExists(string filePath)
        {
            return File.Exists(filePath);
        }

        private static async Task CreateDbFile(string filePath)
        {
            using (await Task.Factory.StartNew(() => new FileStream(filePath, FileMode.CreateNew))) { }
        }

        private async Task Init()
        {
            if (!CheckIfExists(_dbFullPath))
            {
                await CreateDbFile(_dbFullPath);
            }
        }

        private async Task RemoveTempFile()
        {
            await Task.Factory.StartNew(() => File.Delete(_tempFullPath));
        }
    }
}