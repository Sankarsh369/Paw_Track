using Microsoft.EntityFrameworkCore;
using PawTrack.Core.DTOs;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Core.Interfaces;
using PawTrack.Data;

namespace PawTrack.Services;

public class AdoptionService : IAdoptionService
{
    private readonly PawTrackDbContext _context;

    public AdoptionService(PawTrackDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Submit an adoption application for an animal.
    /// Validates:
    /// Rule 1 — Animal must be "Available"
    /// Rule 2 — Adopter cannot have an active (Pending) application for the same animal
    /// Rule 3 — If VisitBookingId is provided:
    ///          - VisitBooking must exist
    ///          - VisitBooking.AdopterId == current adopter
    ///          - VisitBooking.AnimalId == application AnimalId
    /// </summary>
    public async Task<AdoptionApplication> SubmitApplicationAsync(SubmitApplicationDto dto, User currentUser)
    {
        var adopterId = currentUser.Id;

        // 1. Fetch Animal & Validate Availability
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == dto.AnimalId);
        if (animal == null)
        {
            throw new KeyNotFoundException("Animal not found.");
        }

        if (animal.Status != AnimalStatus.Available)
        {
            throw new InvalidOperationException($"Animal is no longer available. Current status: {animal.Status}.");
        }

        // 2. Prevent duplicate active (Pending) application for the same adopter and animal
        var existingActiveApp = await _context.AdoptionApplications
            .AnyAsync(app => app.AdopterId == adopterId && app.AnimalId == dto.AnimalId && app.Status == ApplicationStatus.Pending);

        if (existingActiveApp)
        {
            throw new InvalidOperationException("An active application already exists for this adopter and animal.");
        }

        // 3. Validate VisitBooking integration if supplied
        if (dto.VisitBookingId.HasValue && dto.VisitBookingId.Value > 0)
        {
            var visit = await _context.VisitBookings.FirstOrDefaultAsync(v => v.Id == dto.VisitBookingId.Value);
            if (visit == null)
            {
                throw new KeyNotFoundException($"Visit booking with ID {dto.VisitBookingId.Value} was not found.");
            }

            if (visit.AdopterId != adopterId)
            {
                throw new InvalidOperationException("The selected visit does not belong to this adopter.");
            }

            if (visit.AnimalId != dto.AnimalId)
            {
                throw new InvalidOperationException("The selected visit belongs to a different animal.");
            }
        }

        var newApplication = new AdoptionApplication
        {
            AnimalId = dto.AnimalId,
            AdopterId = adopterId,
            VisitBookingId = dto.VisitBookingId > 0 ? dto.VisitBookingId : null,
            ApplicationDate = DateTime.UtcNow.Date,
            Status = ApplicationStatus.Pending,
            Notes = dto.Notes ?? string.Empty
        };

        _context.AdoptionApplications.Add(newApplication);
        await _context.SaveChangesAsync();

        return newApplication;
    }

    /// <summary>
    /// Get applications filtered by branch authorization and status
    /// </summary>
    public async Task<List<ApplicationResponseDto>> GetApplicationsAsync(User currentUser, ApplicationStatus? statusFilter = null, int? branchFilter = null)
    {
        var query = _context.AdoptionApplications
            .Include(app => app.Animal)
                .ThenInclude(an => an!.Branch)
            .Include(app => app.Adopter)
            .Include(app => app.VisitBooking)
                .ThenInclude(v => v!.VisitSlot)
            .AsQueryable();

        // Branch authorization scoping
        if (currentUser.Role == UserRole.Adopter)
        {
            query = query.Where(app => app.AdopterId == currentUser.Id);
        }
        else if (currentUser.Role != UserRole.OrgAdmin && currentUser.BranchId.HasValue)
        {
            query = query.Where(app => app.Animal != null && app.Animal.BranchId == currentUser.BranchId.Value);
        }
        else if (branchFilter.HasValue && branchFilter.Value > 0)
        {
            query = query.Where(app => app.Animal != null && app.Animal.BranchId == branchFilter.Value);
        }

        if (statusFilter.HasValue)
        {
            query = query.Where(app => app.Status == statusFilter.Value);
        }

        var apps = await query.OrderByDescending(app => app.ApplicationDate).ToListAsync();

        return apps.Select(app => new ApplicationResponseDto
        {
            Id = app.Id,
            AnimalId = app.AnimalId,
            AnimalName = app.Animal?.Name ?? "Unknown",
            AnimalSpecies = app.Animal?.Species ?? string.Empty,
            AnimalBreed = app.Animal?.Breed ?? string.Empty,
            AnimalStatus = app.Animal?.Status ?? AnimalStatus.UnderAssessment,
            AdopterId = app.AdopterId,
            AdopterName = app.Adopter?.Name ?? "Unknown",
            AdopterEmail = app.Adopter?.Email ?? string.Empty,
            BranchId = app.Animal?.BranchId,
            BranchName = app.Animal?.Branch?.Name ?? "Unknown",
            VisitBookingId = app.VisitBookingId,
            ApplicationDate = app.ApplicationDate,
            Status = app.Status,
            RejectionReason = app.RejectionReason,
            Notes = app.Notes,
            LinkedVisit = app.VisitBooking != null ? new LinkedVisitDto
            {
                Id = app.VisitBooking.Id,
                BookingDate = app.VisitBooking.BookingDate,
                Status = app.VisitBooking.Status,
                SlotDate = app.VisitBooking.VisitSlot?.Date,
                SlotTime = app.VisitBooking.VisitSlot != null
                    ? $"{app.VisitBooking.VisitSlot.StartTime:hh\\:mm} - {app.VisitBooking.VisitSlot.EndTime:hh\\:mm}"
                    : string.Empty
            } : null
        }).ToList();
    }

    /// <summary>
    /// Get application by ID with branch security check
    /// </summary>
    public async Task<ApplicationResponseDto> GetApplicationByIdAsync(int id, User currentUser)
    {
        var apps = await GetApplicationsAsync(currentUser);
        var app = apps.FirstOrDefault(a => a.Id == id);
        if (app == null)
        {
            throw new KeyNotFoundException($"Application with ID {id} not found or access denied.");
        }
        return app;
    }
}
