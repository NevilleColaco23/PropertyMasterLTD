import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PropertyLandingComponent } from './property-landing.component';

describe('PropertyLandingComponent', () => {
  let component: PropertyLandingComponent;
  let fixture: ComponentFixture<PropertyLandingComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PropertyLandingComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PropertyLandingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
