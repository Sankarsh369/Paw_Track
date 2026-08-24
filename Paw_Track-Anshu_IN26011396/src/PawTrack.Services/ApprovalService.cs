using Microsoft.EntityFrameworkCore;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Core.Interfaces;
using PawTrack.Data;

namespace PawTrack.Services;

public class ApprovalService : IApprovalService
{
    private readonly PawTrackDbContext _context;
    private static readonly SemaphoreSlim _concurrencyLock = new(1, 1);

    public ApprovalService(PawTrackDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Approve an adoption application.
    /// Transactional execution with concurrency protection:
    /// 1. Validate application exists and status is 'Pending'
    /// 2. Validate staff user has branch authorization
    /// 3. Validate animal exists and status is 'Available'
    /// 4. Create Adoption record with ApprovedById set from currentUser
    /// 5. Update Animal.Status to 'Adopted'
    /// 6. Update Application.Status to 'Approved'
    /// 7. Flag all other open VisitBookings for this animal belonging to other adopters
    /// 8. Commit transaction
    /// </summary>
    public async Task<Adoption> ApproveApplicationAsync(int applicationId, User currentUser)
    {
        if (currentUser.Role != UserRole.OrgAdmin &&
            currentUser.Role != UserRole.BranchAdmin &&
            currentUser.Role != UserRole.RescueStaff)
        {
            throw new UnauthorizedAccessException("Unauthorized: Only shelter staff or admins can approve applications.");
        }

        await _concurrencyLock.WaitAsync();
        try
        {
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = _context.Database.IsInMemory()
                    ? null
                    : await _context.Database.BeginTransactionAsync();

                try
                {
                    // 1. Re-check Application state
                    var application = await _context.AdoptionApplications
                        .FirstOrDefaultAsync(app => app.Id == applicationId);

                    if (application == null)
                    {
                        throw new KeyNotFoundException($"Application with ID {applicationId} was not found.");
                    }

                    if (application.Status != ApplicationStatus.Pending)
                    {
                        throw new InvalidOperationException($"Application is no longer pending. Current status: {application.Status}.");
                    }

                    // 2. Re-check Animal state
                    var animal = await _context.Animals
                        .FirstOrDefaultAsync(a => a.Id == application.AnimalId);

                    if (animal == null)
                    {
                        throw new KeyNotFoundException($"Animal with ID {application.AnimalId} was not found.");
                    }

                    // Branch authorization check
                    if (currentUser.Role != UserRole.OrgAdmin && currentUser.BranchId != animal.BranchId)
                    {
                        throw new UnauthorizedAccessException("Access denied: Staff can only approve applications for their own branch.");
                    }

                    if (animal.Status != AnimalStatus.Available)
                    {
                        throw new InvalidOperationException($"Adoption could not be completed because the animal is no longer available. Current status: {animal.Status}.");
                    }

                    // 3. Create Adoption Record
                    var adoption = new Adoption
                    {
                        ApplicationId = application.Id,
                        AnimalId = animal.Id,
                        AdopterId = application.AdopterId,
                        ApprovedById = currentUser.Id, // Authenticated staff user ID
                        AdoptionDate = DateTime.UtcNow.Date,
                        Notes = $"Approved by {currentUser.Name} ({currentUser.Role})"
                    };

                    _context.Adoptions.Add(adoption);

                    // 4. Update Animal.Status -> Adopted
                    animal.Status = AnimalStatus.Adopted;

                    // 5. Update Application.Status -> Approved
                    application.Status = ApplicationStatus.Approved;

                    // 6. Flag other open VisitBookings for same animal belonging to other adopters
                    var openVisitsForAnimal = await _context.VisitBookings
                        .Where(v => v.AnimalId == animal.Id &&
                                    v.AdopterId != application.AdopterId &&
                                    (v.Status == VisitBookingStatus.Booked || v.Status == VisitBookingStatus.CheckedIn))
                        .ToListAsync();

                    foreach (var visit in openVisitsForAnimal)
                    {
                        visit.Status = VisitBookingStatus.FollowUpRequired;
                        visit.StaffNotes = (string.IsNullOrEmpty(visit.StaffNotes) ? "" : visit.StaffNotes + " | ") +
                            $"Flagged for staff follow-up: Animal #{animal.Id} ({animal.Name}) adopted by User #{application.AdopterId}.";
                    }

                    await _context.SaveChangesAsync();

                    if (transaction != null)
                    {
                        await transaction.CommitAsync();
                    }

                    return adoption;
                }
                catch
                {
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync();
                    }
                    throw;
                }
            });
        }
        finally
        {
            _concurrencyLock.Release();
        }
    }

    /// <summary>
    /// Reject an adoption application.
    /// Requires non-empty rejectionReason.
    /// State Machine: Pending -> Rejected.
    /// Retains application record for history.
    /// </summary>
    public async Task RejectApplicationAsync(int applicationId, string rejectionReason, User currentUser)
    {
        if (currentUser.Role != UserRole.OrgAdmin &&
            currentUser.Role != UserRole.BranchAdmin &&
            currentUser.Role != UserRole.RescueStaff)
        {
            throw new UnauthorizedAccessException("Unauthorized: Only shelter staff or admins can reject applications.");
        }

        if (string.IsNullOrWhiteSpace(rejectionReason))
        {
            throw new ArgumentException("Rejection reason is required.", nameof(rejectionReason));
        }

        await _concurrencyLock.WaitAsync();
        try
        {
            var application = await _context.AdoptionApplications
                .Include(app => app.Animal)
                .FirstOrDefaultAsync(app => app.Id == applicationId);

            if (application == null)
            {
                throw new KeyNotFoundException($"Application with ID {applicationId} was not found.");
            }

            if (application.Status != ApplicationStatus.Pending)
            {
                throw new InvalidOperationException($"Application is no longer pending. Current status: {application.Status}.");
            }

            if (currentUser.Role != UserRole.OrgAdmin && application.Animal != null && currentUser.BranchId != application.Animal.BranchId)
            {
                throw new UnauthorizedAccessException("Access denied: Staff can only reject applications for their own branch.");
            }

            // State transition
            application.Status = ApplicationStatus.Rejected;
            application.RejectionReason = rejectionReason.Trim();

            await _context.SaveChangesAsync();
        }
        finally
        {
            _concurrencyLock.Release();
        }
    }
}
