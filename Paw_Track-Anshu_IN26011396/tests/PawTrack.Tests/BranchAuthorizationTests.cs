using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Services;
using Xunit;

namespace PawTrack.Tests;

public class BranchAuthorizationTests
{
    private readonly User _orgAdmin = new() { Id = 1, Name = "Sarah Admin", Role = UserRole.OrgAdmin, BranchId = null };
    private readonly User _branchAdmin1 = new() { Id = 2, Name = "John Manager", Role = UserRole.BranchAdmin, BranchId = 1 };
    private readonly User _branchAdmin2 = new() { Id = 5, Name = "Elena Manager", Role = UserRole.BranchAdmin, BranchId = 2 };

    [Fact]
    public async Task BranchAdmin1_ApprovingApplicationForBranch2_ThrowsUnauthorizedAccessException()
    {
        using var context = TestDbContextFactory.Create();
        var approvalService = new ApprovalService(context);

        // App #2 belongs to Animal 5 (Branch 2)
        var app2 = await context.AdoptionApplications.FindAsync(2);
        Assert.NotNull(app2);
        app2.Status = ApplicationStatus.Pending;
        await context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            approvalService.ApproveApplicationAsync(2, _branchAdmin1));

        Assert.Contains("Staff can only approve applications for their own branch", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BranchAdmin2_ApprovingApplicationForBranch2_Succeeds()
    {
        using var context = TestDbContextFactory.Create();
        var approvalService = new ApprovalService(context);

        var app2 = await context.AdoptionApplications.FindAsync(2);
        Assert.NotNull(app2);
        app2.Status = ApplicationStatus.Pending;

        var animal5 = await context.Animals.FindAsync(5);
        Assert.NotNull(animal5);
        animal5.Status = AnimalStatus.Available;
        await context.SaveChangesAsync();

        var adoption = await approvalService.ApproveApplicationAsync(2, _branchAdmin2);
        Assert.NotNull(adoption);
    }

    [Fact]
    public async Task OrgAdmin_ApprovingApplicationAcrossBranches_Succeeds()
    {
        using var context = TestDbContextFactory.Create();
        var approvalService = new ApprovalService(context);

        var app2 = await context.AdoptionApplications.FindAsync(2);
        Assert.NotNull(app2);
        app2.Status = ApplicationStatus.Pending;

        var animal5 = await context.Animals.FindAsync(5);
        Assert.NotNull(animal5);
        animal5.Status = AnimalStatus.Available;
        await context.SaveChangesAsync();

        var adoption = await approvalService.ApproveApplicationAsync(2, _orgAdmin);
        Assert.NotNull(adoption);
    }
}
