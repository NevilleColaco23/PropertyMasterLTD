import { Component } from '@angular/core';
import {Subscription} from 'rxjs';
import {AuthService} from '../../auth/services/auth.service';
import { faCoffee } from '@fortawesome/free-solid-svg-icons';


@Component({
    selector: 'app-header',
    templateUrl: './header.component.html',
    styleUrls: ['./header.component.scss'],
    standalone: false
})

export class HeaderComponent {
  isCollapsed = true;
  isLoggedIn = false;
  faCoffee = faCoffee;
  
  private sub!: Subscription;

  constructor(private auth: AuthService) { }

  ngOnInit() {
    this.sub = this.auth.signInState.subscribe(
      userData => this.isLoggedIn = userData != null
    );
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }
}
