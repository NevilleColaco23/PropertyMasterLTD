import { Injectable } from '@angular/core';
import { HttpClient, HttpEventType, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, filter } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface CloudinaryResponse {
  secure_url: string;
  public_id: string;
  url: string;
  original_filename: string;
  format: string;
  width: number;
  height: number;
  bytes: number;
}

export interface UploadProgress {
  progress: number; // 0-100
  status: 'uploading' | 'completed';
  response?: CloudinaryResponse;
}

@Injectable({
  providedIn: 'root'
})
export class CloudinaryUploadService {
  private cloudName = environment.cloudinary.cloudName;
  private uploadPreset = environment.cloudinary.uploadPreset;

  constructor(private http: HttpClient) {
    // Validate configuration on initialization
    if (!this.cloudName || this.cloudName === 'YOUR_CLOUD_NAME') {
      console.error('⚠️ Cloudinary cloud name not configured! Please update environment.ts');
    }
    if (!this.uploadPreset || this.uploadPreset === 'YOUR_UPLOAD_PRESET') {
      console.error('⚠️ Cloudinary upload preset not configured! Please update environment.ts');
    }
  }

  /**
   * Uploads an image file to Cloudinary
   * @param file The image file to upload
   * @param folder Optional folder name in Cloudinary (default: 'property-logos')
   * @returns Observable with the Cloudinary response containing the image URL
   */
  uploadImage(file: File, folder: string = 'property-logos'): Observable<CloudinaryResponse> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('upload_preset', this.uploadPreset);
    formData.append('folder', folder);

    // Optional: Add transformations during upload
    // formData.append('transformation', 'c_fill,h_400,w_400,q_auto,f_auto');

    return this.http.post<CloudinaryResponse>(
      `https://api.cloudinary.com/v1_1/${this.cloudName}/image/upload`,
      formData
    );
  }

  /**
   * Uploads an image file to Cloudinary with progress tracking
   * @param file The image file to upload
   * @param folder Optional folder name in Cloudinary (default: 'property-logos')
   * @returns Observable that emits upload progress updates
   */
  uploadImageWithProgress(file: File, folder: string = 'property-logos'): Observable<UploadProgress> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('upload_preset', this.uploadPreset);
    formData.append('folder', folder);

    return this.http.post<CloudinaryResponse>(
      `https://api.cloudinary.com/v1_1/${this.cloudName}/image/upload`,
      formData,
      {
        reportProgress: true,
        observe: 'events'
      }
    ).pipe(
      map((event: HttpEvent<any>) => {
        if (event.type === HttpEventType.UploadProgress) {
          const progress = event.total ? Math.round((100 * event.loaded) / event.total) : 0;
          return {
            progress,
            status: 'uploading' as const
          };
        } else if (event.type === HttpEventType.Response) {
          return {
            progress: 100,
            status: 'completed' as const,
            response: event.body as CloudinaryResponse
          };
        }
        return { progress: 0, status: 'uploading' as const };
      }),
      filter(progress => progress.progress > 0) // Only emit when we have actual progress
    );
  }

  /**
   * Deletes an image from Cloudinary using its public_id
   * Note: This requires authenticated requests, so you might need to implement this on the backend
   */
  deleteImage(publicId: string): Observable<any> {
    // This would typically be done on the backend with your Cloudinary API secret
    console.warn('Image deletion should be implemented on the backend for security');
    return new Observable(observer => {
      observer.error('Image deletion must be done on the backend');
    });
  }

  /**
   * Extracts the public_id from a Cloudinary URL
   * @param url The Cloudinary URL
   * @returns The public_id or null if not a valid Cloudinary URL
   */
  extractPublicId(url: string): string | null {
    const match = url.match(/\/v\d+\/(.+)\.\w+$/);
    return match ? match[1] : null;
  }
}
