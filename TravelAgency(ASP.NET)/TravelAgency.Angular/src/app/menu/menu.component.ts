import { NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatButton } from '@angular/material/button';
import { MatToolbar } from '@angular/material/toolbar';
import { RouterLink } from '@angular/router';
import { UserService } from '@shared/services/user.service';

@Component({
  selector: 'app-menu',
  imports: [
    MatToolbar, MatButton, RouterLink, NgIf
  ],
  templateUrl: './menu.component.html',
  standalone: true,
  styleUrl: './menu.component.css'
})
export class MenuComponent implements OnInit {
  loggedIn = false;
  constructor(private userService: UserService) {
    this.loggedIn = userService.isLoggedIn();
  }

  ngOnInit() {
    this.userService.loggedInStatusChanged$.subscribe(status => {
      this.loggedIn = status;
    });
  }

  logout() {
    this.userService.logout().subscribe();
  }
}
