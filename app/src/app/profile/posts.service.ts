import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Post } from './post.model';

const STORAGE_KEY = 'pm_profile_posts';

@Injectable({
  providedIn: 'root'
})
export class PostsService {
  private postsSubject = new BehaviorSubject<Post[]>(this.loadPosts());
  posts$: Observable<Post[]> = this.postsSubject.asObservable();

  private loadPosts(): Post[] {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) as Post[] : [];
    } catch {
      return [];
    }
  }

  private savePosts(posts: Post[]): void {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(posts));
    } catch {
      // ignore storage errors (e.g. quota exceeded, private mode)
    }
  }

  addPost(authorName: string, text: string): void {
    const trimmed = text.trim();
    if (!trimmed) {
      return;
    }

    const newPost: Post = {
      id: `${Date.now()}-${Math.random().toString(36).slice(2, 9)}`,
      authorName,
      text: trimmed,
      createdAt: new Date().toISOString()
    };

    const updated = [newPost, ...this.postsSubject.value];
    this.postsSubject.next(updated);
    this.savePosts(updated);
  }

  deletePost(id: string): void {
    const updated = this.postsSubject.value.filter(p => p.id !== id);
    this.postsSubject.next(updated);
    this.savePosts(updated);
  }
}
