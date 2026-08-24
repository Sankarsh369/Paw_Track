import { dbContext } from '../data/dbContext';
import { Branch, User } from '../data/schema';

export class BranchService {
  public getBranches(): Branch[] {
    return dbContext.branches;
  }

  public getUsers(): User[] {
    return dbContext.users;
  }
}

export const branchService = new BranchService();
