import { adoptionService, SubmitApplicationDto } from '../services/adoptionService';
import { approvalService } from '../services/approvalService';
import { User } from '../data/schema';

export class AdoptionController {
  /**
   * Submit an adoption application (Adopter)
   */
  public submitApplication(dto: SubmitApplicationDto, currentUser: User) {
    try {
      const application = adoptionService.submitApplication(dto, currentUser);
      return { success: true, message: 'Adoption application submitted successfully.', data: application };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  }

  /**
   * Get applications list (Filtered by branch and status)
   */
  public getApplications(currentUser: User, statusFilter?: string, branchFilter?: number) {
    try {
      const applications = adoptionService.getApplications(currentUser, statusFilter, branchFilter);
      return { success: true, data: applications };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  }

  /**
   * Get application by ID
   */
  public getApplicationById(id: number, currentUser: User) {
    try {
      const application = adoptionService.getApplicationById(id, currentUser);
      return { success: true, data: application };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  }

  /**
   * Approve an application (Branch Admin / Rescue Staff / Org Admin)
   */
  public async approveApplication(applicationId: number, currentUser: User) {
    try {
      const adoption = await approvalService.approveApplication(applicationId, currentUser);
      return { success: true, message: 'Application approved and adoption record created.', data: adoption };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  }

  /**
   * Reject an application (Branch Admin / Rescue Staff / Org Admin)
   */
  public async rejectApplication(applicationId: number, rejectionReason: string, currentUser: User) {
    try {
      await approvalService.rejectApplication(applicationId, rejectionReason, currentUser);
      return { success: true, message: 'Application rejected.' };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  }
}

export const adoptionController = new AdoptionController();
