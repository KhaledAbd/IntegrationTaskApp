import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LoaderService {
  private _loadingCount = 0;
  public isLoading = signal<boolean>(false);

  show() {
    this._loadingCount++;
    if (this._loadingCount === 1) {
      this.isLoading.set(true);
    }
  }

  hide() {
    this._loadingCount = Math.max(0, this._loadingCount - 1);
    if (this._loadingCount === 0) {
      this.isLoading.set(false);
    }
  }
}
