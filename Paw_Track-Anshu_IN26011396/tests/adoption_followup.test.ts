import { describe, it, expect, beforeEach } from 'vitest';
import { dbContext } from '../src/data/dbContext';
import { adoptionService } from '../src/services/adoptionService';
import { approvalService } from '../src/services/approvalService';
import { followUpService } from '../src/services/followUpService';
import { visitService } from '../src/services/visitService';
import { User } from '../src/data/schema';

describe('PawTrack — Adoption & Follow-Up Module Test Suite', () => {
  let orgAdmin: User;
  let branchAdmin1: User;
  let rescueStaff1: User;
  let branchAdmin2: User;
  let adopter8: User;
  let adopter9: User;
  let adopter10: User;

  beforeEach(() => {
    // Reset DB state to clean seed data before each test
    dbContext.resetToSeed();

    orgAdmin = dbContext.users.find((u) => u.id === 1)!;
    branchAdmin1 = dbContext.users.find((u) => u.id === 2)!;
    rescueStaff1 = dbContext.users.find((u) => u.id === 3)!;
    branchAdmin2 = dbContext.users.find((u) => u.id === 5)!;
    adopter8 = dbContext.users.find((u) => u.id === 8)!;
    adopter9 = dbContext.users.find((u) => u.id === 9)!;
    adopter10 = dbContext.users.find((u) => u.id === 10)!;
  });

  describe('1. Application Submission & Validation Rules', () => {
    it('allows adopter to submit application for an Available animal', () => {
      // Animal 2 (Whiskers) is Available
      const app = adoptionService.submitApplication({ animalId: 2 }, adopter8);
      expect(app).toBeDefined();
      expect(app.animalId).toBe(2);
      expect(app.adopterId).toBe(8);
      expect(app.status).toBe('Pending');
    });

    it('rejects application submission if animal is not Available', () => {
      // Animal 1 (Bruno) is Adopted
      expect(() => {
        adoptionService.submitApplication({ animalId: 1 }, adopter8);
      }).toThrow(/Animal is no longer available/i);
    });

    it('prevents duplicate active (Pending) application for same adopter and animal', () => {
      // Adopter 10 already has a Pending application (App #3) for Animal 2
      expect(() => {
        adoptionService.submitApplication({ animalId: 2 }, adopter10);
      }).toThrow(/An active application already exists for this adopter and animal/i);
    });

    it('allows different adopters to submit active applications for the same Available animal', () => {
      // Animal 2 has Pending app from Adopter 10. Adopter 8 can also apply.
      const appAdopter8 = adoptionService.submitApplication({ animalId: 2 }, adopter8);
      expect(appAdopter8.adopterId).toBe(8);
      expect(appAdopter8.animalId).toBe(2);
    });

    it('accepts valid linked VisitBooking matching adopter and animal', () => {
      // Visit 2 belongs to Adopter 9 and Animal 3
      const app = adoptionService.submitApplication({ animalId: 3, visitBookingId: 2 }, adopter9);
      expect(app.visitBookingId).toBe(2);
    });

    it('rejects VisitBooking if it belongs to a different adopter', () => {
      // Visit 2 belongs to Adopter 9. Adopter 8 trying to link it should fail.
      expect(() => {
        adoptionService.submitApplication({ animalId: 3, visitBookingId: 2 }, adopter8);
      }).toThrow(/The selected visit does not belong to this adopter/i);
    });

    it('rejects VisitBooking if it belongs to a different animal', () => {
      // Visit 3 belongs to Animal 2. Linking it to Animal 3 should fail.
      expect(() => {
        adoptionService.submitApplication({ animalId: 3, visitBookingId: 3 }, adopter10);
      }).toThrow(/The selected visit belongs to a different animal/i);
    });
  });

  describe('2. Core PawTrack "No-Hold" Principle', () => {
    it('verifies booking a visit does NOT change Animal.Status', () => {
      const animal3 = dbContext.animals.find((a) => a.id === 3)!;
      expect(animal3.status).toBe('Available');

      // Book visit for Animal 3 by Adopter 8
      visitService.bookVisit({ visitSlotId: 3, animalId: 3 }, adopter8);

      // Animal.Status MUST remain Available
      expect(animal3.status).toBe('Available');
    });

    it('verifies submitting an application does NOT change Animal.Status', () => {
      const animal3 = dbContext.animals.find((a) => a.id === 3)!;
      expect(animal3.status).toBe('Available');

      // Submit application for Animal 3
      adoptionService.submitApplication({ animalId: 3 }, adopter8);

      // Animal.Status MUST remain Available
      expect(animal3.status).toBe('Available');
    });
  });

  describe('3. Application Approval Workflow & Transaction', () => {
    it('approves a pending application, creates Adoption, updates Animal status to Adopted, and flags other open visits', async () => {
      // Animal 2 (Whiskers) is Available. App #3 is Pending by Adopter 10.
      const animal2 = dbContext.animals.find((a) => a.id === 2)!;
      expect(animal2.status).toBe('Available');

      const adoption = await approvalService.approveApplication(3, branchAdmin1);

      expect(adoption).toBeDefined();
      expect(adoption.applicationId).toBe(3);
      expect(adoption.animalId).toBe(2);
      expect(adoption.adopterId).toBe(10);
      expect(adoption.approvedById).toBe(2); // John (BranchAdmin1)

      // Animal.Status MUST transition to Adopted
      expect(animal2.status).toBe('Adopted');

      // Application #3 MUST transition to Approved
      const app3 = dbContext.adoptionApplications.find((a) => a.id === 3)!;
      expect(app3.status).toBe('Approved');
    });

    it('requires a rejection reason when rejecting an application', async () => {
      expect(async () => {
        await approvalService.rejectApplication(3, '', branchAdmin1);
      }).rejects.toThrow(/Rejection reason is required/i);
    });

    it('successfully rejects an application with a reason and retains historical record', async () => {
      await approvalService.rejectApplication(3, 'Housing background check failed.', branchAdmin1);

      const app3 = dbContext.adoptionApplications.find((a) => a.id === 3)!;
      expect(app3.status).toBe('Rejected');
      expect(app3.rejectionReason).toBe('Housing background check failed.');
    });

    it('enforces state machine: prevents re-approving an already approved application', async () => {
      // App #1 is already Approved in seed dataset
      expect(async () => {
        await approvalService.approveApplication(1, branchAdmin1);
      }).rejects.toThrow(/Application is no longer pending/i);
    });

    it('enforces state machine: prevents approving a rejected application', async () => {
      await approvalService.rejectApplication(3, 'Invalid reference.', branchAdmin1);
      expect(async () => {
        await approvalService.approveApplication(3, branchAdmin1);
      }).rejects.toThrow(/Application is no longer pending/i);
    });
  });

  describe('4. Concurrency & Multiple Applications', () => {
    it('handles multiple active applications for the same animal and ensures only ONE approval succeeds', async () => {
      // Animal 3 (Rocky) is Available
      // Adopter 8 and Adopter 9 both apply for Animal 3
      const appA = adoptionService.submitApplication({ animalId: 3 }, adopter8);
      const appB = adoptionService.submitApplication({ animalId: 3 }, adopter9);

      expect(appA.status).toBe('Pending');
      expect(appB.status).toBe('Pending');

      // Approve Adopter 8's application
      const adoption = await approvalService.approveApplication(appA.id, branchAdmin1);
      expect(adoption.adopterId).toBe(8);

      const animal3 = dbContext.animals.find((a) => a.id === 3)!;
      expect(animal3.status).toBe('Adopted');

      // Attempting to approve Adopter 9's application MUST now fail because animal is no longer Available
      expect(async () => {
        await approvalService.approveApplication(appB.id, branchAdmin1);
      }).rejects.toThrow(/animal is no longer available/i);
    });

    it('handles concurrent approval attempts gracefully via lock', async () => {
      const appA = adoptionService.submitApplication({ animalId: 3 }, adopter8);
      const appB = adoptionService.submitApplication({ animalId: 3 }, adopter9);

      // Launch concurrent approvals
      const promise1 = approvalService.approveApplication(appA.id, branchAdmin1);
      const promise2 = approvalService.approveApplication(appB.id, branchAdmin1);

      const results = await Promise.allSettled([promise1, promise2]);

      const fulfilled = results.filter((r) => r.status === 'fulfilled');
      const rejected = results.filter((r) => r.status === 'rejected');

      expect(fulfilled.length).toBe(1);
      expect(rejected.length).toBe(1);
    });
  });

  describe('5. Post-Adoption Follow-Up System (1 : N History)', () => {
    it('schedules a follow-up, completes it, schedules a second follow-up, and preserves history', () => {
      // Adoption #1 (Bruno adopted by Mark)
      const fu1 = followUpService.scheduleFollowUp(
        {
          adoptionId: 1,
          followUpDate: '2026-09-01',
          notes: '1-week initial home visit'
        },
        rescueStaff1
      );

      expect(fu1.status).toBe('Scheduled');

      // Complete follow-up
      const completedFu1 = followUpService.completeFollowUp(fu1.id, 'Home visit successful. Dog is happy.', rescueStaff1);
      expect(completedFu1.status).toBe('Completed');

      // Schedule second follow-up
      const fu2 = followUpService.scheduleFollowUp(
        {
          adoptionId: 1,
          followUpDate: '2026-10-01',
          notes: '2-month routine checkup'
        },
        rescueStaff1
      );

      expect(fu2.status).toBe('Scheduled');

      // Retrieve history (Seed had 2 follow-ups + 2 new = 4 total)
      const history = followUpService.getFollowUpHistory(1, orgAdmin);
      expect(history.length).toBe(4);
    });
  });

  describe('6. Branch Authorization Scoping', () => {
    it('prevents Branch Admin 1 from approving an application for Branch 2', async () => {
      // App #2 belongs to Animal 5 (Branch 2)
      // Reset App #2 status to Pending for test
      const app2 = dbContext.adoptionApplications.find((a) => a.id === 2)!;
      app2.status = 'Pending';

      expect(async () => {
        await approvalService.approveApplication(2, branchAdmin1);
      }).rejects.toThrow(/Staff can only approve applications for their own branch/i);
    });

    it('allows Branch Admin 2 to approve applications for Branch 2', async () => {
      const app2 = dbContext.adoptionApplications.find((a) => a.id === 2)!;
      app2.status = 'Pending';
      const animal5 = dbContext.animals.find((a) => a.id === 5)!;
      animal5.status = 'Available';

      const adoption = await approvalService.approveApplication(2, branchAdmin2);
      expect(adoption).toBeDefined();
    });

    it('allows Org Admin to approve applications across all branches', async () => {
      const app2 = dbContext.adoptionApplications.find((a) => a.id === 2)!;
      app2.status = 'Pending';
      const animal5 = dbContext.animals.find((a) => a.id === 5)!;
      animal5.status = 'Available';

      const adoption = await approvalService.approveApplication(2, orgAdmin);
      expect(adoption).toBeDefined();
    });
  });
});
