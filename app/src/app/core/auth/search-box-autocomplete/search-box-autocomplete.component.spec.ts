import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SearchBoxAutocompleteComponent } from './search-box-autocomplete.component';

describe('SearchBoxAutocompleteComponent', () => {
  let component: SearchBoxAutocompleteComponent;
  let fixture: ComponentFixture<SearchBoxAutocompleteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SearchBoxAutocompleteComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SearchBoxAutocompleteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
