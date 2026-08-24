using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Services;
using Xunit;

namespace PawTrack.Tests;

public class RejectionWorkflowTests
{
    private readonly User _branchAdmin1 = new() { Id = 2, Name = "John Manager", Role = UserRole.BranchAdmin, BranchId = 1 };

    [Fact]
    public async Task RejectApplication_WithoutReason_ThrowsArgumentException()
    {
        using var context = TestDbContextFactory.Create();
        var approvalService = new ApprovalService(context);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            approvalService.RejectApplicationAsync(3, "", _branchAdmin1));

        Assert.Contains("Rejection reason is required", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RejectApplication_WithValidReason_SetsStatusRejectedAndRetainsRecord()
    {
        using var context = TestDbContextFactory.Create();
        var approvalService = new ApprovalService(context);

        await approvalService.RejectApplicationAsync(3, "Housing background check failed.", _branchAdmin1);

        var app3 = await context.AdoptionApplications.FindAsync(3);
        Assert.NotNull(app3);
        Assert.Equal(ApplicationStatus.Rejected, app3.Status);
        Assert.Equal("Housing background check failed.", app3.RejectionReason);
    }
}
