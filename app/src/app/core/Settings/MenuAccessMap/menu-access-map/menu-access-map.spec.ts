import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MenuAccessMap } from './menu-access-map';

describe('MenuAccessMap', () => {
  let component: MenuAccessMap;
  let fixture: ComponentFixture<MenuAccessMap>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MenuAccessMap]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MenuAccessMap);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
