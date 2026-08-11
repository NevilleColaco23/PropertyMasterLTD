import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, Subscription, catchError, interval, of, startWith, switchMap, tap } from 'rxjs';
import { Attachment, Post, PostType } from './post.model';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PostsService {
  private apiUrl = `${environment.apiUrl}/posts`;

  private postsSubject = new BehaviorSubject<Post[]>([]);
  posts$: Observable<Post[]> = this.postsSubject.asObservable();

  private unseenCountSubject = new BehaviorSubject<number>(0);
  unseenCount$: Observable<number> = this.unseenCountSubject.asObservable();

  private unseenPostIdsSubject = new BehaviorSubject<Set<number>>(new Set<number>());
  unseenPostIds$: Observable<Set<number>> = this.unseenPostIdsSubject.asObservable();

  private seenPostIds = new Set<number>();
  private hasSeededSeenIds = false;

  private pollingSub: Subscription | null = null;
  private lastLimit: number | undefined = undefined;

  constructor(private http: HttpClient) {
    this.refresh();
  }

  refresh(limit?: number): void {
    this.lastLimit = limit;
    const url = limit ? `${this.apiUrl}?limit=${limit}` : this.apiUrl;
    this.http.get<Post[]>(url)
      .pipe(
        catchError(() => of([] as Post[]))
      )
      .subscribe(posts => this.applyPosts(posts));
  }

  /**
   * Starts polling the feed at a fixed interval so new posts can be detected
   * even while the user is on a different tab. Safe to call multiple times;
   * only one polling subscription is kept active.
   */
  startPolling(limit?: number, intervalMs = 15000): void {
    this.lastLimit = limit;
    this.stopPolling();

    this.pollingSub = interval(intervalMs)
      .pipe(
        startWith(0),
        switchMap(() => {
          const url = this.lastLimit ? `${this.apiUrl}?limit=${this.lastLimit}` : this.apiUrl;
          return this.http.get<Post[]>(url).pipe(catchError(() => of([] as Post[])));
        })
      )
      .subscribe(posts => this.applyPosts(posts));
  }

  stopPolling(): void {
    this.pollingSub?.unsubscribe();
    this.pollingSub = null;
  }

  /**
   * Marks all currently loaded posts as seen and resets the unseen counter/highlighting.
   * Call this when the user opens/focuses the Team Feed tab (typically after a short delay
   * so the "New" highlight is visible for a moment, similar to LinkedIn).
   */
  markAllSeen(): void {
    this.postsSubject.value.forEach(p => this.seenPostIds.add(p.id));
    this.hasSeededSeenIds = true;
    this.unseenCountSubject.next(0);
    this.unseenPostIdsSubject.next(new Set<number>());
  }

  private applyPosts(posts: Post[]): void {
    if (!this.hasSeededSeenIds) {
      // First load: treat everything already present as seen so we only count genuinely new arrivals.
      posts.forEach(p => this.seenPostIds.add(p.id));
      this.hasSeededSeenIds = true;
    } else {
      const unseenIds = new Set<number>(posts.filter(p => !this.seenPostIds.has(p.id)).map(p => p.id));
      this.unseenCountSubject.next(unseenIds.size);
      this.unseenPostIdsSubject.next(unseenIds);
    }

    this.postsSubject.next(posts);
  }

  addPost(authorName: string, text: string, postType: PostType = PostType.General, targetGroupId?: number, attachments: Attachment[] = []): void {
    const trimmed = text.trim();
    if (!trimmed) {
      return;
    }

    this.http.post<number>(this.apiUrl, { authorName, text: trimmed, postType, targetGroupId, attachments })
      .pipe(
        tap(newId => {
          if (typeof newId === 'number') {
            this.seenPostIds.add(newId);
          }
          this.refresh(this.lastLimit);
        }),
        catchError(() => of(null))
      )
      .subscribe();
  }

  addComment(postId: number, authorName: string, text: string, attachments: Attachment[] = []): void {
    const trimmed = text.trim();
    if (!trimmed) {
      return;
    }

    this.http.post<number>(`${this.apiUrl}/${postId}/comments`, { postId, authorName, text: trimmed, attachments })
      .pipe(
        tap(() => this.refresh(this.lastLimit)),
        catchError(() => of(null))
      )
      .subscribe();
  }

  acknowledgePost(postId: number, userName: string): void {
    this.http.post<void>(`${this.apiUrl}/${postId}/acknowledge`, { postId, userName })
      .pipe(
        tap(() => this.refresh()),
        catchError(() => of(null))
      )
      .subscribe();
  }

  acknowledgeComment(postId: number, commentId: number, userName: string): void {
    this.http.post<void>(`${this.apiUrl}/${postId}/comments/${commentId}/acknowledge`, { postId, commentId, userName })
      .pipe(
        tap(() => this.refresh()),
        catchError(() => of(null))
      )
      .subscribe();
  }

  deletePost(id: number): void {
    const previous = this.postsSubject.value;

    // Optimistically remove the post from the UI while the request is in flight.
    this.postsSubject.next(previous.filter(p => p.id !== id));

    this.http.delete<void>(`${this.apiUrl}/${id}`)
      .pipe(
        catchError(() => {
          // Restore state if the delete failed server-side.
          this.postsSubject.next(previous);
          return of(null);
        })
      )
      .subscribe();
  }
}
