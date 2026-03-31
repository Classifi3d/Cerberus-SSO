// import { inject } from '@angular/core';
// import {
// 	HttpInterceptorFn,
// 	HttpRequest,
// 	HttpHandlerFn,
// 	HttpEvent,
// 	HttpErrorResponse,
// } from '@angular/common/http';
// import { Observable, throwError, switchMap, catchError } from 'rxjs';
// import { ApiService } from './api.service';

// export const authInterceptor: HttpInterceptorFn = (
// 	req: HttpRequest<any>,
// 	next: HttpHandlerFn
// ): Observable<HttpEvent<any>> => {
// 	const api = inject(ApiService);

// 	const token = localStorage.getItem('OAuth-Token');

// 	let authReq = req;

// 	// Attach token if exists
// 	if (token) {
// 		authReq = req.clone({
// 			setHeaders: {
// 				Authorization: `Bearer ${token}`,
// 			},
// 		});
// 	}

// 	return next(authReq).pipe(
// 		catchError((error: HttpErrorResponse) => {
// 			// Handle 401 → try refresh
// 			if (error.status === 401) {
// 				const refreshToken = localStorage.getItem('Refresh-Token');

// 				if (!refreshToken) {
// 					return throwError(() => error);
// 				}

// 				return api.refreshToken(refreshToken).pipe(
// 					switchMap((res: any) => {
// 						localStorage.setItem('OAuth-Token', res.access_token);
// 						localStorage.setItem('Refresh-Token', res.refresh_token);

// 						const retryReq = req.clone({
// 							setHeaders: {
// 								Authorization: `Bearer ${res.access_token}`,
// 							},
// 						});

// 						return next(retryReq);
// 					}),
// 					catchError(err => {
// 						// Refresh failed → logout
// 						localStorage.clear();
// 						return throwError(() => err);
// 					})
// 				);
// 			}

// 			return throwError(() => error);
// 		})
// 	);
// };
