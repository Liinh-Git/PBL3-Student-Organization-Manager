using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Org.Shared.Features.Milestones;

namespace Org.Frontend.Services.Milestones
{
    public interface IMilestoneService
    {
        Task<List<MilestoneDto>> GetMilestonesAsync(Guid eventId);
    }
}