import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SystemMessagesSnackbarComponent } from './system-messages-snackbar.component';

describe('SystemMessagesSnackbarComponent', () => {
  let component: SystemMessagesSnackbarComponent;
  let fixture: ComponentFixture<SystemMessagesSnackbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SystemMessagesSnackbarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SystemMessagesSnackbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
