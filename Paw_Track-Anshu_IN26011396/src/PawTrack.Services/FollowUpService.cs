using Microsoft.EntityFrameworkCore;
using PawTrack.Core.DTOs;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Core.Interfaces;
using PawTrack.Data;

namespace PawTrack.Services;

public class FollowUpService : IFollowUpService
{
    private readonly PawTrackDbContext _context;

    public FollowUpService(PawTrackDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Schedule a new post-adoption follow-up check-in.
    /// Validates:
    /// - Adoption exists
    /// - Branch authorization (Adoption -> Animal -> Branch)
    /// - FollowUpDate is provided
    /// - Notes are provided
    /// Creates a new FollowUp record without modifying previous historical records.
    /// </summary>
    public async Task<FollowUp> ScheduleFollowUpAsync(ScheduleFollowUpDto dto, User currentUser)
    {
        if (currentUser.Role != UserRole.OrgAdmin &&
            currentUser.Role != UserRole.BranchAdmin &&
            currentUser.Role != UserRole.RescueStaff &&
            currentUser.Role != UserRole.Veterinarian)
        {
            throw new UnauthorizedAccessException("Unauthorized: Only staff can schedule follow-ups.");
        }

        if (string.IsNullOrWhiteSpace(dto.Notes))
        {
            throw new ArgumentException("Follow-up notes are required.", nameof(dto.Notes));
        }

        var adoption = await _context.Adoptions
            .Include(a => a.Animal)
            .FirstOrDefaultAsync(a => a.Id == dto.AdoptionId);

        if (adoption == null)
        {
            throw new KeyNotFoundException($"Adoption with ID {dto.AdoptionId} was not found.");
        }

        // Branch authorization check
        if (currentUser.Role != UserRole.OrgAdmin && adoption.Animal != null && currentUser.BranchId != adoption.Animal.BranchId)
        {
            throw new UnauthorizedAccessException("Access denied: Staff can only schedule follow-ups for their own branch.");
        }

        var newFollowUp = new FollowUp
        {
            AdoptionId = dto.AdoptionId,
            ConductedById = currentUser.Id,
            FollowUpDate = dto.FollowUpDate,
            Notes = dto.Notes.Trim(),
            Status = FollowUpStatus.Scheduled
        };

        _context.FollowUps.Add(newFollowUp);
        await _context.SaveChangesAsync();

        return newFollowUp;
    }

    /// <summary>
    /// Complete a scheduled follow-up
    /// </summary>
    public async Task<FollowUp> CompleteFollowUpAsync(int followUpId, string? notesUpdate, User currentUser)
    {
        if (currentUser.Role != UserRole.OrgAdmin &&
            currentUser.Role != UserRole.BranchAdmin &&
            currentUser.Role != UserRole.RescueStaff &&
            currentUser.Role != UserRole.Veterinarian)
        {
            throw new UnauthorizedAccessException("Unauthorized: Only staff can complete follow-ups.");
        }

        var followUp = await _context.FollowUps
            .Include(f => f.Adoption)
                .ThenInclude(a => a!.Animal)
            .FirstOrDefaultAsync(f => f.Id == followUpId);

        if (followUp == null)
        {
            throw new KeyNotFoundException($"Follow-up with ID {followUpId} was not found.");
        }

        if (currentUser.Role != UserRole.OrgAdmin && followUp.Adoption?.Animal != null && currentUser.BranchId != followUp.Adoption.Animal.BranchId)
        {
            throw new UnauthorizedAccessException("Access denied: Staff can only manage follow-ups for their own branch.");
        }

        followUp.Status = FollowUpStatus.Completed;
        if (!string.IsNullOrWhiteSpace(notesUpdate))
        {
            followUp.Notes = $"{followUp.Notes} [Updated: {notesUpdate.Trim()}]";
        }

        await _context.SaveChangesAsync();
        return followUp;
    }

    /// <summary>
    /// Get follow-up history for a specific adoption
    /// </summary>
    public async Task<List<FollowUpResponseDto>> GetFollowUpHistoryAsync(int adoptionId, User currentUser)
    {
        var adoption = await _context.Adoptions
            .Include(a => a.Animal)
            .FirstOrDefaultAsync(a => a.Id == adoptionId);

        if (adoption == null)
        {
            throw new KeyNotFoundException($"Adoption with ID {adoptionId} was not found.");
        }

        // Branch authorization check
        if (currentUser.Role != UserRole.OrgAdmin && currentUser.Role != UserRole.Adopter && adoption.Animal != null && currentUser.BranchId != adoption.Animal.BranchId)
        {
            throw new UnauthorizedAccessException("Access denied: Staff can only view follow-ups for their own branch.");
        }

        // Adopter can only view follow-ups for their own adoption
        if (currentUser.Role == UserRole.Adopter && adoption.AdopterId != currentUser.Id)
        {
            throw new UnauthorizedAccessException("Access denied: You can only view follow-ups for your own adoptions.");
        }

        var followUps = await _context.FollowUps
            .Include(f => f.ConductedBy)
            .Where(f => f.AdoptionId == adoptionId)
            .OrderBy(f => f.FollowUpDate)
            .ToListAsync();

        return followUps.Select(f => new FollowUpResponseDto
        {
            Id = f.Id,
            AdoptionId = f.AdoptionId,
            ConductedById = f.ConductedById,
            ConductedByName = f.ConductedBy?.Name ?? "Staff Member",
            ConductedByRole = f.ConductedBy?.Role.ToString() ?? "Staff",
            FollowUpDate = f.FollowUpDate,
            Notes = f.Notes,
            Status = f.Status
        }).ToList();
    }

    /// <summary>
    /// Get all adoptions with enriched detail for Adoption & Follow-Up management page
    /// </summary>
    public async Task<List<AdoptionResponseDto>> GetAdoptionsAsync(User currentUser)
    {
        var query = _context.Adoptions
            .Include(a => a.Animal)
                .ThenInclude(an => an!.Branch)
            .Include(a => a.Adopter)
            .Include(a => a.ApprovedBy)
            .Include(a => a.FollowUps)
                .ThenInclude(f => f.ConductedBy)
            .AsQueryable();

        // Branch scoping
        if (currentUser.Role == UserRole.Adopter)
        {
            query = query.Where(a => a.AdopterId == currentUser.Id);
        }
        else if (currentUser.Role != UserRole.OrgAdmin && currentUser.BranchId.HasValue)
        {
            query = query.Where(a => a.Animal != null && a.Animal.BranchId == currentUser.BranchId.Value);
        }

        var adoptions = await query.OrderByDescending(a => a.AdoptionDate).ToListAsync();

        return adoptions.Select(a => new AdoptionResponseDto
        {
            Id = a.Id,
            ApplicationId = a.ApplicationId,
            AnimalId = a.AnimalId,
            AnimalName = a.Animal?.Name ?? "Unknown",
            AnimalSpecies = a.Animal?.Species ?? string.Empty,
            AnimalBreed = a.Animal?.Breed ?? string.Empty,
            AnimalImageUrl = a.Animal?.ImageUrl,
            AdopterId = a.AdopterId,
            AdopterName = a.Adopter?.Name ?? "Unknown",
            AdopterEmail = a.Adopter?.Email ?? string.Empty,
            ApprovedById = a.ApprovedById,
            ApprovedByName = a.ApprovedBy?.Name ?? "Staff Member",
            AdoptionDate = a.AdoptionDate,
            Notes = a.Notes,
            BranchId = a.Animal?.BranchId,
            BranchName = a.Animal?.Branch?.Name ?? "Unknown",
            FollowUpCount = a.FollowUps.Count,
            FollowUpHistory = a.FollowUps
                .OrderBy(f => f.FollowUpDate)
                .Select(f => new FollowUpResponseDto
                {
                    Id = f.Id,
                    AdoptionId = f.AdoptionId,
                    ConductedById = f.ConductedById,
                    ConductedByName = f.ConductedBy?.Name ?? "Staff Member",
                    ConductedByRole = f.ConductedBy?.Role.ToString() ?? "Staff",
                    FollowUpDate = f.FollowUpDate,
                    Notes = f.Notes,
                    Status = f.Status
                }).ToList()
        }).ToList();
    }

    /// <summary>
    /// Get single adoption by ID
    /// </summary>
    public async Task<AdoptionResponseDto> GetAdoptionByIdAsync(int id, User currentUser)
    {
        var adoptions = await GetAdoptionsAsync(currentUser);
        var adoption = adoptions.FirstOrDefault(a => a.Id == id);
        if (adoption == null)
        {
            throw new KeyNotFoundException($"Adoption with ID {id} was not found or access denied.");
        }
        return adoption;
    }
}
