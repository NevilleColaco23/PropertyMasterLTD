import { Component, OnInit, OnDestroy,ChangeDetectorRef  } from '@angular/core';
import {AuthService} from '../core/auth/services/auth.service';
import { Subscription } from 'rxjs';
import { distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-index',
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.scss']
})
export class IndexComponent implements OnInit, OnDestroy {

  isLoggedIn: boolean = false;
  receivedData: any;

  private sub: Subscription;

  constructor(private auth : AuthService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.sub = this.auth.signInState.subscribe(
      (userData) => 
        {
          this.isLoggedIn = userData != null;
        }      
    ) 
  }

  ngOnDestroy() {
      this.sub.unsubscribe();
  }
}
