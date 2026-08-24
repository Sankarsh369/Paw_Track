import { dbContext } from '../data/dbContext';
import { AdoptionApplication, User } from '../data/schema';

export interface SubmitApplicationDto {
  animalId: number;
  visitBookingId?: number | null;
  notes?: string;
}

export class AdoptionService {
  /**
   * Submit an adoption application for an animal.
   * Validates:
   * Rule 1 — Animal must be "Available"
   * Rule 2 — Adopter cannot have an active (Pending) application for the same animal
   * Rule 3 — If VisitBookingId is provided:
   *          - VisitBooking must exist
   *          - VisitBooking.AdopterId == current adopter
   *          - VisitBooking.AnimalId == application AnimalId
   */
  public submitApplication(dto: SubmitApplicationDto, currentUser: User): AdoptionApplication {
    if (currentUser.role !== 'Adopter' && currentUser.role !== 'OrgAdmin') {
      // Adopters submit applications for themselves
    }

    const adopterId = currentUser.id;

    // 1. Fetch Animal & Validate Availability
    const animal = dbContext.animals.find((a) => a.id === dto.animalId);
    if (!animal) {
      throw new Error('Animal not found.');
    }

    if (animal.status !== 'Available') {
      throw new Error(`Animal is no longer available. Current status: ${animal.status}.`);
    }

    // 2. Prevent duplicate active application
    const existingActiveApp = dbContext.adoptionApplications.find(
      (app) => app.adopterId === adopterId && app.animalId === dto.animalId && app.status === 'Pending'
    );

    if (existingActiveApp) {
      throw new Error('An active application already exists for this adopter and animal.');
    }

    // 3. Validate VisitBooking integration if supplied
    if (dto.visitBookingId) {
      const visit = dbContext.visitBookings.find((v) => v.id === dto.visitBookingId);
      if (!visit) {
        throw new Error(`Visit booking with ID ${dto.visitBookingId} was not found.`);
      }

      if (visit.adopterId !== adopterId) {
        throw new Error('The selected visit does not belong to this adopter.');
      }

      if (visit.animalId !== dto.animalId) {
        throw new Error('The selected visit belongs to a different animal.');
      }
    }

    // Create Application
    const newApplication: AdoptionApplication = {
      id: dbContext.getNextId('adoptionApplications'),
      animalId: dto.animalId,
      adopterId: adopterId,
      visitBookingId: dto.visitBookingId || null,
      applicationDate: new Date().toISOString().split('T')[0],
      status: 'Pending',
      notes: dto.notes || ''
    };

    dbContext.adoptionApplications.push(newApplication);
    return newApplication;
  }

  /**
   * Get applications filtered by branch authorization and status
   */
  public getApplications(currentUser: User, statusFilter?: string, branchFilter?: number): any[] {
    let apps = [...dbContext.adoptionApplications];

    // Branch authorization scoping
    if (currentUser.role !== 'OrgAdmin' && currentUser.branchId !== null) {
      // Branch scoped staff only see applications for animals in their branch
      const branchAnimalIds = new Set(
        dbContext.animals.filter((a) => a.branchId === currentUser.branchId).map((a) => a.id)
      );
      apps = apps.filter((app) => branchAnimalIds.has(app.animalId));
    } else if (branchFilter) {
      const branchAnimalIds = new Set(
        dbContext.animals.filter((a) => a.branchId === branchFilter).map((a) => a.id)
      );
      apps = apps.filter((app) => branchAnimalIds.has(app.animalId));
    }

    if (statusFilter && statusFilter !== 'All') {
      apps = apps.filter((app) => app.status === statusFilter);
    }

    // Enrich with detailed information for Presentation layer
    return apps.map((app) => {
      const animal = dbContext.animals.find((a) => a.id === app.animalId);
      const adopter = dbContext.users.find((u) => u.id === app.adopterId);
      const branch = animal ? dbContext.branches.find((b) => b.id === animal.branchId) : null;
      const visit = app.visitBookingId
        ? dbContext.visitBookings.find((v) => v.id === app.visitBookingId)
        : null;
      const visitSlot = visit
        ? dbContext.visitSlots.find((s) => s.id === visit.visitSlotId)
        : null;

      return {
        ...app,
        animalName: animal?.name || 'Unknown',
        animalSpecies: animal?.species || '',
        animalBreed: animal?.breed || '',
        animalStatus: animal?.status || '',
        adopterName: adopter?.name || 'Unknown',
        adopterEmail: adopter?.email || '',
        branchId: animal?.branchId,
        branchName: branch?.name || 'Unknown',
        linkedVisit: visit
          ? {
              id: visit.id,
              bookingDate: visit.bookingDate,
              status: visit.status,
              slotDate: visitSlot?.date,
              slotTime: visitSlot ? `${visitSlot.startTime} - ${visitSlot.endTime}` : ''
            }
          : null
      };
    });
  }

  /**
   * Get application by ID with branch security check
   */
  public getApplicationById(id: number, currentUser: User): any {
    const apps = this.getApplications(currentUser);
    const app = apps.find((a) => a.id === id);
    if (!app) {
      throw new Error(`Application with ID ${id} not found or access denied.`);
    }
    return app;
  }
}

export const adoptionService = new AdoptionService();
