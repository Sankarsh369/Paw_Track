import { dbContext } from '../data/dbContext';
import { Animal, User } from '../data/schema';

export class AnimalService {
  public getAnimals(currentUser?: User, branchFilter?: number, statusFilter?: string): Animal[] {
    let animals = [...dbContext.animals];

    if (currentUser && currentUser.role !== 'OrgAdmin' && currentUser.role !== 'Adopter' && currentUser.branchId !== null) {
      animals = animals.filter((a) => a.branchId === currentUser.branchId);
    } else if (branchFilter) {
      animals = animals.filter((a) => a.branchId === branchFilter);
    }

    if (statusFilter && statusFilter !== 'All') {
      animals = animals.filter((a) => a.status === statusFilter);
    }

    return animals;
  }

  public getAnimalById(id: number): Animal {
    const animal = dbContext.animals.find((a) => a.id === id);
    if (!animal) {
      throw new Error(`Animal with ID ${id} was not found.`);
    }
    return animal;
  }
}

export const animalService = new AnimalService();
