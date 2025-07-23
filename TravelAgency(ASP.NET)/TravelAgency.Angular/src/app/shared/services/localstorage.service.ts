import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LocalStorageService {
  setItem(key: string, value: string) {
    localStorage.setItem(key, value);
  }

  getItem<T>(key: string): T | null {
    const item = localStorage.getItem(key);
    try {
      return item ? JSON.parse(item) : null;
    }
    catch {
      return null;
    }
  }

  removeItem(key: string) {
    localStorage.removeItem(key);
  }
}
