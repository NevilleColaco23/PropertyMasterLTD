import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, map, of, tap } from 'rxjs';
import { Group } from './post.model';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GroupsService {
  private apiUrl = `${environment.apiUrl}/groups`;

  private groupsSubject = new BehaviorSubject<Group[]>([]);
  groups$: Observable<Group[]> = this.groupsSubject.asObservable();

  constructor(private http: HttpClient) {
    this.refresh();
  }

  refresh(): void {
    this.http.get<Group[]>(this.apiUrl)
      .pipe(
        catchError(() => of([] as Group[]))
      )
      .subscribe(groups => this.groupsSubject.next(groups));
  }

  addGroup(name: string, description: string = ''): Observable<number | null> {
    const trimmed = name.trim();
    if (!trimmed) {
      return of(null);
    }

    return this.http.post<number>(this.apiUrl, { name: trimmed, description })
      .pipe(
        tap(() => this.refresh()),
        catchError(() => of(null))
      );
  }

  updateGroup(id: number, name: string, description: string = ''): Observable<boolean> {
    const trimmed = name.trim();
    if (!trimmed) {
      return of(false);
    }

    return this.http.put(`${this.apiUrl}/${id}`, { id, name: trimmed, description })
      .pipe(
        tap(() => this.refresh()),
        map(() => true),
        catchError(() => of(false))
      );
  }

  deleteGroup(id: number): Observable<boolean> {
    return this.http.delete(`${this.apiUrl}/${id}`)
      .pipe(
        tap(() => this.refresh()),
        map(() => true),
        catchError(() => of(false))
      );
  }
}
