using MTGWebApi.Interfaces;

namespace MTGWebApi.Data
{
    public class AppDbInitializer : AppDbContext, IHostedService
    {
        public AppDbInitializer(IConfiguration configuration, IAppDbFileHandler appDbFileHandler) : base(configuration, appDbFileHandler)
        {
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Init();
            await _appDbFileHandler.DeleteFile(_tempFullPath);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _appDbFileHandler.DeleteFile(_tempFullPath);
        }

        private async Task Init()
        {
            if (!_appDbFileHandler.DoesFileExists(_dbFullPath))
            {
                await _appDbFileHandler.CreateFile(_dbFullPath);
            }
        }
    }
}