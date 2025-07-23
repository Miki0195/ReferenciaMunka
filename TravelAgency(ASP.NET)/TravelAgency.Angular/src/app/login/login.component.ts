import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatCard, MatCardContent, MatCardTitle } from '@angular/material/card';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '@shared/services/user.service';

@Component({
  selector: 'app-login',
  imports: [
    MatCard,
    MatCardContent,
    MatFormField,
    ReactiveFormsModule,
    MatCardTitle,
    MatError,
    MatInput,
    MatLabel,
    NgIf,
    MatButton
  ],
  templateUrl: './login.component.html',
  standalone: true,
  styleUrl: './login.component.css'
})
export class LoginComponent {
  loginForm: FormGroup;
  errorMessage: string | null = null;

  constructor(private fb: FormBuilder, private userService: UserService, private router: Router, private route: ActivatedRoute) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      this.userService.login(this.loginForm.value).subscribe({
          next: () => {
            const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
            this.router.navigateByUrl(returnUrl).then(_ => _);
          },
         error: (error) => {
          this.errorMessage = error.response;
          this.resetMessage();
        }
      });
    }
  }

  getErrorMessage(field: string): string {
    const control = this.loginForm.get(field);

    if(control){
      if (control.hasError('required')) {
        return 'A mező kitöltése kötelező';
      }
      if (control.hasError('email')) {
        return 'Érvénytelen email cím';
      }
      if (control.hasError('minlength')) {
        return 'A jelszó túl rövid';
      }
    }
    return '';
  }

  private resetMessage() {
    setTimeout(() => {
      this.errorMessage = null;
    }, 3000);
  }
}
