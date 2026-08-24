import * as XLSX from 'xlsx';
import { DbState } from './dbContext';

/**
 * Utility to parse PawTrack_Sample_Dataset.xlsx dynamically
 * and map spreadsheet rows directly to PawTrack DbState entities.
 */
export function loadDatasetFromExcel(excelBuffer: ArrayBuffer | Uint8Array): Partial<DbState> {
  const workbook = XLSX.read(excelBuffer, { type: 'array' });
  const result: Partial<DbState> = {};

  if (workbook.SheetNames.includes('Branch')) {
    const sheet = workbook.Sheets['Branch'];
    const rows: any[] = XLSX.utils.sheet_to_json(sheet, { header: 1 });
    result.branches = rows.slice(1).filter(r => r[0]).map((r) => ({
      id: Number(r[0]),
      name: r[1] || `Branch #${r[0]}`,
      region: r[2] || 'Default Region',
      address: r[3] || 'Default Address',
      phone: r[4] || '+1-555-0000'
    }));
  }

  if (workbook.SheetNames.includes('User')) {
    const sheet = workbook.Sheets['User'];
    const rows: any[] = XLSX.utils.sheet_to_json(sheet, { header: 1 });
    result.users = rows.slice(1).filter(r => r[0]).map((r) => ({
      id: Number(r[0]),
      name: r[1] || `User #${r[0]}`,
      email: r[2] || `user${r[0]}@pawtrack.org`,
      role: (r[3] || (Number(r[0]) >= 8 ? 'Adopter' : 'RescueStaff')) as any,
      branchId: r[4] ? Number(r[4]) : null
    }));
  }

  if (workbook.SheetNames.includes('Animal')) {
    const sheet = workbook.Sheets['Animal'];
    const rows: any[] = XLSX.utils.sheet_to_json(sheet, { header: 1 });
    result.animals = rows.slice(1).filter(r => r[0]).map((r) => ({
      id: Number(r[0]),
      name: r[1] || `Animal #${r[0]}`,
      species: r[2] || 'Dog',
      breed: r[3] || 'Mix',
      age: Number(r[4] || 2),
      gender: (r[5] || 'Male') as any,
      rescueDate: r[6] || '2026-06-01',
      rescueLocation: r[7] || 'City Rescue',
      status: (r[8] || (Number(r[0]) === 1 || Number(r[0]) === 5 ? 'Adopted' : 'Available')) as any,
      branchId: Number(r[9] || (Number(r[0]) <= 3 ? 1 : 2)),
      categoryId: Number(r[10] || 1),
      microchipNumber: r[11] ? String(r[11]) : undefined,
      description: r[12] || 'Rescued animal looking for a loving home.'
    }));
  }

  return result;
}
