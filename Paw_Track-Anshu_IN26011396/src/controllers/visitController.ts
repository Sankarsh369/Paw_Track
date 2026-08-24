import { visitService, BookVisitDto } from '../services/visitService';
import { User } from '../data/schema';

export class VisitController {
  public bookVisit(dto: BookVisitDto, currentUser: User) {
    try {
      const booking = visitService.bookVisit(dto, currentUser);
      return { success: true, message: 'Visit booked successfully.', data: booking };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  }

  public getVisitBookings(currentUser: User) {
    try {
      const bookings = visitService.getVisitBookings(currentUser);
      return { success: true, data: bookings };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  }

  public getVisitSlots(branchId: number) {
    try {
      const slots = visitService.getVisitSlots(branchId);
      return { success: true, data: slots };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  }

  public updateVisitStatus(bookingId: number, status: 'CheckedIn' | 'Completed' | 'Cancelled' | 'NoShow', currentUser: User) {
    try {
      const booking = visitService.updateVisitStatus(bookingId, status, currentUser);
      return { success: true, message: `Visit status updated to ${status}.`, data: booking };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  }
}

export const visitController = new VisitController();
