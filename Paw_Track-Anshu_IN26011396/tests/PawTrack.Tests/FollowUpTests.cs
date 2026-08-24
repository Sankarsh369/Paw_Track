using PawTrack.Core.DTOs;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Services;
using Xunit;

namespace PawTrack.Tests;

public class FollowUpTests
{
    private readonly User _rescueStaff1 = new() { Id = 3, Name = "Alice Staff", Role = UserRole.RescueStaff, BranchId = 1 };
    private readonly User _orgAdmin = new() { Id = 1, Name = "Sarah Admin", Role = UserRole.OrgAdmin, BranchId = null };

    [Fact]
    public async Task FollowUp_Schedule_Complete_ScheduleSecond_PreservesOneToNHistory()
    {
        using var context = TestDbContextFactory.Create();
        var followUpService = new FollowUpService(context);

        // Schedule first follow-up for Adoption #1
        var fu1 = await followUpService.ScheduleFollowUpAsync(new ScheduleFollowUpDto
        {
            AdoptionId = 1,
            FollowUpDate = DateTime.Parse("2026-09-01"),
            Notes = "1-week initial home visit"
        }, _rescueStaff1);

        Assert.Equal(FollowUpStatus.Scheduled, fu1.Status);

        // Complete first follow-up
        var completedFu1 = await followUpService.CompleteFollowUpAsync(fu1.Id, "Home visit successful. Dog is happy.", _rescueStaff1);
        Assert.Equal(FollowUpStatus.Completed, completedFu1.Status);

        // Schedule second follow-up
        var fu2 = await followUpService.ScheduleFollowUpAsync(new ScheduleFollowUpDto
        {
            AdoptionId = 1,
            FollowUpDate = DateTime.Parse("2026-10-01"),
            Notes = "2-month routine checkup"
        }, _rescueStaff1);

        Assert.Equal(FollowUpStatus.Scheduled, fu2.Status);

        // Retrieve full history (Seed had 2 follow-ups + 2 new = 4 total)
        var history = await followUpService.GetFollowUpHistoryAsync(1, _orgAdmin);
        Assert.Equal(4, history.Count);
    }
}
