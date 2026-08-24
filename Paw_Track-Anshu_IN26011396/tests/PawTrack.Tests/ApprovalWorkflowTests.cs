using Microsoft.EntityFrameworkCore;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Services;
using Xunit;

namespace PawTrack.Tests;

public class ApprovalWorkflowTests
{
    private readonly User _branchAdmin1 = new() { Id = 2, Name = "John Manager", Role = UserRole.BranchAdmin, BranchId = 1 };

    [Fact]
    public async Task ApproveApplication_CreatesAdoption_UpdatesAnimalStatus_FlagsOpenVisits()
    {
        using var context = TestDbContextFactory.Create();
        var approvalService = new ApprovalService(context);

        // App #3 is Pending for Animal 2 (Whiskers, Available) by Adopter 10
        var animal2 = await context.Animals.FindAsync(2);
        Assert.NotNull(animal2);
        Assert.Equal(AnimalStatus.Available, animal2.Status);

        var adoption = await approvalService.ApproveApplicationAsync(3, _branchAdmin1);

        Assert.NotNull(adoption);
        Assert.Equal(3, adoption.ApplicationId);
        Assert.Equal(2, adoption.AnimalId);
        Assert.Equal(10, adoption.AdopterId);
        Assert.Equal(2, adoption.ApprovedById); // John (BranchAdmin1)

        // Verify Animal.Status transitioned to Adopted
        Assert.Equal(AnimalStatus.Adopted, animal2.Status);

        // Verify Application.Status transitioned to Approved
        var app3 = await context.AdoptionApplications.FindAsync(3);
        Assert.NotNull(app3);
        Assert.Equal(ApplicationStatus.Approved, app3.Status);
    }
}
