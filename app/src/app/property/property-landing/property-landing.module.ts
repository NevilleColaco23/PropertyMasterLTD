import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PropertyLandingRoutingModule } from './property-landing-routing.module';
import { PropertyLandingComponent } from './property-landing.component';
import { FormsModule, ReactiveFormsModule  } from '@angular/forms';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';
import { MatIcon } from '@angular/material/icon';
import { MatCard } from '@angular/material/card';
import { MatButton } from '@angular/material/button';
import { SearchBoxAutocompleteComponent } from '../../core/search-box-autocomplete/search-box-autocomplete.component';
import { MatCardModule  } from '@angular/material/card';
import { MatProgressBarModule  } from '@angular/material/progress-bar';
import { MatPaginatorModule  } from '@angular/material/paginator';
import { systemmessagessnackbarModule } from '../../core/system-messages-snackbar/system-message-snackbar.module';

@NgModule({
  declarations: [
    PropertyLandingComponent,
    SearchBoxAutocompleteComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatToolbarModule,
    MatMenuModule,
    MatIcon,
    MatCard,
    MatButton,
    MatCardModule,
    MatProgressBarModule,
    MatPaginatorModule,
    PropertyLandingRoutingModule,
    systemmessagessnackbarModule
  ]
})
export class PropertyLandingModule { }
