import {
  Branch,
  User,
  Category,
  Animal,
  VisitSlot,
  VisitBooking,
  AdoptionApplication,
  Adoption,
  FollowUp,
  Payment
} from './schema';

export interface DbState {
  branches: Branch[];
  users: User[];
  categories: Category[];
  animals: Animal[];
  visitSlots: VisitSlot[];
  visitBookings: VisitBooking[];
  adoptionApplications: AdoptionApplication[];
  adoptions: Adoption[];
  followUps: FollowUp[];
  payments: Payment[];
}

const initialSeedData: DbState = {
  branches: [
    { id: 1, name: 'Downtown Shelter', region: 'Metro Region', address: '124 Shelter Way, City Center', phone: '+1-555-0192' },
    { id: 2, name: 'Westside Rescue', region: 'West District', address: '890 Haven Blvd, Westside', phone: '+1-555-0198' }
  ],
  users: [
    { id: 1, name: 'Sarah Admin (Org)', email: 'sarah.admin@pawtrack.org', role: 'OrgAdmin', branchId: null },
    { id: 2, name: 'John Manager (Branch 1)', email: 'john.b1@pawtrack.org', role: 'BranchAdmin', branchId: 1 },
    { id: 3, name: 'Alice Staff (Branch 1)', email: 'alice.s1@pawtrack.org', role: 'RescueStaff', branchId: 1 },
    { id: 4, name: 'Dr. Bob Vet (Branch 1)', email: 'bob.vet1@pawtrack.org', role: 'Veterinarian', branchId: 1 },
    { id: 5, name: 'Elena Manager (Branch 2)', email: 'elena.b2@pawtrack.org', role: 'BranchAdmin', branchId: 2 },
    { id: 6, name: 'David Staff (Branch 2)', email: 'david.s2@pawtrack.org', role: 'RescueStaff', branchId: 2 },
    { id: 7, name: 'Dr. Carol Vet (Branch 2)', email: 'carol.vet2@pawtrack.org', role: 'Veterinarian', branchId: 2 },
    { id: 8, name: 'Mark Adopter', email: 'mark.adopter@gmail.com', role: 'Adopter', branchId: null },
    { id: 9, name: 'Emily Adopter', email: 'emily.adopter@yahoo.com', role: 'Adopter', branchId: null },
    { id: 10, name: 'Michael Adopter', email: 'michael.adopter@outlook.com', role: 'Adopter', branchId: null }
  ],
  categories: [
    { id: 1, name: 'Dog' },
    { id: 2, name: 'Cat' },
    { id: 3, name: 'Rabbit' },
    { id: 4, name: 'Bird' }
  ],
  animals: [
    {
      id: 1,
      name: 'Bruno',
      species: 'Dog',
      breed: 'Golden Retriever',
      age: 3,
      gender: 'Male',
      rescueDate: '2026-06-10',
      rescueLocation: 'Central Park',
      status: 'Adopted',
      branchId: 1,
      categoryId: 1,
      microchipNumber: '985141002341234',
      description: 'Friendly and active Golden Retriever who loves fetching balls.',
      imageUrl: 'https://images.unsplash.com/photo-1552053831-71594a27632d?w=600&auto=format&fit=crop&q=80'
    },
    {
      id: 2,
      name: 'Whiskers',
      species: 'Cat',
      breed: 'Persian',
      age: 2,
      gender: 'Female',
      rescueDate: '2026-07-01',
      rescueLocation: 'Downtown Alley',
      status: 'Available',
      branchId: 1,
      categoryId: 2,
      microchipNumber: '985141002341235',
      description: 'Calm, fluffy feline who enjoys sunny spots and quiet cuddles.',
      imageUrl: 'https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=600&auto=format&fit=crop&q=80'
    },
    {
      id: 3,
      name: 'Rocky',
      species: 'Dog',
      breed: 'Beagle',
      age: 1,
      gender: 'Male',
      rescueDate: '2026-07-15',
      rescueLocation: 'North Suburbs',
      status: 'Available',
      branchId: 1,
      categoryId: 1,
      microchipNumber: '985141002341236',
      description: 'Playful pup with a great curious nose for outdoor adventures.',
      imageUrl: 'https://images.unsplash.com/photo-1537151608828-ea2b11777ee8?w=600&auto=format&fit=crop&q=80'
    },
    {
      id: 4,
      name: 'Tweety',
      species: 'Bird',
      breed: 'Canary',
      age: 1,
      gender: 'Female',
      rescueDate: '2026-07-20',
      rescueLocation: 'Westside Market',
      status: 'Available',
      branchId: 2,
      categoryId: 4,
      description: 'Cheerful yellow singing canary looking for a warm home.',
      imageUrl: 'https://images.unsplash.com/photo-1522858547137-f1dcec554f55?w=600&auto=format&fit=crop&q=80'
    },
    {
      id: 5,
      name: 'Simba',
      species: 'Cat',
      breed: 'Siamese',
      age: 4,
      gender: 'Male',
      rescueDate: '2026-06-25',
      rescueLocation: 'Sunset Avenue',
      status: 'Adopted',
      branchId: 2,
      categoryId: 2,
      microchipNumber: '985141002341238',
      description: 'Vocal and loving Siamese who forms deep bonds with owners.',
      imageUrl: 'https://images.unsplash.com/photo-1513360371669-4adf3dd7dff8?w=600&auto=format&fit=crop&q=80'
    },
    {
      id: 6,
      name: 'Coco',
      species: 'Dog',
      breed: 'Poodle',
      age: 2,
      gender: 'Female',
      rescueDate: '2026-08-01',
      rescueLocation: 'Eastside Square',
      status: 'Available',
      branchId: 2,
      categoryId: 1,
      microchipNumber: '985141002341239',
      description: 'Intelligent, hypoallergenic companion ready for trick training.',
      imageUrl: 'https://images.unsplash.com/photo-1591769225440-811ad7d6eab2?w=600&auto=format&fit=crop&q=80'
    },
    {
      id: 7,
      name: 'Fluffy',
      species: 'Rabbit',
      breed: 'Angora',
      age: 1,
      gender: 'Male',
      rescueDate: '2026-08-05',
      rescueLocation: 'Greenwood Farm',
      status: 'Available',
      branchId: 1,
      categoryId: 3,
      description: 'Gentle bunny who enjoys fresh leafy greens and lap pets.',
      imageUrl: 'https://images.unsplash.com/photo-1585110396000-c9ffd4e4b308?w=600&auto=format&fit=crop&q=80'
    }
  ],
  visitSlots: [
    { id: 1, branchId: 1, date: '2026-08-25', startTime: '10:00', endTime: '11:00', capacity: 3, bookedCount: 2 },
    { id: 2, branchId: 1, date: '2026-08-25', startTime: '14:00', endTime: '15:00', capacity: 3, bookedCount: 1 },
    { id: 3, branchId: 1, date: '2026-08-26', startTime: '11:00', endTime: '12:00', capacity: 3, bookedCount: 0 },
    { id: 4, branchId: 2, date: '2026-08-25', startTime: '10:00', endTime: '11:00', capacity: 2, bookedCount: 2 },
    { id: 5, branchId: 2, date: '2026-08-26', startTime: '15:00', endTime: '16:00', capacity: 2, bookedCount: 0 }
  ],
  visitBookings: [
    { id: 1, visitSlotId: 1, animalId: 1, adopterId: 8, bookingDate: '2026-08-18', status: 'Completed', staffNotes: 'Adopter met Bruno; very affectionate interaction.' },
    { id: 2, visitSlotId: 1, animalId: 3, adopterId: 9, bookingDate: '2026-08-19', status: 'Completed', staffNotes: 'Emily loved Rocky, considering application.' },
    { id: 3, visitSlotId: 2, animalId: 2, adopterId: 10, bookingDate: '2026-08-21', status: 'Completed', staffNotes: 'Michael visited Whiskers.' },
    { id: 4, visitSlotId: 4, animalId: 5, adopterId: 9, bookingDate: '2026-08-20', status: 'Completed', staffNotes: 'Visit with Simba was wonderful.' },
    { id: 5, visitSlotId: 4, animalId: 6, adopterId: 10, bookingDate: '2026-08-22', status: 'Booked', staffNotes: 'Scheduled visit for Coco.' }
  ],
  adoptionApplications: [
    { id: 1, animalId: 1, adopterId: 8, visitBookingId: 1, applicationDate: '2026-08-20', status: 'Approved', notes: 'Experienced dog owner with fenced yard.' },
    { id: 2, animalId: 5, adopterId: 9, visitBookingId: 4, applicationDate: '2026-08-21', status: 'Approved', notes: 'Apartment owner with cat-proofing.' },
    { id: 3, animalId: 2, adopterId: 10, visitBookingId: 3, applicationDate: '2026-08-22', status: 'Pending', notes: 'First-time cat adopter, home visit completed.' }
  ],
  adoptions: [
    { id: 1, applicationId: 1, animalId: 1, adopterId: 8, approvedById: 2, adoptionDate: '2026-08-21', notes: 'Final adoption paperwork signed.' },
    { id: 2, applicationId: 2, animalId: 5, adopterId: 9, approvedById: 5, adoptionDate: '2026-08-22', notes: 'Adoption completed smoothly.' }
  ],
  followUps: [
    { id: 1, adoptionId: 1, conductedById: 3, followUpDate: '2026-08-28', notes: 'Bruno is settling in well. Eating properly.', status: 'Completed' },
    { id: 2, adoptionId: 2, conductedById: 6, followUpDate: '2026-08-29', notes: 'Simba health check ok. Very friendly with new owner.', status: 'Completed' },
    { id: 3, adoptionId: 1, conductedById: 3, followUpDate: '2026-09-15', notes: 'Routine 1-month post-adoption checkup scheduled.', status: 'Scheduled' }
  ],
  payments: [
    { id: 1, adoptionId: 1, animalId: 1, donorUserId: 8, amount: 1500, type: 'AdoptionFee', status: 'Completed', paymentDate: '2026-08-21' },
    { id: 2, adoptionId: 2, animalId: 5, donorUserId: 9, amount: 1800, type: 'AdoptionFee', status: 'Completed', paymentDate: '2026-08-22' },
    { id: 3, animalId: 3, donorUserId: 10, amount: 500, type: 'Donation', status: 'Completed', paymentDate: '2026-08-22' }
  ]
};

