using PawTrack.Core.DTOs;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Services;
using Xunit;

namespace PawTrack.Tests;

public class NoHoldRuleTests
{
    private readonly User _adopter8 = new() { Id = 8, Name = "Mark Adopter", Role = UserRole.Adopter, BranchId = null };

    [Fact]
    public async Task BookingVisit_DoesNotChangeAnimalStatus()
    {
        using var context = TestDbContextFactory.Create();
        var visitService = new VisitService(context);

        var animal3 = await context.Animals.FindAsync(3);
        Assert.NotNull(animal3);
        Assert.Equal(AnimalStatus.Available, animal3.Status);

        // Book visit for Animal 3
        await visitService.BookVisitAsync(new BookVisitDto { VisitSlotId = 3, AnimalId = 3 }, _adopter8);

        // Animal.Status MUST remain Available
        Assert.Equal(AnimalStatus.Available, animal3.Status);
    }

    [Fact]
    public async Task SubmittingApplication_DoesNotChangeAnimalStatus()
    {
        using var context = TestDbContextFactory.Create();
        var adoptionService = new AdoptionService(context);

        var animal3 = await context.Animals.FindAsync(3);
        Assert.NotNull(animal3);
        Assert.Equal(AnimalStatus.Available, animal3.Status);

        // Submit application for Animal 3
        await adoptionService.SubmitApplicationAsync(new SubmitApplicationDto { AnimalId = 3 }, _adopter8);

        // Animal.Status MUST remain Available
        Assert.Equal(AnimalStatus.Available, animal3.Status);
    }
}
