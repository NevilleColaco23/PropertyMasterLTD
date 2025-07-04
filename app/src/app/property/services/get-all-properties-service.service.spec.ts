import { TestBed } from '@angular/core/testing';

import { GetAllPropertiesServiceService } from './get-all-properties-service.service';

describe('GetAllPropertiesServiceService', () => {
  let service: GetAllPropertiesServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GetAllPropertiesServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
