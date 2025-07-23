import { NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatCard, MatCardContent, MatCardTitle } from '@angular/material/card';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { Router } from '@angular/router';
import { UserDto } from '@shared/models/dtos/userDto';
import { UserService } from '@shared/services/user.service';

@Component({
  selector: 'app-create-user',
  templateUrl: './create-user.component.html',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardContent,
    MatCardTitle,
    MatCard,
    MatError,
    MatLabel,
    MatFormField,
    MatInput,
    NgIf,
    MatButton
  ],
  styleUrls: ['./create-user.component.css']
})
export class CreateUserComponent {
  userForm: FormGroup;
  errorMessage: string | null = null;

  constructor(private fb: FormBuilder, private userService: UserService, private router: Router) {
    this.userForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(60)]],
      email: ['', [Validators.required, Validators.email]],
      address: ['', Validators.required],
      phoneNumber: ['', [Validators.required, Validators.pattern("^[0-9\\-\\+]{9,15}$")]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  getErrorMessage(field: string): string {
    const control = this.userForm.get(field);

    if(control){
      if (control.hasError('required')) {
        return 'A mező kitöltése kötelező';
      }
      if (control.hasError('email')) {
        return 'Érvénytelen email cím';
      }
      if (control.hasError('pattern')) {
        return 'Érvénytelen telefonszám';
      }
      if (control.hasError('maxLength')) {
        return 'A név túl hosszú';
      }
      if (control.hasError('minlength')) {
        return 'A jelszó túl rövid';
      }
    }

    return '';
  }

  onSubmit() {
    if (this.userForm.valid) {
      const userDto: UserDto = this.userForm.value;
      this.userService.createUser(userDto).subscribe({
        next: (_: UserDto) => {
          this.router.navigate(['/login']).then(_ => _);
        },
        error: (error) => {
          this.errorMessage = error.response;
          this.resetMessage();
        }
      })
    }
  }

  private resetMessage() {
    setTimeout(() => {
      this.errorMessage = null;
    }, 3000);
  }
}
