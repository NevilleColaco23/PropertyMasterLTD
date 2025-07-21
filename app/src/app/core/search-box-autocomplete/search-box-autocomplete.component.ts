import { Component,  ElementRef,  ViewChild,  AfterViewInit,  OnDestroy, Output, EventEmitter } from '@angular/core';
import { debounceTime,  distinctUntilChanged,  Subject,  takeUntil,  catchError,  map,  of,} from 'rxjs';
import { Router } from '@angular/router';
import { ErrorHandlingCommonServiceService } from '../../Common/Services/error-handling-common-service.service';
import { HttpClient } from "@angular/common/http";
import { AppConfig } from '../../Appconfig';

export interface ApiResponse<T> {
  pageIndex: number;
  pageSize: number;
  pageCount: number;
  rowCount: number;
  activeFilter: string | null;
  activeOrderBy: string;
  firstRowOnPage: number;
  lastRowOnPage: number;
  results: T[];
}

export interface GetSearchResultsDTO {
  label: string;
  path: string;
  parentLabel: string | null;
  isSubItem: boolean;
}


@Component({
  selector: 'app-search-box-autocomplete',
  templateUrl: './search-box-autocomplete.component.html',
  styleUrl: './search-box-autocomplete.component.css'
})
export class SearchBoxAutocompleteComponent implements AfterViewInit, OnDestroy {
  searchQuery = '';
  searchSuggestions: (GetSearchResultsDTO & { displayLabel: string })[] = [];
  selectedSuggestionIndex = -1;
  isLoadingSuggestions = false;
  private pathAPI : string;
  private searchInputChanged$ = new Subject<string>();
  private destroy$ = new Subject<void>();
@Output() searchSelected = new EventEmitter<GetSearchResultsDTO>();

  @ViewChild('searchBoxWrapper') searchBoxWrapper!: ElementRef;

  constructor(   
    private router: Router, private errorHandling: ErrorHandlingCommonServiceService,private http: HttpClient
  ,private config: AppConfig) {
      this.pathAPI = this.config.setting['PathAPI'];
    }

  ngAfterViewInit(): void {
    document.addEventListener('click', this.onDocumentClick.bind(this));

    this.searchInputChanged$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe((query) => {
        if (query.trim()) {
          this.fetchSearchSuggestions(query);
        } else {
          this.searchSuggestions = [];
        }
      });
  }

  onSearchQueryChanged(value: string): void {
    this.searchInputChanged$.next(value);
  }

  fetchSearchSuggestions(query: string): void {
  this.isLoadingSuggestions = true;

  this.getSearchSuggestionsFromApi(query)
    .pipe(
      catchError((err) => {
        this.errorHandling.handleError(err);
        return of([]);
      }),
      takeUntil(this.destroy$)
    )
    .subscribe((suggestions: any) => {
      this.searchSuggestions = suggestions;
      this.isLoadingSuggestions = false;
      this.selectedSuggestionIndex = -1;
    });
}


  getSearchSuggestionsFromApi(query: string) {
    // Specify the expected API response type: ApiResponse<GetSearchResultsDTO>
    return this.http.get<ApiResponse<GetSearchResultsDTO>>(`${this.pathAPI}v1/menu/search?SearchText=${query}`)
       .pipe(
      //   map(response => {
      //     console.log('Raw API response for search suggestions:', response);

      //     // Access the 'results' array from the response object
      //     // and then map over that array.
      //     return response.results.map(item => ({
      //       ...item,
      //       displayLabel: item.parentLabel ? `${item.parentLabel} > ${item.label}` : item.label
      //     }));
      //   }),
      // Inside getSearchSuggestionsFromApi method, within the map operator:
map(response => {
  console.log('Raw API response for search suggestions:', response);

  return response.results.map((item: any) => { // Use 'any' temporarily or define a new interface for the raw API item
    const labelFromApi = Array.isArray(item.searchedItem) && item.searchedItem.length > 0
                         ? item.searchedItem[0]
                         : (typeof item.searchedItem === 'string' ? item.searchedItem : ''); // Handle if it's a string or other cases

              return {
                label: labelFromApi,
                path: item.path || '', // Make sure 'path' exists, provide default if not
                parentLabel: item.parentLabel || null, // Make sure 'parentLabel' exists
                isSubItem: item.isSubItem || false, // Make sure 'isSubItem' exists
                displayLabel: item.parentLabel
                  ? `${item.parentLabel} > ${labelFromApi}`
                  : labelFromApi
              };
            });
          }),
        catchError(err => {
          this.errorHandling.handleError(err);
          return of([]); // Return an empty observable array on error
        }),
        takeUntil(this.destroy$) // Ensure takeUntil is the last RxJS operator before subscribe
      );
  }


  
  onSearchFocus(): void {
    if (this.searchSuggestions.length === 0 && this.searchQuery.trim()) {
      this.fetchSearchSuggestions(this.searchQuery);
    }
  }

  onKeyDown(event: KeyboardEvent): void {
    if (!this.searchSuggestions.length) return;

    const total = this.searchSuggestions.length;

    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.selectedSuggestionIndex =
        (this.selectedSuggestionIndex + 1) % total;
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.selectedSuggestionIndex =
        (this.selectedSuggestionIndex - 1 + total) % total;
    } else if (event.key === 'Enter') {
      event.preventDefault();
      if (this.selectedSuggestionIndex !== -1) {
        const selected = this.searchSuggestions[this.selectedSuggestionIndex];
        this.onSuggestionClick(selected);
      }
    }
  }

  onSuggestionClick(suggestion: GetSearchResultsDTO): void {
    this.searchSelected.emit(suggestion); // Emit the selected DTO
    this.clearSearch(); // Clear the search box
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.searchSuggestions = [];
    this.selectedSuggestionIndex = -1;
  }

  onDocumentClick(event: MouseEvent): void {
    if (
      this.searchBoxWrapper &&
      !this.searchBoxWrapper.nativeElement.contains(event.target)
    ) {
      this.clearSearch();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}