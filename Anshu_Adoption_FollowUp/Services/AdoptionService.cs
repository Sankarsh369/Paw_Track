using Microsoft.EntityFrameworkCore;
using PawTrack.Api.Data;
using PawTrack.Api.DTOs.Adoption;
using PawTrack.Api.DTOs.FollowUp;
using PawTrack.Api.Models;

namespace PawTrack.Api.Services
{
    public class AdoptionService : IAdoptionService
    {
        private readonly PawTrackDbContext _context;

        public AdoptionService(PawTrackDbContext context)
        {
            _context = context;
        }

        public async Task<AdoptionApplicationDto> ApplyAsync(int adopterId, CreateAdoptionApplicationDto dto)
        {
            var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == dto.AnimalId);
            if (animal is null)
                throw new KeyNotFoundException($"Animal with Id {dto.AnimalId} was not found.");

            if (animal.Status == AnimalStatus.Adopted)
                throw new InvalidOperationException("This animal has already been adopted.");

            // Database Design §16.6 acceptance criteria: one active application per adopter
            // per animal. "Active" = Pending — a rejected one shouldn't block reapplying.
            var alreadyApplied = await _context.AdoptionApplications.AnyAsync(a =>
                a.AnimalId == dto.AnimalId &&
                a.AdopterId == adopterId &&
                a.Status == AdoptionApplicationStatus.Pending);
            if (alreadyApplied)
                throw new InvalidOperationException("You already have a pending application for this animal.");

            if (dto.VisitBookingId.HasValue)
            {
                var visitExists = await _context.VisitBookings.AnyAsync(v => v.Id == dto.VisitBookingId.Value && v.AdopterId == adopterId);
                if (!visitExists)
                    throw new KeyNotFoundException("Visit booking not found for this adopter.");
            }

            var application = new AdoptionApplication
            {
                AnimalId = dto.AnimalId,
                AdopterId = adopterId,
                VisitBookingId = dto.VisitBookingId,
                Status = AdoptionApplicationStatus.Pending
            };

            _context.AdoptionApplications.Add(application);

            // Once a real application is in, the animal is no longer open for casual browsing —
            // per Database Design §2, this is the point that actually locks it (visits alone don't).
            if (animal.Status == AnimalStatus.Available)
                animal.Status = AnimalStatus.Pending;

            await _context.SaveChangesAsync();

            await _context.Entry(application).Reference(a => a.Animal).LoadAsync();
            await _context.Entry(application).Reference(a => a.Adopter).LoadAsync();

            return ToApplicationDto(application);
        }

        public async Task<List<AdoptionApplicationDto>> GetApplicationsForAdopterAsync(int adopterId)
        {
            return await _context.AdoptionApplications
                .Include(a => a.Animal)
                .Include(a => a.Adopter)
                .Where(a => a.AdopterId == adopterId)
                .OrderByDescending(a => a.ApplicationDate)
                .Select(a => ToApplicationDto(a))
                .ToListAsync();
        }

        public async Task<List<AdoptionApplicationDto>> GetApplicationsForBranchAsync(int? branchId, string? status)
        {
            var query = _context.AdoptionApplications
                .Include(a => a.Animal)
                .Include(a => a.Adopter)
                .AsQueryable();

            if (branchId.HasValue)
                query = query.Where(a => a.Animal!.BranchId == branchId.Value);

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AdoptionApplicationStatus>(status, true, out var parsedStatus))
                query = query.Where(a => a.Status == parsedStatus);

