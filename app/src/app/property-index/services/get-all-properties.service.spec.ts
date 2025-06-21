import { TestBed } from '@angular/core/testing';

import { GetAllPropertiesService } from './get-all-properties.service';

describe('GetAllPropertiesService', () => {
  let service: GetAllPropertiesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GetAllPropertiesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
