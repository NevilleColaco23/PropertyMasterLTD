export enum PostType {
  General = 0,
  Important = 1,
  Announcement = 2,
  Question = 3
}

export interface Attachment {
  url: string;
  fileName: string;
  contentType: string;
}

export interface Acknowledgement {
  userId: number;
  userName: string;
  acknowledgedAt: string;
}

export interface PostComment {
  id: number;
  userId: number;
  authorName: string;
  authorAlias?: string;
  text: string;
  mentions: string[];
  attachments: Attachment[];
  acknowledgements: Acknowledgement[];
  createdAt: string;
}

export interface Post {
  id: number;
  userId: number;
  authorName: string;
  authorAlias?: string;
  text: string;
  postType: PostType;
  mentions: string[];
  attachments: Attachment[];
  targetGroupId?: number;
  acknowledgements: Acknowledgement[];
  comments: PostComment[];
  createdAt: string;
}

export interface Group {
  id: number;
  name: string;
  description: string;
}
