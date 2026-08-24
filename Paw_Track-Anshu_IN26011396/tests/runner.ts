import assert from 'node:assert';
import { dbContext } from '../src/data/dbContext.js';
import { adoptionService } from '../src/services/adoptionService.js';
import { approvalService } from '../src/services/approvalService.js';
import { followUpService } from '../src/services/followUpService.js';
import { visitService } from '../src/services/visitService.js';

console.log('====================================================');
console.log('   PAWTRACK — AUTOMATED INTEGRATION TEST RUNNER');
console.log('====================================================\n');

let passedCount = 0;
let totalCount = 0;

async function runTest(name: string, fn: () => void | Promise<void>) {
  totalCount++;
  try {
    await fn();
    console.log(`[PASS] ${name}`);
    passedCount++;
  } catch (err: any) {
    console.error(`[FAIL] ${name}:`, err.message);
  }
}

async function runAll() {
  const orgAdmin = dbContext.users.find((u) => u.id === 1)!;
  const branchAdmin1 = dbContext.users.find((u) => u.id === 2)!;
  const rescueStaff1 = dbContext.users.find((u) => u.id === 3)!;
  const branchAdmin2 = dbContext.users.find((u) => u.id === 5)!;
  const adopter8 = dbContext.users.find((u) => u.id === 8)!;
  const adopter9 = dbContext.users.find((u) => u.id === 9)!;
  const adopter10 = dbContext.users.find((u) => u.id === 10)!;

  // Reset before test 1
  dbContext.resetToSeed();

  await runTest('1. Application Submission for Available animal succeeds', () => {
    const app = adoptionService.submitApplication({ animalId: 2 }, adopter8);
    assert.strictEqual(app.animalId, 2);
    assert.strictEqual(app.adopterId, 8);
    assert.strictEqual(app.status, 'Pending');
  });

  await runTest('2. Application Submission for non-Available animal fails', () => {
    assert.throws(() => {
      adoptionService.submitApplication({ animalId: 1 }, adopter8);
    }, /Animal is no longer available/i);
  });

  await runTest('3. Duplicate active application prevention', () => {
    assert.throws(() => {
      adoptionService.submitApplication({ animalId: 2 }, adopter10);
    }, /An active application already exists/i);
  });

  await runTest('4. VisitBooking ownership validation', () => {
    assert.throws(() => {
      adoptionService.submitApplication({ animalId: 3, visitBookingId: 2 }, adopter8);
    }, /The selected visit does not belong to this adopter/i);
  });

  await runTest('5. No-Hold Rule: Booking visit leaves Animal.Status as Available', () => {
    const animal3 = dbContext.animals.find((a) => a.id === 3)!;
    assert.strictEqual(animal3.status, 'Available');
    visitService.bookVisit({ visitSlotId: 3, animalId: 3 }, adopter8);
    assert.strictEqual(animal3.status, 'Available');
  });

  await runTest('6. Approval Workflow & Animal Status Transition', async () => {
    const animal2 = dbContext.animals.find((a) => a.id === 2)!;
    assert.strictEqual(animal2.status, 'Available');
    const adoption = await approvalService.approveApplication(3, branchAdmin1);
    assert.strictEqual(adoption.animalId, 2);
    assert.strictEqual(adoption.adopterId, 10);
    assert.strictEqual(animal2.status, 'Adopted');
  });

  await runTest('7. Rejection Reason Requirement', async () => {
    dbContext.resetToSeed();
    await assert.rejects(async () => {
      await approvalService.rejectApplication(3, '', branchAdmin1);
    }, /Rejection reason is required/i);
  });

  await runTest('8. Rejection Persistence', async () => {
    dbContext.resetToSeed();
    await approvalService.rejectApplication(3, 'Housing check failed.', branchAdmin1);
    const app3 = dbContext.adoptionApplications.find((a) => a.id === 3)!;
    assert.strictEqual(app3.status, 'Rejected');
    assert.strictEqual(app3.rejectionReason, 'Housing check failed.');
  });

  await runTest('9. State Machine: Re-approval prohibited', async () => {
    dbContext.resetToSeed();
    await assert.rejects(async () => {
      await approvalService.approveApplication(1, branchAdmin1);
    }, /Application is no longer pending/i);
  });

  await runTest('10. Concurrency: Multiple applications & single approval win', async () => {
    dbContext.resetToSeed();
    const appA = adoptionService.submitApplication({ animalId: 3 }, adopter8);
    const appB = adoptionService.submitApplication({ animalId: 3 }, adopter9);

    const adoption = await approvalService.approveApplication(appA.id, branchAdmin1);
    assert.strictEqual(adoption.adopterId, 8);

    const animal3 = dbContext.animals.find((a) => a.id === 3)!;
    assert.strictEqual(animal3.status, 'Adopted');

    await assert.rejects(async () => {
      await approvalService.approveApplication(appB.id, branchAdmin1);
    }, /animal is no longer available/i);
  });

  await runTest('11. Follow-Up System: 1 to N History & Schedule/Complete', () => {
    dbContext.resetToSeed();
    const fu1 = followUpService.scheduleFollowUp(
      { adoptionId: 1, followUpDate: '2026-09-01', notes: 'Initial check' },
      rescueStaff1
    );
    assert.strictEqual(fu1.status, 'Scheduled');

    const completed = followUpService.completeFollowUp(fu1.id, 'Check completed', rescueStaff1);
    assert.strictEqual(completed.status, 'Completed');

    const fu2 = followUpService.scheduleFollowUp(
      { adoptionId: 1, followUpDate: '2026-10-01', notes: '2nd check' },
      rescueStaff1
    );
    assert.strictEqual(fu2.status, 'Scheduled');

    const history = followUpService.getFollowUpHistory(1, orgAdmin);
    assert.strictEqual(history.length, 4); // 2 seed + 2 new
  });

  await runTest('12. Branch Authorization Security', async () => {
    dbContext.resetToSeed();
    const app2 = dbContext.adoptionApplications.find((a) => a.id === 2)!;
    app2.status = 'Pending';
    await assert.rejects(async () => {
      await approvalService.approveApplication(2, branchAdmin1);
    }, /Staff can only approve applications for their own branch/i);
  });

  console.log('\n====================================================');
  console.log(` RESULTS: ${passedCount} / ${totalCount} TESTS PASSED`);
  console.log('====================================================\n');
}

runAll();
