import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });
    service = TestBed.inject(AuthService);
    // Clear localStorage before each test
    localStorage.clear();
  });

  afterEach(() => {
    // Clean up after each test
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getUserId', () => {
    it('should return null when no token is stored', () => {
      const userId = service.getUserId();
      expect(userId).toBeNull();
    });

    it('should extract userId from a valid JWT token', () => {
      // This is a sample JWT token with userId claim
      // Payload: { "userId": "12345", "username": "testuser", "email": "test@test.com", "exp": 9999999999 }
      const sampleToken = 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiIxMjM0NSIsInVzZXJuYW1lIjoidGVzdHVzZXIiLCJlbWFpbCI6InRlc3RAdGVzdC5jb20iLCJleHAiOjk5OTk5OTk5OTl9.8cV7Qj0d9vRpBqxJXz4MKCJZZsF1yJ0nQrV0Vj8xKJQ';
      localStorage.setItem('auth_tokenString', sampleToken);

      const userId = service.getUserId();
      expect(userId).toBe('12345');
    });

    it('should handle token without Bearer prefix', () => {
      const sampleToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiI2Nzg5MCIsInVzZXJuYW1lIjoidGVzdHVzZXIyIiwiZW1haWwiOiJ0ZXN0MkB0ZXN0LmNvbSIsImV4cCI6OTk5OTk5OTk5OX0.X8cV7Qj0d9vRpBqxJXz4MKCJZZsF1yJ0nQrV0Vj8xKJ';
      localStorage.setItem('auth_tokenString', sampleToken);

      const userId = service.getUserId();
      expect(userId).toBe('67890');
    });

    it('should return null for invalid JWT token', () => {
      localStorage.setItem('auth_tokenString', 'Bearer invalid.token.here');
      
      const userId = service.getUserId();
      expect(userId).toBeNull();
    });

    it('should extract userId from sub claim if userId claim is not present', () => {
      // Payload: { "sub": "user-sub-123", "username": "testuser", "email": "test@test.com", "exp": 9999999999 }
      const sampleToken = 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyLXN1Yi0xMjMiLCJ1c2VybmFtZSI6InRlc3R1c2VyIiwiZW1haWwiOiJ0ZXN0QHRlc3QuY29tIiwiZXhwIjo5OTk5OTk5OTk5fQ.X8cV7Qj0d9vRpBqxJXz4MKCJZZsF1yJ0nQrV0Vj8xKJ';
      localStorage.setItem('auth_tokenString', sampleToken);

      const userId = service.getUserId();
      expect(userId).toBe('user-sub-123');
    });
  });

  describe('getDecodedToken', () => {
    it('should return null when no token is stored', () => {
      const decoded = service.getDecodedToken();
      expect(decoded).toBeNull();
    });

    it('should return decoded token with all claims', () => {
      const sampleToken = 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiIxMjM0NSIsInVzZXJuYW1lIjoidGVzdHVzZXIiLCJlbWFpbCI6InRlc3RAdGVzdC5jb20iLCJleHAiOjk5OTk5OTk5OTl9.8cV7Qj0d9vRpBqxJXz4MKCJZZsF1yJ0nQrV0Vj8xKJQ';
      localStorage.setItem('auth_tokenString', sampleToken);

      const decoded = service.getDecodedToken();
      expect(decoded).toBeTruthy();
      expect(decoded.userId).toBe('12345');
      expect(decoded.username).toBe('testuser');
      expect(decoded.email).toBe('test@test.com');
    });

    it('should return null for invalid JWT token', () => {
      localStorage.setItem('auth_tokenString', 'Bearer invalid.token.here');
      
      const decoded = service.getDecodedToken();
      expect(decoded).toBeNull();
    });
  });

  describe('getUserToken', () => {
    it('should return null when no token is stored', () => {
      const token = service.getUserToken();
      expect(token).toBeNull();
    });

    it('should return stored token string', () => {
      const sampleToken = 'Bearer abc123';
      localStorage.setItem('auth_tokenString', sampleToken);

      const token = service.getUserToken();
      expect(token).toBe(sampleToken);
    });
  });

  describe('isSignedIn', () => {
    it('should return false when user is not signed in', () => {
      expect(service.isSignedIn()).toBeFalse();
    });
  });

  describe('signOut', () => {
    it('should clear all auth data from localStorage', () => {
      localStorage.setItem('auth_tokenString', 'Bearer test');
      localStorage.setItem('auth_userData', '{}');
      localStorage.setItem('auth_tokenExpiresAt', '123456789');

      service.signOut();

      expect(localStorage.getItem('auth_tokenString')).toBeNull();
      expect(localStorage.getItem('auth_userData')).toBeNull();
      expect(localStorage.getItem('auth_tokenExpiresAt')).toBeNull();
    });
  });
});
