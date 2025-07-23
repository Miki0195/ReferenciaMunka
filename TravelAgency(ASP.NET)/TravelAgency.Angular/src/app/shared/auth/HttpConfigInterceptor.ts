import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { UserService } from '../services/user.service';

export const httpInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const userService = inject(UserService);

  const clonedRequest = req.clone({
    withCredentials: true
  });

  return next(clonedRequest).pipe(
    catchError((error: HttpErrorResponse) => {

      const handleErrResponse = {
        status: error.status,
        errorText: error.message || error.error?.errorMessage || error.error?.title,
        response: error.error,
      };

      if (error.status === 401) {
        userService.updateLoggedInStatus(false);
        router.navigate(['/login']).then(_ => _);
      }

      if (error.status === 400 && error.error && error.error.errors) {
        // Handle ValidationProblemDetails
        const validationErrors = error.error.errors;
        console.error('Validation errors:', validationErrors);

        // Process validation errors
        handleErrResponse.response = Object.entries(validationErrors).map(([field, messages]) => ({
          field,
          messages: Array.isArray(messages) ? messages : [messages]
        })).flatMap(f => f.messages).join(" ");
      }

        return throwError(() => handleErrResponse);
    })
  );
};