export class DbContext {
  private static instance: DbContext;
  private state: DbState;
  private transactionSnapshot: DbState | null = null;
  private isTransactionActive = false;
  private lockPromise: Promise<void> = Promise.resolve();

  private constructor() {
    this.state = JSON.parse(JSON.stringify(initialSeedData));
  }

  public static getInstance(): DbContext {
    if (!DbContext.instance) {
      DbContext.instance = new DbContext();
    }
    return DbContext.instance;
  }

  // Reset to original seed state (useful for tests)
  public resetToSeed(): void {
    this.state = JSON.parse(JSON.stringify(initialSeedData));
    this.transactionSnapshot = null;
    this.isTransactionActive = false;
  }

  // Synchronize execution lock for concurrency protection
  public async acquireLock<T>(action: () => Promise<T>): Promise<T> {
    let release: () => void = () => {};
    const nextLock = new Promise<void>((resolve) => {
      release = resolve;
    });

    const currentLock = this.lockPromise;
    this.lockPromise = nextLock;

    await currentLock;
    try {
      return await action();
    } finally {
      release();
    }
  }

  // Transaction support
  public beginTransaction(): void {
    if (this.isTransactionActive) {
      throw new Error('A transaction is already in progress.');
    }
    this.transactionSnapshot = JSON.parse(JSON.stringify(this.state));
    this.isTransactionActive = true;
  }

