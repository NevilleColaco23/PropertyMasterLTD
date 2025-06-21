import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PropertyIndexComponent } from './property-index.component';

describe('PropertyIndexComponent', () => {
  let component: PropertyIndexComponent;
  let fixture: ComponentFixture<PropertyIndexComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PropertyIndexComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PropertyIndexComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
