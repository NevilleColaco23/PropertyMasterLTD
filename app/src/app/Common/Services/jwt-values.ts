import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode'; 

@Injectable({
  providedIn: 'root'
})
export class JwtValues {

  constructor() { }

  getUserIdFromToken(): string | null {
    // 1. Get the token from storage (e.g., localStorage)
    const token = localStorage.getItem('accessToken');

    if (token) {
      try {
        // 2. Decode the token to get the payload
        // 💥 Use the imported function name: jwtDecode
        const decodedToken: any = jwtDecode(token); 
        
        // 3. Access the 'sub' claim from the payload
        return decodedToken.sub; 

      } catch (error) {
        console.error('Error decoding token:', error);
        return null;
      }
    }
    return null;
  }
}