  public commitTransaction(): void {
    if (!this.isTransactionActive) {
      throw new Error('No active transaction to commit.');
    }
    this.transactionSnapshot = null;
    this.isTransactionActive = false;
  }

  public rollbackTransaction(): void {
    if (!this.isTransactionActive || !this.transactionSnapshot) {
      throw new Error('No active transaction to rollback.');
    }
    this.state = JSON.parse(JSON.stringify(this.transactionSnapshot));
    this.transactionSnapshot = null;
    this.isTransactionActive = false;
  }

  // Collection Accessors
  public get branches(): Branch[] { return this.state.branches; }
  public get users(): User[] { return this.state.users; }
  public get categories(): Category[] { return this.state.categories; }
  public get animals(): Animal[] { return this.state.animals; }
  public get visitSlots(): VisitSlot[] { return this.state.visitSlots; }
  public get visitBookings(): VisitBooking[] { return this.state.visitBookings; }
  public get adoptionApplications(): AdoptionApplication[] { return this.state.adoptionApplications; }
  public get adoptions(): Adoption[] { return this.state.adoptions; }
  public get followUps(): FollowUp[] { return this.state.followUps; }
  public get payments(): Payment[] { return this.state.payments; }

  // Auto-increment ID helpers
  public getNextId(collection: keyof DbState): number {
    const list = this.state[collection] as any[];
    if (list.length === 0) return 1;
    return Math.max(...list.map(item => item.id)) + 1;
  }
}

export const dbContext = DbContext.getInstance();
