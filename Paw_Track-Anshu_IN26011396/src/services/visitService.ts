import { dbContext } from '../data/dbContext';
import { VisitBooking, VisitSlot, User } from '../data/schema';

export interface BookVisitDto {
  visitSlotId: number;
  animalId: number;
}

export class VisitService {
  /**
   * Book a visit slot for an animal.
   * CRITICAL PAWTRACK RULE: Booking a visit DOES NOT change Animal.Status!
   * The animal remains Available for other adopters.
   */
  public bookVisit(dto: BookVisitDto, currentUser: User): VisitBooking {
    const slot = dbContext.visitSlots.find((s) => s.id === dto.visitSlotId);
    if (!slot) {
      throw new Error(`Visit slot with ID ${dto.visitSlotId} was not found.`);
    }

    if (slot.bookedCount >= slot.capacity) {
      throw new Error('This visit slot has reached its maximum capacity.');
    }

    const animal = dbContext.animals.find((a) => a.id === dto.animalId);
    if (!animal) {
      throw new Error(`Animal with ID ${dto.animalId} was not found.`);
    }

    if (animal.branchId !== slot.branchId) {
      throw new Error('Selected visit slot belongs to a different branch than the animal.');
    }

    // Create booking
    const booking: VisitBooking = {
      id: dbContext.getNextId('visitBookings'),
      visitSlotId: dto.visitSlotId,
      animalId: dto.animalId,
      adopterId: currentUser.id,
      bookingDate: new Date().toISOString().split('T')[0],
      status: 'Booked'
    };

    // Increment slot booked count
    slot.bookedCount += 1;

    dbContext.visitBookings.push(booking);
    return booking;
  }

  /**
   * Update visit booking status (CheckedIn, Completed, Cancelled, NoShow)
   */
  public updateVisitStatus(bookingId: number, status: 'CheckedIn' | 'Completed' | 'Cancelled' | 'NoShow', currentUser: User): VisitBooking {
    const booking = dbContext.visitBookings.find((b) => b.id === bookingId);
    if (!booking) {
      throw new Error(`Visit booking with ID ${bookingId} was not found.`);
    }

    const animal = dbContext.animals.find((a) => a.id === booking.animalId);
    if (currentUser.role !== 'OrgAdmin' && animal && currentUser.branchId !== animal.branchId) {
      throw new Error('Access denied: Staff can only manage visits for their own branch.');
    }

    booking.status = status;
    return booking;
  }

  /**
   * Get available visit slots for a branch
   */
  public getVisitSlots(branchId: number): VisitSlot[] {
    return dbContext.visitSlots.filter((s) => s.branchId === branchId);
  }

  /**
   * Get visit bookings for currentUser or branch
   */
  public getVisitBookings(currentUser: User): any[] {
    let bookings = [...dbContext.visitBookings];

    if (currentUser.role === 'Adopter') {
      bookings = bookings.filter((b) => b.adopterId === currentUser.id);
    } else if (currentUser.role !== 'OrgAdmin' && currentUser.branchId !== null) {
      const branchAnimalIds = new Set(
        dbContext.animals.filter((a) => a.branchId === currentUser.branchId).map((a) => a.id)
      );
      bookings = bookings.filter((b) => branchAnimalIds.has(b.animalId));
    }

    return bookings.map((b) => {
      const animal = dbContext.animals.find((a) => a.id === b.animalId);
      const adopter = dbContext.users.find((u) => u.id === b.adopterId);
      const slot = dbContext.visitSlots.find((s) => s.id === b.visitSlotId);

      return {
        ...b,
        animalName: animal?.name || 'Unknown',
        animalSpecies: animal?.species || '',
        adopterName: adopter?.name || 'Unknown',
        slotDate: slot?.date,
        slotTime: slot ? `${slot.startTime} - ${slot.endTime}` : ''
      };
    });
  }
}

export const visitService = new VisitService();
