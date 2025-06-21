import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from './core-components/header/header.component';
import { FooterComponent } from './core-components/footer/footer.component';
import { RouterModule } from '@angular/router';
import { NgbCollapseModule } from '@ng-bootstrap/ng-bootstrap';
import { LoginFormComponent } from '../../app/core/auth/component/login-form/login-form.component';
import { NotFoundComponent } from './pages/not-found/not-found.component';
import { ReactiveFormsModule } from '@angular/forms';
import { IndexComponent } from '../_index/index.component';
import { PropertyIndexComponent } from '../property-index/property-index.component';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthInterceptor } from "./auth/interceptors/auth-interceptor";
import { ServerValidationErrorInterceptor } from './errorhandling/http-errors/services/http-error.interceptor';
import { CachingInterceptor } from './http/services/caching-interceptor';
import { HttpErrorNotificationComponent } from './errorhandling/http-errors/components/http-error-notification/http-error-notification.component';
import { CurrencySelectorComponent } from './exchangeRates/components/currency-selector/currency-selector.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { ShowValidationErrorsComponent } from './errorhandling/form-validation/components/show-validation-errors/show-validation-errors.component';
import { ItemCountSelectorComponent } from './core-components/item-count-selector/item-count-selector.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { PropertyHomepageComponent } from '../property-homepage/property-homepage.component';
import { CoreRoutingModule } from '../core/core-routing.module';
import { CalendarComponentModule } from '../core/core-components/calender/calender.module';
import { NavbarLoginInfoComponent } from '../../app/core/auth/component/navbar-login-info/navbar-login-info.component';

@NgModule({
  declarations: [HeaderComponent, FooterComponent,NavbarLoginInfoComponent,LoginFormComponent,NotFoundComponent,IndexComponent
    ,PropertyIndexComponent,HttpErrorNotificationComponent,CurrencySelectorComponent,ShowValidationErrorsComponent,ItemCountSelectorComponent
    ,PropertyHomepageComponent],
  imports: [CommonModule,RouterModule,NgbCollapseModule,FontAwesomeModule,ReactiveFormsModule,NgbModule,CoreRoutingModule,CalendarComponentModule],
  exports: [HeaderComponent, FooterComponent,NavbarLoginInfoComponent,LoginFormComponent,IndexComponent
    ,PropertyIndexComponent,CurrencySelectorComponent,ShowValidationErrorsComponent,ItemCountSelectorComponent,PropertyHomepageComponent
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ServerValidationErrorInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: CachingInterceptor,
      multi: true
    }
  ]
})
export class CoreModule { 
}
