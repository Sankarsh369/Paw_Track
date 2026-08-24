using PawTrack.Core.DTOs;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Services;
using Xunit;

namespace PawTrack.Tests;

public class ConcurrencyApprovalTests
{
    private readonly User _branchAdmin1 = new() { Id = 2, Name = "John Manager", Role = UserRole.BranchAdmin, BranchId = 1 };
    private readonly User _adopter8 = new() { Id = 8, Name = "Mark Adopter", Role = UserRole.Adopter, BranchId = null };
    private readonly User _adopter9 = new() { Id = 9, Name = "Emily Adopter", Role = UserRole.Adopter, BranchId = null };

    [Fact]
    public async Task MultipleApplicationsOnSameAnimal_OnlyOneApprovalSucceeds()
    {
        using var context = TestDbContextFactory.Create();
        var adoptionService = new AdoptionService(context);
        var approvalService = new ApprovalService(context);

        // Animal 3 (Rocky) is Available
        // Both Adopter 8 and Adopter 9 submit applications for Animal 3
        var appA = await adoptionService.SubmitApplicationAsync(new SubmitApplicationDto { AnimalId = 3 }, _adopter8);
        var appB = await adoptionService.SubmitApplicationAsync(new SubmitApplicationDto { AnimalId = 3 }, _adopter9);

        Assert.Equal(ApplicationStatus.Pending, appA.Status);
        Assert.Equal(ApplicationStatus.Pending, appB.Status);

        // Approve Adopter 8's application
        var adoption = await approvalService.ApproveApplicationAsync(appA.Id, _branchAdmin1);
        Assert.Equal(8, adoption.AdopterId);

        var animal3 = await context.Animals.FindAsync(3);
        Assert.NotNull(animal3);
        Assert.Equal(AnimalStatus.Adopted, animal3.Status);

        // Attempting to approve Adopter 9's application MUST now fail because animal is no longer Available
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            approvalService.ApproveApplicationAsync(appB.Id, _branchAdmin1));

        Assert.Contains("animal is no longer available", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
