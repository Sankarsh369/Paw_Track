import { dbContext } from '../data/dbContext';
import { Adoption, User } from '../data/schema';

export class ApprovalService {
  /**
   * Approve an adoption application.
   * Transactional execution with concurrency protection:
   * 1. Validate application exists and status is 'Pending'
   * 2. Validate staff user has branch authorization
   * 3. Validate animal exists and status is 'Available'
   * 4. Create Adoption record with ApprovedById set from currentUser
   * 5. Update Animal.Status to 'Adopted'
   * 6. Update Application.Status to 'Approved'
   * 7. Flag all other open VisitBookings for this animal belonging to other adopters
   * 8. Commit transaction
   */
  public async approveApplication(applicationId: number, currentUser: User): Promise<Adoption> {
    // Role check: Only BranchAdmin, RescueStaff, or OrgAdmin can approve
    if (
      currentUser.role !== 'OrgAdmin' &&
      currentUser.role !== 'BranchAdmin' &&
      currentUser.role !== 'RescueStaff'
    ) {
      throw new Error('Unauthorized: Only shelter staff or admins can approve applications.');
    }

    // Wrap entire approval process in concurrency lock and atomic transaction
    return await dbContext.acquireLock(async () => {
      dbContext.beginTransaction();

      try {
        // 1. Re-check Application state
        const application = dbContext.adoptionApplications.find((app) => app.id === applicationId);
        if (!application) {
          throw new Error(`Application with ID ${applicationId} was not found.`);
        }

        if (application.status !== 'Pending') {
          throw new Error(
            `Application is no longer pending. Current status: ${application.status}.`
          );
        }

        // 2. Re-check Animal state
        const animal = dbContext.animals.find((a) => a.id === application.animalId);
        if (!animal) {
          throw new Error(`Animal with ID ${application.animalId} was not found.`);
        }

        // Branch authorization check
        if (currentUser.role !== 'OrgAdmin' && currentUser.branchId !== animal.branchId) {
          throw new Error('Access denied: Staff can only approve applications for their own branch.');
        }

        if (animal.status !== 'Available') {
          throw new Error(
            `Adoption could not be completed because the animal is no longer available. Current status: ${animal.status}.`
          );
        }

        // 3. Create Adoption Record
        const adoption: Adoption = {
          id: dbContext.getNextId('adoptions'),
          applicationId: application.id,
          animalId: animal.id,
          adopterId: application.adopterId,
          approvedById: currentUser.id, // Authenticated staff user ID
          adoptionDate: new Date().toISOString().split('T')[0],
          notes: `Approved by ${currentUser.name} (${currentUser.role})`
        };
        dbContext.adoptions.push(adoption);

        // 4. Update Animal.Status -> Adopted
        animal.status = 'Adopted';

        // 5. Update Application.Status -> Approved
        application.status = 'Approved';

        // 6. Flag other open VisitBookings for same animal belonging to other adopters
        const openVisitsForAnimal = dbContext.visitBookings.filter(
          (v) =>
            v.animalId === animal.id &&
            v.adopterId !== application.adopterId &&
            (v.status === 'Booked' || v.status === 'CheckedIn')
        );

        for (const visit of openVisitsForAnimal) {
          visit.status = 'FollowUpRequired';
          visit.staffNotes = (visit.staffNotes ? visit.staffNotes + ' | ' : '') +
            `Flagged for staff follow-up: Animal #${animal.id} (${animal.name}) adopted by User #${application.adopterId}.`;
        }

        // Commit logical transaction
        dbContext.commitTransaction();
        return adoption;
      } catch (error) {
        dbContext.rollbackTransaction();
        throw error;
      }
    });
  }

  /**
   * Reject an adoption application.
   * Requires non-empty rejectionReason.
   * State Machine: Pending -> Rejected.
   * Retains application record for history.
   */
  public async rejectApplication(
    applicationId: number,
    rejectionReason: string,
    currentUser: User
  ): Promise<void> {
    if (
      currentUser.role !== 'OrgAdmin' &&
      currentUser.role !== 'BranchAdmin' &&
      currentUser.role !== 'RescueStaff'
    ) {
      throw new Error('Unauthorized: Only shelter staff or admins can reject applications.');
    }

    if (!rejectionReason || rejectionReason.trim() === '') {
      throw new Error('Rejection reason is required.');
    }

    return await dbContext.acquireLock(async () => {
      dbContext.beginTransaction();

      try {
        const application = dbContext.adoptionApplications.find((app) => app.id === applicationId);
        if (!application) {
          throw new Error(`Application with ID ${applicationId} was not found.`);
        }

        if (application.status !== 'Pending') {
          throw new Error(
            `Application is no longer pending. Current status: ${application.status}.`
          );
        }

        const animal = dbContext.animals.find((a) => a.id === application.animalId);
        if (!animal) {
          throw new Error(`Animal with ID ${application.animalId} was not found.`);
        }

        if (currentUser.role !== 'OrgAdmin' && currentUser.branchId !== animal.branchId) {
          throw new Error('Access denied: Staff can only reject applications for their own branch.');
        }

        // State transition
        application.status = 'Rejected';
        application.rejectionReason = rejectionReason.trim();

        dbContext.commitTransaction();
      } catch (error) {
        dbContext.rollbackTransaction();
        throw error;
      }
    });
  }
}

export const approvalService = new ApprovalService();
