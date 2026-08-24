using PawTrack.Core.DTOs;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Services;
using Xunit;

namespace PawTrack.Tests;

public class AdoptionApplicationTests
{
    private readonly User _adopter8 = new() { Id = 8, Name = "Mark Adopter", Role = UserRole.Adopter, BranchId = null };
    private readonly User _adopter9 = new() { Id = 9, Name = "Emily Adopter", Role = UserRole.Adopter, BranchId = null };
    private readonly User _adopter10 = new() { Id = 10, Name = "Michael Adopter", Role = UserRole.Adopter, BranchId = null };

    [Fact]
    public async Task SubmitApplication_ForAvailableAnimal_Succeeds()
    {
        using var context = TestDbContextFactory.Create();
        var service = new AdoptionService(context);

        // Animal 2 (Whiskers) is Available
        var app = await service.SubmitApplicationAsync(new SubmitApplicationDto { AnimalId = 2 }, _adopter8);

        Assert.NotNull(app);
        Assert.Equal(2, app.AnimalId);
        Assert.Equal(8, app.AdopterId);
        Assert.Equal(ApplicationStatus.Pending, app.Status);
    }

    [Fact]
    public async Task SubmitApplication_ForNonAvailableAnimal_ThrowsInvalidOperationException()
    {
        using var context = TestDbContextFactory.Create();
        var service = new AdoptionService(context);

        // Animal 1 (Bruno) is Adopted
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SubmitApplicationAsync(new SubmitApplicationDto { AnimalId = 1 }, _adopter8));

        Assert.Contains("Animal is no longer available", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SubmitApplication_DuplicateActiveApplication_ThrowsInvalidOperationException()
    {
        using var context = TestDbContextFactory.Create();
        var service = new AdoptionService(context);

        // Adopter 10 already has a Pending application (App #3) for Animal 2
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SubmitApplicationAsync(new SubmitApplicationDto { AnimalId = 2 }, _adopter10));

        Assert.Contains("An active application already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SubmitApplication_DifferentAdoptersForSameAvailableAnimal_Allowed()
    {
        using var context = TestDbContextFactory.Create();
        var service = new AdoptionService(context);

        // Animal 2 has Pending app from Adopter 10 in seed data. Adopter 8 can also apply!
        var appAdopter8 = await service.SubmitApplicationAsync(new SubmitApplicationDto { AnimalId = 2 }, _adopter8);

        Assert.NotNull(appAdopter8);
        Assert.Equal(8, appAdopter8.AdopterId);
        Assert.Equal(2, appAdopter8.AnimalId);
        Assert.Equal(ApplicationStatus.Pending, appAdopter8.Status);
    }

    [Fact]
    public async Task SubmitApplication_WithValidMatchingVisitBooking_Succeeds()
    {
        using var context = TestDbContextFactory.Create();
        var service = new AdoptionService(context);

        // Visit 2 belongs to Adopter 9 and Animal 3
        var app = await service.SubmitApplicationAsync(new SubmitApplicationDto
        {
            AnimalId = 3,
            VisitBookingId = 2
        }, _adopter9);

        Assert.NotNull(app);
        Assert.Equal(2, app.VisitBookingId);
    }

    [Fact]
    public async Task SubmitApplication_WithMismatchedAdopterVisit_ThrowsInvalidOperationException()
    {
        using var context = TestDbContextFactory.Create();
        var service = new AdoptionService(context);

        // Visit 2 belongs to Adopter 9. Adopter 8 trying to link it should fail!
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SubmitApplicationAsync(new SubmitApplicationDto
            {
                AnimalId = 3,
                VisitBookingId = 2
            }, _adopter8));

        Assert.Contains("The selected visit does not belong to this adopter", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SubmitApplication_WithMismatchedAnimalVisit_ThrowsInvalidOperationException()
    {
        using var context = TestDbContextFactory.Create();
        var service = new AdoptionService(context);

        // Visit 3 belongs to Animal 2. Linking it to Animal 3 should fail!
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SubmitApplicationAsync(new SubmitApplicationDto
            {
                AnimalId = 3,
                VisitBookingId = 3
            }, _adopter10));

        Assert.Contains("The selected visit belongs to a different animal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
