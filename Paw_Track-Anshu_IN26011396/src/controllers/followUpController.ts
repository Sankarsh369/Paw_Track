import { followUpService, ScheduleFollowUpDto } from '../services/followUpService';
import { User } from '../data/schema';

export class FollowUpController {
  /**
   * Schedule a post-adoption follow-up check-in
   */
  public createFollowUp(dto: ScheduleFollowUpDto, currentUser: User) {
    try {
      const followUp = followUpService.scheduleFollowUp(dto, currentUser);
      return { success: true, message: 'Follow-up check-in scheduled successfully.', data: followUp };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  }

  /**
   * Complete a scheduled follow-up
   */
  public completeFollowUp(followUpId: number, notesUpdate: string | undefined, currentUser: User) {
    try {
      const followUp = followUpService.completeFollowUp(followUpId, notesUpdate, currentUser);
      return { success: true, message: 'Follow-up marked as completed.', data: followUp };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  }

  /**
   * Get follow-up history for an adoption
   */
  public getFollowUps(adoptionId: number, currentUser: User) {
    try {
      const history = followUpService.getFollowUpHistory(adoptionId, currentUser);
      return { success: true, data: history };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  }

  /**
   * Get all adoptions with detailed follow-up history
   */
  public getAdoptions(currentUser: User) {
    try {
      const adoptions = followUpService.getAdoptions(currentUser);
      return { success: true, data: adoptions };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  }
}

export const followUpController = new FollowUpController();
