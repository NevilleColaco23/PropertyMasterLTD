import { Injectable } from '@angular/core';
import { HttpHeaders } from '@angular/common/http';

/**
 * Service to help create HTTP headers with activity messages
 * Makes it easy for services to attach meaningful activity messages to API calls
 */
@Injectable({
  providedIn: 'root'
})
export class ActivityMessageService {

  /**
   * Creates HTTP headers with an activity message
   * @param message Human-readable activity message (e.g., "Added Ocean View Suite to 3rd floor")
   * @param existingHeaders Optional existing headers to merge with
   * @returns HttpHeaders with X-Activity-Message header
   */
  createHeaders(message: string, existingHeaders?: HttpHeaders): HttpHeaders {
    if (!existingHeaders) {
      existingHeaders = new HttpHeaders();
    }

    return existingHeaders.set('X-Activity-Message', message);
  }

  /**
   * Helper method to create headers with activity message for CREATE operations
   * @param entityName Name of the entity being created
   * @param additionalContext Optional additional context (e.g., "to 3rd floor")
   */
  createMessageForCreate(entityName: string, additionalContext?: string): HttpHeaders {
    const message = additionalContext
      ? `Added ${entityName} ${additionalContext}`
      : `Added ${entityName}`;
    
    return this.createHeaders(message);
  }

  /**
   * Helper method to create headers with activity message for UPDATE operations
   * @param entityName Name of the entity being updated
   * @param changes What was changed (e.g., "room rate", "status")
   */
  createMessageForUpdate(entityName: string, changes?: string): HttpHeaders {
    const message = changes
      ? `Updated ${entityName} - ${changes}`
      : `Updated ${entityName}`;
    
    return this.createHeaders(message);
  }

  /**
   * Helper method to create headers with activity message for DELETE operations
   * @param entityName Name of the entity being deleted
   */
  createMessageForDelete(entityName: string): HttpHeaders {
    return this.createHeaders(`Removed ${entityName}`);
  }

  /**
   * Helper method to create headers with activity message for STATUS CHANGE operations
   * @param entityName Name of the entity
   * @param newStatus The new status
   */
  createMessageForStatusChange(entityName: string, newStatus: string): HttpHeaders {
    return this.createHeaders(`Changed ${entityName} status to ${newStatus}`);
  }
}
