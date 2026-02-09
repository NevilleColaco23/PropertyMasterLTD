export interface JwtPayload {
  sub?: string;        // Subject (user ID)
  userId?: string;     // Custom user ID claim
  username?: string;   // Username
  email?: string;      // Email
  jti?: string;        // JWT ID
  iat?: number;        // Issued at
  exp?: number;        // Expiration
  iss?: string;        // Issuer
  aud?: string;        // Audience
  nameid?: string;     // Alternative user ID claim
}
