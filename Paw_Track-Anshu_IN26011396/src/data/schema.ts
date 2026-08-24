export type UserRole = 'OrgAdmin' | 'BranchAdmin' | 'RescueStaff' | 'Veterinarian' | 'Adopter';

export type AnimalStatus = 'UnderAssessment' | 'Available' | 'Pending' | 'Adopted';

export type ApplicationStatus = 'Pending' | 'Approved' | 'Rejected';

export type VisitBookingStatus = 'Booked' | 'CheckedIn' | 'Completed' | 'Cancelled' | 'NoShow' | 'FollowUpRequired';

export type FollowUpStatus = 'Scheduled' | 'Completed';

export interface Branch {
  id: number;
  name: string;
  region: string;
  address: string;
  phone: string;
}

export interface User {
  id: number;
  name: string;
  email: string;
  role: UserRole;
  branchId: number | null; // null for OrgAdmin
}

export interface Category {
  id: number;
  name: string;
}

export interface Animal {
  id: number;
  name: string;
  species: string;
  breed: string;
  age: number;
  gender: 'Male' | 'Female';
  rescueDate: string;
  rescueLocation: string;
  status: AnimalStatus;
  branchId: number;
  categoryId: number;
  microchipNumber?: string;
  description?: string;
  imageUrl?: string;
}

export interface VisitSlot {
  id: number;
  branchId: number;
  date: string;
  startTime: string;
  endTime: string;
  capacity: number;
  bookedCount: number;
}

export interface VisitBooking {
  id: number;
  visitSlotId: number;
  animalId: number;
  adopterId: number;
  bookingDate: string;
  status: VisitBookingStatus;
  staffNotes?: string;
}

export interface AdoptionApplication {
  id: number;
  animalId: number;
  adopterId: number;
  visitBookingId?: number | null;
  applicationDate: string;
  status: ApplicationStatus;
  rejectionReason?: string | null;
  notes?: string;
}

export interface Adoption {
  id: number;
  applicationId: number;
  animalId: number;
  adopterId: number;
  approvedById: number;
  adoptionDate: string;
  notes?: string;
}

export interface FollowUp {
  id: number;
  adoptionId: number;
  conductedById: number;
  followUpDate: string;
  notes: string;
  status: FollowUpStatus;
}

export interface Payment {
  id: number;
  adoptionId?: number | null;
  animalId?: number | null;
  donorUserId?: number | null;
  amount: number;
  type: 'AdoptionFee' | 'Donation';
  status: 'Pending' | 'Completed' | 'Failed';
  paymentDate: string;
}
