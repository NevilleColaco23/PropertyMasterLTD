import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PropertyHomepageComponent } from './property-homepage.component';

describe('PropertyHomepageComponent', () => {
  let component: PropertyHomepageComponent;
  let fixture: ComponentFixture<PropertyHomepageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PropertyHomepageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PropertyHomepageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
