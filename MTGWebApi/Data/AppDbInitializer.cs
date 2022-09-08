namespace MTGWebApi.Data
{
    public class AppDbInitializer : AppDbContext, IHostedService
    {
        public AppDbInitializer(IConfiguration configuration) : base(configuration)
        {
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
            await Task.Run(() =>
            {
                using var fileStream = new FileStream(filePath, FileMode.CreateNew);
            });
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
            await Task.Run(() => File.Delete(_tempFullPath));
        }
    }
}