            return await query
                .OrderByDescending(a => a.ApplicationDate)
                .Select(a => ToApplicationDto(a))
                .ToListAsync();
        }

        public async Task<AdoptionApplicationDto?> GetApplicationByIdAsync(int id)
        {
            var app = await _context.AdoptionApplications
                .Include(a => a.Animal)
                .Include(a => a.Adopter)
                .FirstOrDefaultAsync(a => a.Id == id);

            return app is null ? null : ToApplicationDto(app);
        }

        public async Task<AdoptionDto?> ReviewApplicationAsync(int applicationId, int reviewerId, ReviewAdoptionApplicationDto dto)
        {
            var application = await _context.AdoptionApplications
                .Include(a => a.Animal)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application is null) return null;

            if (!dto.Approve)
            {
                application.Status = AdoptionApplicationStatus.Rejected;
                application.RejectionReason = dto.RejectionReason;

                // Free the animal back up if nothing else has claimed it
                if (application.Animal is not null && application.Animal.Status == AnimalStatus.Pending)
                    application.Animal.Status = AnimalStatus.Available;

                await _context.SaveChangesAsync();
                return null;
            }

            application.Status = AdoptionApplicationStatus.Approved;

            var adoption = new Adoption
            {
                ApplicationId = application.Id,
                AnimalId = application.AnimalId,
                AdopterId = application.AdopterId,
                ApprovedById = reviewerId,
                AdoptionDate = DateTime.UtcNow.Date
            };

            if (application.Animal is not null)
                application.Animal.Status = AnimalStatus.Adopted;

            _context.Adoptions.Add(adoption);
            await _context.SaveChangesAsync();

            await _context.Entry(adoption).Reference(a => a.Animal).LoadAsync();
            await _context.Entry(adoption).Reference(a => a.Adopter).LoadAsync();
            await _context.Entry(adoption).Reference(a => a.ApprovedBy).LoadAsync();

            return ToAdoptionDto(adoption);
        }

        public async Task<List<AdoptionDto>> GetAdoptionsAsync(int? branchId)
        {
            var query = _context.Adoptions
                .Include(a => a.Animal)
                .Include(a => a.Adopter)
                .Include(a => a.ApprovedBy)
                .AsQueryable();

            if (branchId.HasValue)
                query = query.Where(a => a.Animal!.BranchId == branchId.Value);

            return await query
                .OrderByDescending(a => a.AdoptionDate)
                .Select(a => ToAdoptionDto(a))
                .ToListAsync();
        }

        public async Task<AdoptionDto?> GetAdoptionByIdAsync(int id)
        {
            var adoption = await _context.Adoptions
                .Include(a => a.Animal)
                .Include(a => a.Adopter)
                .Include(a => a.ApprovedBy)
                .FirstOrDefaultAsync(a => a.Id == id);

            return adoption is null ? null : ToAdoptionDto(adoption);
        }

        public async Task<List<FollowUpDto>> GetFollowUpsForAdoptionAsync(int adoptionId)
        {
            return await _context.FollowUps
                .Include(f => f.Adoption).ThenInclude(a => a!.Animal)
                .Include(f => f.Adoption).ThenInclude(a => a!.Adopter)
                .Include(f => f.ConductedBy)
                .Where(f => f.AdoptionId == adoptionId)
                .OrderByDescending(f => f.FollowUpDate)
                .Select(f => ToFollowUpDto(f))
                .ToListAsync();
        }

        public async Task<FollowUpDto> CreateFollowUpAsync(int conductedById, CreateFollowUpDto dto)
        {
            var adoptionExists = await _context.Adoptions.AnyAsync(a => a.Id == dto.AdoptionId);
            if (!adoptionExists)
                throw new KeyNotFoundException($"Adoption with Id {dto.AdoptionId} was not found.");

            var followUp = new FollowUp
            {
                AdoptionId = dto.AdoptionId,
                ConductedById = conductedById,
                FollowUpDate = dto.FollowUpDate,
                Notes = dto.Notes,
                Status = Enum.Parse<FollowUpStatus>(dto.Status, ignoreCase: true)
            };

            _context.FollowUps.Add(followUp);
            await _context.SaveChangesAsync();

            await _context.Entry(followUp).Reference(f => f.Adoption).LoadAsync();
            await _context.Entry(followUp.Adoption!).Reference(a => a.Animal).LoadAsync();
            await _context.Entry(followUp.Adoption!).Reference(a => a.Adopter).LoadAsync();
            await _context.Entry(followUp).Reference(f => f.ConductedBy).LoadAsync();

            return ToFollowUpDto(followUp);
        }

        private static AdoptionApplicationDto ToApplicationDto(AdoptionApplication a) => new()
        {
            Id = a.Id,
            AnimalId = a.AnimalId,
            AnimalName = a.Animal?.Name,
            AdopterId = a.AdopterId,
            AdopterName = a.Adopter?.Name,
            VisitBookingId = a.VisitBookingId,
            ApplicationDate = a.ApplicationDate,
            Status = a.Status.ToString(),
            RejectionReason = a.RejectionReason
        };

        private static AdoptionDto ToAdoptionDto(Adoption a) => new()
        {
            Id = a.Id,
            ApplicationId = a.ApplicationId,
            AnimalId = a.AnimalId,
            AnimalName = a.Animal?.Name,
            AdopterId = a.AdopterId,
            AdopterName = a.Adopter?.Name,
            ApprovedById = a.ApprovedById,
            ApprovedByName = a.ApprovedBy?.Name,
            AdoptionDate = a.AdoptionDate
        };

        private static FollowUpDto ToFollowUpDto(FollowUp f) => new()
        {
            Id = f.Id,
            AdoptionId = f.AdoptionId,
            AnimalName = f.Adoption?.Animal?.Name,
            AdopterName = f.Adoption?.Adopter?.Name,
            ConductedById = f.ConductedById,
            ConductedByName = f.ConductedBy?.Name,
            FollowUpDate = f.FollowUpDate,
            Notes = f.Notes,
            Status = f.Status.ToString()
        };
    }
}
