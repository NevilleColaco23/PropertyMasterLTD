import { addDays as dateFnsAddDays, subDays as dateFnsSubDays } from 'date-fns';

export function addDays(date: Date, amount: number): Date {
  return dateFnsAddDays(date, amount);
}

export function subDays(date: Date, amount: number): Date {
  return dateFnsSubDays(date, amount);
}
