import { TestBed } from '@angular/core/testing';

import { ErrorHandlingCommonService } from './error-handling-common.service';

describe('ErrorHandlingCommonService', () => {
  let service: ErrorHandlingCommonService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ErrorHandlingCommonService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
