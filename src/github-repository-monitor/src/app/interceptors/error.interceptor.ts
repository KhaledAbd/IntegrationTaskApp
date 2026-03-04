import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { DialogService } from '@progress/kendo-angular-dialog';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const dialogService = inject(DialogService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unknown error occurred!';

      if (error.error instanceof ErrorEvent) {
        // Client-side or network error
        errorMessage = `Error: ${error.error.message}`;
      } else if (error.status === 0) {
        // Server unreachable
        errorMessage = 'Server not available, please try again.';
      } else {
        // Backend error
        errorMessage = `Unable to fetch data, please try again.`;

        // If the backend returns a specific error message in the body, prefer that
        if (error.error && typeof error.error === 'string') {
          errorMessage = error.error;
        } else if (error.error && error.error.message) {
          errorMessage = error.error.message;
        }
      }

      dialogService.open({
        title: 'Error',
        content: errorMessage,
        actions: [{ text: 'OK', themeColor: 'primary' }],
        width: 450,
        minWidth: 250,
      });

      return throwError(() => error);
    }),
  );
};
