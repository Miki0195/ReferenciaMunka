import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterOutlet } from '@angular/router';

import { MenuComponent } from './menu/menu.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, MatToolbarModule, RouterOutlet, MenuComponent],
  template: `
    <app-menu></app-menu>
    <router-outlet></router-outlet>
  `
})

export class AppComponent {
  title = 'TravelAgency.Angular';
}
