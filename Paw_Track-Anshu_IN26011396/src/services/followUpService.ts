import { dbContext } from '../data/dbContext';
import { FollowUp, User } from '../data/schema';

export interface ScheduleFollowUpDto {
  adoptionId: number;
  followUpDate: string;
  notes: string;
}

export class FollowUpService {
  /**
   * Schedule a new post-adoption follow-up check-in.
   * Validates:
   * - Adoption exists
   * - Branch authorization (Adoption -> Animal -> Branch)
   * - FollowUpDate is provided
   * - Notes are provided
   * Creates a new FollowUp record without modifying previous historical records.
   */
  public scheduleFollowUp(dto: ScheduleFollowUpDto, currentUser: User): FollowUp {
    if (
      currentUser.role !== 'OrgAdmin' &&
      currentUser.role !== 'BranchAdmin' &&
      currentUser.role !== 'RescueStaff' &&
      currentUser.role !== 'Veterinarian'
    ) {
      throw new Error('Unauthorized: Only staff can schedule follow-ups.');
    }

    if (!dto.followUpDate || dto.followUpDate.trim() === '') {
      throw new Error('Follow-up date is required.');
    }

    if (!dto.notes || dto.notes.trim() === '') {
      throw new Error('Follow-up notes are required.');
    }

    const adoption = dbContext.adoptions.find((a) => a.id === dto.adoptionId);
    if (!adoption) {
      throw new Error(`Adoption with ID ${dto.adoptionId} was not found.`);
    }

    const animal = dbContext.animals.find((a) => a.id === adoption.animalId);
    if (!animal) {
      throw new Error(`Animal with ID ${adoption.animalId} was not found.`);
    }

    // Branch authorization check
    if (currentUser.role !== 'OrgAdmin' && currentUser.branchId !== animal.branchId) {
      throw new Error('Access denied: Staff can only schedule follow-ups for their own branch.');
    }

    const newFollowUp: FollowUp = {
      id: dbContext.getNextId('followUps'),
      adoptionId: dto.adoptionId,
      conductedById: currentUser.id,
      followUpDate: dto.followUpDate,
      notes: dto.notes.trim(),
      status: 'Scheduled'
    };

    dbContext.followUps.push(newFollowUp);
    return newFollowUp;
  }

  /**
   * Complete a scheduled follow-up
   */
  public completeFollowUp(followUpId: number, notesUpdate: string | undefined, currentUser: User): FollowUp {
    if (
      currentUser.role !== 'OrgAdmin' &&
      currentUser.role !== 'BranchAdmin' &&
      currentUser.role !== 'RescueStaff' &&
      currentUser.role !== 'Veterinarian'
    ) {
      throw new Error('Unauthorized: Only staff can complete follow-ups.');
    }

    const followUp = dbContext.followUps.find((f) => f.id === followUpId);
    if (!followUp) {
      throw new Error(`Follow-up with ID ${followUpId} was not found.`);
    }

    const adoption = dbContext.adoptions.find((a) => a.id === followUp.adoptionId);
    const animal = adoption ? dbContext.animals.find((a) => a.id === adoption.animalId) : null;

    if (currentUser.role !== 'OrgAdmin' && animal && currentUser.branchId !== animal.branchId) {
      throw new Error('Access denied: Staff can only manage follow-ups for their own branch.');
    }

    followUp.status = 'Completed';
    if (notesUpdate && notesUpdate.trim()) {
      followUp.notes = `${followUp.notes} [Updated: ${notesUpdate.trim()}]`;
    }

    return followUp;
  }

  /**
   * Get follow-up history for a specific adoption
   */
  public getFollowUpHistory(adoptionId: number, currentUser: User): any[] {
    const adoption = dbContext.adoptions.find((a) => a.id === adoptionId);
    if (!adoption) {
      throw new Error(`Adoption with ID ${adoptionId} was not found.`);
    }

    const animal = dbContext.animals.find((a) => a.id === adoption.animalId);

    // Branch authorization check
    if (currentUser.role !== 'OrgAdmin' && currentUser.role !== 'Adopter' && animal && currentUser.branchId !== animal.branchId) {
      throw new Error('Access denied: Staff can only view follow-ups for their own branch.');
    }

    // Adopter can only view follow-ups for their own adoption
    if (currentUser.role === 'Adopter' && adoption.adopterId !== currentUser.id) {
      throw new Error('Access denied: You can only view follow-ups for your own adoptions.');
    }

    const followUps = dbContext.followUps.filter((f) => f.adoptionId === adoptionId);

    return followUps.map((f) => {
      const conductedBy = dbContext.users.find((u) => u.id === f.conductedById);
      return {
        ...f,
        conductedByName: conductedBy?.name || 'Staff Member',
        conductedByRole: conductedBy?.role || 'Staff'
      };
    });
  }

  /**
   * Get all adoptions with enriched detail for Adoption & Follow-Up management page
   */
  public getAdoptions(currentUser: User): any[] {
    let adoptions = [...dbContext.adoptions];

    // Branch scoping
    if (currentUser.role !== 'OrgAdmin' && currentUser.role !== 'Adopter' && currentUser.branchId !== null) {
      const branchAnimalIds = new Set(
        dbContext.animals.filter((a) => a.branchId === currentUser.branchId).map((a) => a.id)
      );
      adoptions = adoptions.filter((a) => branchAnimalIds.has(a.animalId));
    } else if (currentUser.role === 'Adopter') {
      adoptions = adoptions.filter((a) => a.adopterId === currentUser.id);
    }

    return adoptions.map((a) => {
      const animal = dbContext.animals.find((an) => an.id === a.animalId);
      const adopter = dbContext.users.find((u) => u.id === a.adopterId);
      const approvedBy = dbContext.users.find((u) => u.id === a.approvedById);
      const branch = animal ? dbContext.branches.find((b) => b.id === animal.branchId) : null;
      const history = this.getFollowUpHistory(a.id, currentUser);

      return {
        ...a,
        animalName: animal?.name || 'Unknown',
        animalSpecies: animal?.species || '',
        animalBreed: animal?.breed || '',
        animalImageUrl: animal?.imageUrl || '',
        adopterName: adopter?.name || 'Unknown',
        adopterEmail: adopter?.email || '',
        approvedByName: approvedBy?.name || 'Staff Member',
        branchId: animal?.branchId,
        branchName: branch?.name || 'Unknown',
        followUpCount: history.length,
        followUpHistory: history
      };
    });
  }
}

export const followUpService = new FollowUpService();
