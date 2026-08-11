import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Observable } from 'rxjs';
import { AuthService } from '../../core/auth/services/auth.service';
import { PostsService } from '../../profile/posts.service';
import { FeedSettingsService } from '../../profile/feed-settings.service';
import { MentionsService, MentionableUser } from '../../profile/mentions.service';
import { Post } from '../../profile/post.model';

interface MentionState {
  active: boolean;
  query: string;
  startIndex: number;
  suggestions: MentionableUser[];
}

@Component({
  selector: 'app-team-feed-widget',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './team-feed-widget.component.html',
  styleUrls: ['./team-feed-widget.component.css']
})
export class TeamFeedWidgetComponent implements OnInit, OnDestroy {
  posts$!: Observable<Post[]>;
  composerText = '';
  currentUsername: string | null = null;

  maxPostsToShow = 50;
  editingLimit = false;
  limitDraft = 50;

  mentionableUsers: MentionableUser[] = [];

  composerMention: MentionState = { active: false, query: '', startIndex: -1, suggestions: [] };
  commentMention: MentionState = { active: false, query: '', startIndex: -1, suggestions: [] };

  expandedComments: { [postId: number]: boolean } = {};
  commentDrafts: { [postId: number]: string } = {};
  activeCommentPostId: number | null = null;

  unseenPostIds = new Set<number>();
  private markSeenTimeout: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private authService: AuthService,
    private postsService: PostsService,
    private feedSettingsService: FeedSettingsService,
    private mentionsService: MentionsService
  ) {
    this.authService.signInState.subscribe(userData => {
      this.currentUsername = userData?.username || null;
    });
  }

  ngOnInit(): void {
    this.posts$ = this.postsService.posts$;

    this.feedSettingsService.get().subscribe(settings => {
      this.maxPostsToShow = settings.maxPostsToShow;
      this.limitDraft = settings.maxPostsToShow;
      // Poll continuously so new posts are detected even while the user is on another dashboard tab.
      this.postsService.startPolling(this.maxPostsToShow);
    });

    this.mentionsService.getUsersForCurrentProperty().subscribe(users => {
      this.mentionableUsers = users;
    });

    this.postsService.unseenPostIds$.subscribe(ids => {
      this.unseenPostIds = ids;
    });

    // Let the "New" highlight be visible briefly (like LinkedIn) before marking posts as seen.
    if (this.markSeenTimeout) {
      clearTimeout(this.markSeenTimeout);
    }
    this.markSeenTimeout = setTimeout(() => this.postsService.markAllSeen(), 4000);
  }

  ngOnDestroy(): void {
    // Keep polling running in the background so the tab badge stays accurate while the user is elsewhere;
    // polling is stopped centrally by the dashboard/service lifecycle, not on widget destroy.
    if (this.markSeenTimeout) {
      clearTimeout(this.markSeenTimeout);
    }
  }

  isNewPost(postId: number): boolean {
    return this.unseenPostIds.has(postId);
  }

  submitPost(): void {
    if (!this.composerText.trim() || !this.currentUsername) {
      return;
    }

    this.postsService.addPost(this.currentUsername, this.composerText);
    this.composerText = '';
    this.composerMention.active = false;
  }

  acknowledgePost(postId: number): void {
    if (!this.currentUsername) {
      return;
    }
    this.postsService.acknowledgePost(postId, this.currentUsername);
  }

  hasAcknowledged(acknowledgements: { userName: string }[]): boolean {
    return acknowledgements?.some(a => a.userName === this.currentUsername) ?? false;
  }

  toggleComments(postId: number): void {
    this.expandedComments[postId] = !this.expandedComments[postId];
  }

  submitComment(postId: number): void {
    const text = this.commentDrafts[postId];
    if (!text || !text.trim() || !this.currentUsername) {
      return;
    }

    this.postsService.addComment(postId, this.currentUsername, text);
    this.commentDrafts[postId] = '';
    this.commentMention.active = false;
  }

  startEditLimit(): void {
    this.limitDraft = this.maxPostsToShow;
    this.editingLimit = true;
  }

  cancelEditLimit(): void {
    this.editingLimit = false;
  }

  saveLimit(): void {
    const value = Math.max(1, Math.min(500, Math.floor(this.limitDraft) || 50));

    this.feedSettingsService.save(value).subscribe(() => {
      this.maxPostsToShow = value;
      this.editingLimit = false;
      this.postsService.refresh(this.maxPostsToShow);
    });
  }

  // ===== @mention handling =====

  onComposerInput(event: Event): void {
    const target = event.target as HTMLTextAreaElement;
    this.updateMentionState(this.composerText, target.selectionStart ?? this.composerText.length, this.composerMention);
  }

  onCommentInput(event: Event, postId: number): void {
    const target = event.target as HTMLInputElement;
    this.activeCommentPostId = postId;
    this.updateMentionState(this.commentDrafts[postId] || '', target.selectionStart ?? 0, this.commentMention);
  }

  selectComposerMention(user: MentionableUser): void {
    this.composerText = this.applyMention(this.composerText, this.composerMention, user);
    this.composerMention.active = false;
  }

  selectCommentMention(user: MentionableUser, postId: number): void {
    this.commentDrafts[postId] = this.applyMention(this.commentDrafts[postId] || '', this.commentMention, user);
    this.commentMention.active = false;
  }

  private updateMentionState(text: string, cursorIndex: number, state: MentionState): void {
    const upToCursor = text.slice(0, cursorIndex);
    const match = /@([\w.]*)$/.exec(upToCursor);

    if (!match) {
      state.active = false;
      return;
    }

    const query = match[1].toLowerCase();
    state.query = query;
    state.startIndex = match.index;
    state.suggestions = this.mentionableUsers
      .filter(u => u.username.toLowerCase().startsWith(query) || u.email.toLowerCase().startsWith(query))
      .slice(0, 6);
    state.active = state.suggestions.length > 0;
  }

  private applyMention(text: string, state: MentionState, user: MentionableUser): string {
    if (state.startIndex < 0) {
      return text;
    }

    const before = text.slice(0, state.startIndex);
    const after = text.slice(state.startIndex + 1 + state.query.length);

    return `${before}@${user.username} ${after}`;
  }
}
