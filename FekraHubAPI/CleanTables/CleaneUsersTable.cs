using FekraHubAPI.Data;

namespace FekraHubAPI.CleanTables
{
    
    public class CleaneUsersTable : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromDays(30); // start clean every 30 days ========

        public CleaneUsersTable(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanUpUnconfirmedUsersAsync();
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }

        private async Task CleanUpUnconfirmedUsersAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                //================ email was registerd 30 days ago to delete ===============
                var thresholdDate = DateTime.UtcNow.AddDays(-30);

                var unconfirmedUsers = context.ApplicationUser
                    .Where(u => !u.EmailConfirmed && u.RegistrationDate <= thresholdDate)
                    .ToList();

                if (unconfirmedUsers.Any())
                {
                    context.Users.RemoveRange(unconfirmedUsers);
                    await context.SaveChangesAsync();
                }
            }
        }
    }

}
