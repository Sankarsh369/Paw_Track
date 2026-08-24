import { User, UserRole } from '../data/schema';
import { dbContext } from '../data/dbContext';

export interface AuthContext {
  currentUser: User;
}

export function getCurrentUser(userId: number): User {
  const user = dbContext.users.find((u) => u.id === userId);
  if (!user) {
    throw new Error(`User with ID ${userId} not found.`);
  }
  return user;
}

export function authorizeRole(user: User, allowedRoles: UserRole[]): void {
  if (!allowedRoles.includes(user.role)) {
    throw new Error(`Access denied. User role '${user.role}' is not authorized.`);
  }
}

export function enforceBranchScope(user: User, entityBranchId: number): void {
  if (user.role === 'OrgAdmin') {
    return; // OrgAdmin has cross-branch access
  }
  if (user.branchId !== entityBranchId) {
    throw new Error(`Branch authorization violation. Access restricted to branch #${user.branchId}.`);
  }
}
