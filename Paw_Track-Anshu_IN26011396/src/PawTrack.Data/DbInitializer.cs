using PawTrack.Core.Entities;
using PawTrack.Core.Enums;

namespace PawTrack.Data;

public static class DbInitializer
{
    public static void Initialize(PawTrackDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Branches.Any())
        {
            return; // DB has already been seeded
        }

        var branches = new List<Branch>
        {
            new() { Id = 1, Name = "Downtown Shelter", Region = "Metro Region", Address = "124 Shelter Way, City Center", Phone = "+1-555-0192" },
            new() { Id = 2, Name = "Westside Rescue", Region = "West District", Address = "890 Haven Blvd, Westside", Phone = "+1-555-0198" }
        };
        context.Branches.AddRange(branches);
        context.SaveChanges();

        var users = new List<User>
        {
            new() { Id = 1, Name = "Sarah Admin (Org)", Email = "sarah.admin@pawtrack.org", Role = UserRole.OrgAdmin, BranchId = null },
            new() { Id = 2, Name = "John Manager (Branch 1)", Email = "john.b1@pawtrack.org", Role = UserRole.BranchAdmin, BranchId = 1 },
            new() { Id = 3, Name = "Alice Staff (Branch 1)", Email = "alice.s1@pawtrack.org", Role = UserRole.RescueStaff, BranchId = 1 },
            new() { Id = 4, Name = "Dr. Bob Vet (Branch 1)", Email = "bob.vet1@pawtrack.org", Role = UserRole.Veterinarian, BranchId = 1 },
            new() { Id = 5, Name = "Elena Manager (Branch 2)", Email = "elena.b2@pawtrack.org", Role = UserRole.BranchAdmin, BranchId = 2 },
            new() { Id = 6, Name = "David Staff (Branch 2)", Email = "david.s2@pawtrack.org", Role = UserRole.RescueStaff, BranchId = 2 },
            new() { Id = 7, Name = "Dr. Carol Vet (Branch 2)", Email = "carol.vet2@pawtrack.org", Role = UserRole.Veterinarian, BranchId = 2 },
            new() { Id = 8, Name = "Mark Adopter", Email = "mark.adopter@gmail.com", Role = UserRole.Adopter, BranchId = null },
            new() { Id = 9, Name = "Emily Adopter", Email = "emily.adopter@yahoo.com", Role = UserRole.Adopter, BranchId = null },
            new() { Id = 10, Name = "Michael Adopter", Email = "michael.adopter@outlook.com", Role = UserRole.Adopter, BranchId = null }
        };
        context.Users.AddRange(users);
        context.SaveChanges();

        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Dog" },
            new() { Id = 2, Name = "Cat" },
            new() { Id = 3, Name = "Rabbit" },
            new() { Id = 4, Name = "Bird" }
        };
        context.Categories.AddRange(categories);
        context.SaveChanges();

        var animals = new List<Animal>
        {
            new()
            {
                Id = 1,
                Name = "Bruno",
                Species = "Dog",
                Breed = "Golden Retriever",
                Age = 3,
                Gender = "Male",
                RescueDate = DateTime.Parse("2026-06-10"),
                RescueLocation = "Central Park",
                Status = AnimalStatus.Adopted,
                BranchId = 1,
                CategoryId = 1,
                MicrochipNumber = "985141002341234",
                Description = "Friendly and active Golden Retriever who loves fetching balls.",
                ImageUrl = "https://images.unsplash.com/photo-1552053831-71594a27632d?w=600&auto=format&fit=crop&q=80"
            },
            new()
            {
                Id = 2,
                Name = "Whiskers",
                Species = "Cat",
                Breed = "Persian",
                Age = 2,
                Gender = "Female",
                RescueDate = DateTime.Parse("2026-07-01"),
                RescueLocation = "Downtown Alley",
                Status = AnimalStatus.Available,
                BranchId = 1,
                CategoryId = 2,
                MicrochipNumber = "985141002341235",
                Description = "Calm, fluffy feline who enjoys sunny spots and quiet cuddles.",
                ImageUrl = "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=600&auto=format&fit=crop&q=80"
            },
            new()
            {
                Id = 3,
                Name = "Rocky",
                Species = "Dog",
                Breed = "Beagle",
                Age = 1,
                Gender = "Male",
                RescueDate = DateTime.Parse("2026-07-15"),
                RescueLocation = "North Suburbs",
                Status = AnimalStatus.Available,
                BranchId = 1,
                CategoryId = 1,
                MicrochipNumber = "985141002341236",
                Description = "Playful pup with a great curious nose for outdoor adventures.",
                ImageUrl = "https://images.unsplash.com/photo-1537151608828-ea2b11777ee8?w=600&auto=format&fit=crop&q=80"
            },
            new()
            {
                Id = 4,
                Name = "Tweety",
                Species = "Bird",
                Breed = "Canary",
                Age = 1,
                Gender = "Female",
                RescueDate = DateTime.Parse("2026-07-20"),
                RescueLocation = "Westside Market",
                Status = AnimalStatus.Available,
                BranchId = 2,
                CategoryId = 4,
                Description = "Cheerful yellow singing canary looking for a warm home.",
                ImageUrl = "https://images.unsplash.com/photo-1522858547137-f1dcec554f55?w=600&auto=format&fit=crop&q=80"
            },
            new()
            {
                Id = 5,
                Name = "Simba",
                Species = "Cat",
                Breed = "Siamese",
                Age = 4,
                Gender = "Male",
                RescueDate = DateTime.Parse("2026-06-25"),
                RescueLocation = "Sunset Avenue",
                Status = AnimalStatus.Adopted,
                BranchId = 2,
                CategoryId = 2,
                MicrochipNumber = "985141002341238",
                Description = "Vocal and loving Siamese who forms deep bonds with owners.",
                ImageUrl = "https://images.unsplash.com/photo-1513360371669-4adf3dd7dff8?w=600&auto=format&fit=crop&q=80"
            },
            new()
            {
                Id = 6,
                Name = "Coco",
                Species = "Dog",
                Breed = "Poodle",
                Age = 2,
                Gender = "Female",
                RescueDate = DateTime.Parse("2026-08-01"),
                RescueLocation = "Eastside Square",
                Status = AnimalStatus.Available,
                BranchId = 2,
                CategoryId = 1,
                MicrochipNumber = "985141002341239",
                Description = "Intelligent, hypoallergenic companion ready for trick training.",
                ImageUrl = "https://images.unsplash.com/photo-1591769225440-811ad7d6eab2?w=600&auto=format&fit=crop&q=80"
            },
            new()
            {
                Id = 7,
                Name = "Fluffy",
                Species = "Rabbit",
                Breed = "Angora",
                Age = 1,
                Gender = "Male",
                RescueDate = DateTime.Parse("2026-08-05"),
                RescueLocation = "Greenwood Farm",
                Status = AnimalStatus.Available,
                BranchId = 1,
                CategoryId = 3,
                Description = "Gentle bunny who enjoys fresh leafy greens and lap pets.",
                ImageUrl = "https://images.unsplash.com/photo-1585110396000-c9ffd4e4b308?w=600&auto=format&fit=crop&q=80"
            }
        };
        context.Animals.AddRange(animals);
        context.SaveChanges();

        var visitSlots = new List<VisitSlot>
        {
            new() { Id = 1, BranchId = 1, Date = DateTime.Parse("2026-08-25"), StartTime = TimeSpan.Parse("10:00"), EndTime = TimeSpan.Parse("11:00"), Capacity = 3, BookedCount = 2 },
            new() { Id = 2, BranchId = 1, Date = DateTime.Parse("2026-08-25"), StartTime = TimeSpan.Parse("14:00"), EndTime = TimeSpan.Parse("15:00"), Capacity = 3, BookedCount = 1 },
            new() { Id = 3, BranchId = 1, Date = DateTime.Parse("2026-08-26"), StartTime = TimeSpan.Parse("11:00"), EndTime = TimeSpan.Parse("12:00"), Capacity = 3, BookedCount = 0 },
            new() { Id = 4, BranchId = 2, Date = DateTime.Parse("2026-08-25"), StartTime = TimeSpan.Parse("10:00"), EndTime = TimeSpan.Parse("11:00"), Capacity = 2, BookedCount = 2 },
            new() { Id = 5, BranchId = 2, Date = DateTime.Parse("2026-08-26"), StartTime = TimeSpan.Parse("15:00"), EndTime = TimeSpan.Parse("16:00"), Capacity = 2, BookedCount = 0 }
        };
        context.VisitSlots.AddRange(visitSlots);
        context.SaveChanges();

        var visitBookings = new List<VisitBooking>
        {
            new() { Id = 1, VisitSlotId = 1, AnimalId = 1, AdopterId = 8, BookingDate = DateTime.Parse("2026-08-18"), Status = VisitBookingStatus.Completed, StaffNotes = "Adopter met Bruno; very affectionate interaction." },
            new() { Id = 2, VisitSlotId = 1, AnimalId = 3, AdopterId = 9, BookingDate = DateTime.Parse("2026-08-19"), Status = VisitBookingStatus.Completed, StaffNotes = "Emily loved Rocky, considering application." },
            new() { Id = 3, VisitSlotId = 2, AnimalId = 2, AdopterId = 10, BookingDate = DateTime.Parse("2026-08-21"), Status = VisitBookingStatus.Completed, StaffNotes = "Michael visited Whiskers." },
            new() { Id = 4, VisitSlotId = 4, AnimalId = 5, AdopterId = 9, BookingDate = DateTime.Parse("2026-08-20"), Status = VisitBookingStatus.Completed, StaffNotes = "Visit with Simba was wonderful." },
            new() { Id = 5, VisitSlotId = 4, AnimalId = 6, AdopterId = 10, BookingDate = DateTime.Parse("2026-08-22"), Status = VisitBookingStatus.Booked, StaffNotes = "Scheduled visit for Coco." }
        };
        context.VisitBookings.AddRange(visitBookings);
        context.SaveChanges();

        var adoptionApplications = new List<AdoptionApplication>
        {
            new() { Id = 1, AnimalId = 1, AdopterId = 8, VisitBookingId = 1, ApplicationDate = DateTime.Parse("2026-08-20"), Status = ApplicationStatus.Approved, Notes = "Experienced dog owner with fenced yard." },
            new() { Id = 2, AnimalId = 5, AdopterId = 9, VisitBookingId = 4, ApplicationDate = DateTime.Parse("2026-08-21"), Status = ApplicationStatus.Approved, Notes = "Apartment owner with cat-proofing." },
            new() { Id = 3, AnimalId = 2, AdopterId = 10, VisitBookingId = 3, ApplicationDate = DateTime.Parse("2026-08-22"), Status = ApplicationStatus.Pending, Notes = "First-time cat adopter, home visit completed." }
        };
        context.AdoptionApplications.AddRange(adoptionApplications);
        context.SaveChanges();

        var adoptions = new List<Adoption>
        {
            new() { Id = 1, ApplicationId = 1, AnimalId = 1, AdopterId = 8, ApprovedById = 2, AdoptionDate = DateTime.Parse("2026-08-21"), Notes = "Final adoption paperwork signed." },
            new() { Id = 2, ApplicationId = 2, AnimalId = 5, AdopterId = 9, ApprovedById = 5, AdoptionDate = DateTime.Parse("2026-08-22"), Notes = "Adoption completed smoothly." }
        };
        context.Adoptions.AddRange(adoptions);
        context.SaveChanges();

        var followUps = new List<FollowUp>
        {
            new() { Id = 1, AdoptionId = 1, ConductedById = 3, FollowUpDate = DateTime.Parse("2026-08-28"), Notes = "Bruno is settling in well. Eating properly.", Status = FollowUpStatus.Completed },
            new() { Id = 2, AdoptionId = 2, ConductedById = 6, FollowUpDate = DateTime.Parse("2026-08-29"), Notes = "Simba health check ok. Very friendly with new owner.", Status = FollowUpStatus.Completed },
            new() { Id = 3, AdoptionId = 1, ConductedById = 3, FollowUpDate = DateTime.Parse("2026-09-15"), Notes = "Routine 1-month post-adoption checkup scheduled.", Status = FollowUpStatus.Scheduled }
        };
        context.FollowUps.AddRange(followUps);
        context.SaveChanges();
    }
}
