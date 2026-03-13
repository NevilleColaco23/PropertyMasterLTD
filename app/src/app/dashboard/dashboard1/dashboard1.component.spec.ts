import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Dashboard1Component } from './dashboard1.component';
import { Router } from '@angular/router';
import { MatSelectChange } from '@angular/material/select';

describe('Dashboard1Component', () => {
  let component: Dashboard1Component;
  let fixture: ComponentFixture<Dashboard1Component>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [Dashboard1Component],
      providers: [
        { provide: Router, useValue: mockRouter }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Dashboard1Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize on ngOnInit', () => {
    spyOn(console, 'log');
    component.ngOnInit();
    expect(console.log).toHaveBeenCalledWith('Dashboard1 component initialized');
  });

  it('should have default dashboard selected as dashboard1', () => {
    expect(component.selectedDashboard).toBe('dashboard1');
  });

  it('should have dashboard types defined', () => {
    expect(component.dashboardTypes.length).toBeGreaterThan(0);
    expect(component.dashboardTypes[0].value).toBe('dashboard1');
    expect(component.dashboardTypes[0].label).toBe('Overview Dashboard');
  });

  it('should handle dashboard change', () => {
    spyOn(console, 'log');
    const event = { value: 'dashboard1' } as MatSelectChange;
    component.onDashboardChange(event);
    expect(console.log).toHaveBeenCalledWith('Switching to dashboard:', 'Overview Dashboard');
  });

  it('should warn when selecting unimplemented dashboard', () => {
    spyOn(console, 'warn');
    const event = { value: 'dashboard2' } as MatSelectChange;
    component.onDashboardChange(event);
    expect(console.warn).toHaveBeenCalled();
  });
});
