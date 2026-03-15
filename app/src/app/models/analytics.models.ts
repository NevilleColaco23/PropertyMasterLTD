// ============================================
// ANALYTICS MODELS
// TypeScript interfaces for analytics data
// ============================================

export interface ActivityAnalyticsSummary {
  totalActivities: number;
  uniqueUsers: number;
  uniqueEntities: number;
  averageDuration: number;
  successCount: number;
  failureCount: number;
  successRate: number;
  mostActiveUser: string;
  mostCommonActivityType: string;
  earliestActivity: Date | null;
  latestActivity: Date | null;
}

export interface UserActivityStats {
  userId: number;
  username: string;
  totalActivities: number;
  successfulActivities: number;
  failedActivities: number;
  successRate: number;
  lastActivityTime: Date;
  topActivityTypes: string[];
  mostAccessedEntities: string[];
}

export interface ActivityTypeDistribution {
  activityType: string;
  count: number;
  percentage: number;
  averageDuration: number;
  successCount: number;
  failureCount: number;
}

export interface EntityAccessStats {
  entityType: string;
  entityId: string;
  accessCount: number;
  uniqueUsers: number;
  firstAccess: Date;
  lastAccess: Date;
  topUsers: string[];
}

export interface PeakUsageTime {
  hour: number;
  hourLabel: string;
  activityCount: number;
  topActivityTypes: string[];
}

export interface DailyActivityTrend {
  date: Date;
  dateLabel: string;
  totalActivities: number;
  successfulActivities: number;
  failedActivities: number;
  uniqueUsers: number;
  averageDuration: number;
}

export interface FailedLoginAttempt {
  activityId: number;
  username: string;
  ipAddress: string;
  userAgent: string;
  timestamp: Date;
  errorMessage: string;
  attemptCount: number;
}

export interface SecurityAlertSummary {
  failedLoginCount: number;
  suspiciousIPCount: number;
  multipleFailedAttemptsCount: number;
  recentFailedLogins: FailedLoginAttempt[];
  suspiciousIPAddresses: string[];
}

export interface PerformanceMetrics {
  activityType: string;
  averageDuration: number;
  minDuration: number;
  maxDuration: number;
  medianDuration: number;
  totalCount: number;
  slowRequestCount: number;
}

export interface ActivityExport {
  activityId: number;
  username: string;
  activityType: string;
  entityType: string;
  entityId: string;
  description: string;
  ipAddress: string;
  timestamp: Date;
  duration: number;
  status: string;
  errorMessage: string;
}
