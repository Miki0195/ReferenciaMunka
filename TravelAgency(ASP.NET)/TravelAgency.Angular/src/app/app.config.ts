import { provideHttpClient, withInterceptors, withJsonpSupport } from '@angular/common/http';
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { MAT_DATE_LOCALE, provideNativeDateAdapter } from '@angular/material/core';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { httpInterceptor } from './shared/auth/HttpConfigInterceptor';


export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withJsonpSupport(),withInterceptors([httpInterceptor])),
    provideAnimations(),
    provideNativeDateAdapter(),
    {provide: MAT_DATE_LOCALE, useValue: 'hu-HU'}
  ]
};
