import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, of, tap } from 'rxjs';
import { Attachment, Post, PostType } from './post.model';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PostsService {
  private apiUrl = `${environment.apiUrl}/posts`;

  private postsSubject = new BehaviorSubject<Post[]>([]);
  posts$: Observable<Post[]> = this.postsSubject.asObservable();

  constructor(private http: HttpClient) {
    this.refresh();
  }

  refresh(): void {
    this.http.get<Post[]>(this.apiUrl)
      .pipe(
        catchError(() => of([] as Post[]))
      )
      .subscribe(posts => this.postsSubject.next(posts));
  }

  addPost(authorName: string, text: string, postType: PostType = PostType.General, targetGroupId?: number, attachments: Attachment[] = []): void {
    const trimmed = text.trim();
    if (!trimmed) {
      return;
    }

    this.http.post<number>(this.apiUrl, { authorName, text: trimmed, postType, targetGroupId, attachments })
      .pipe(
        tap(() => this.refresh()),
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
        tap(() => this.refresh()),
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
