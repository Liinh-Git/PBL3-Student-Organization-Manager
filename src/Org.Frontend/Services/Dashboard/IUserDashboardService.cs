using Org.Frontend.ViewModels;

namespace Org.Frontend.Services.Dashboard;

public interface IUserDashboardService
{
    Task<UserDashboardViewModel> GetDashboardAsync(CancellationToken ct = default);
}
