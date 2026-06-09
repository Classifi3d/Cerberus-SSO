import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, switchMap, throwError } from 'rxjs';

import { StorageService } from '../services/storage.service';
import { OAuthService } from '../services/oauth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
	const storage = inject(StorageService);
	const oauthService = inject(OAuthService);

	const accessToken = storage.getAccessToken();

	let clonedRequest = req;

	if (accessToken) {
		clonedRequest = req.clone({
			setHeaders: {
				Authorization: `Bearer ${accessToken}`,
			},
		});
	}

	return next(clonedRequest).pipe(
		catchError((error: HttpErrorResponse) => {
			if (error.status !== 401) {
				return throwError(() => error);
			}

			const refreshToken = storage.getRefreshToken();

			if (!refreshToken) {
				storage.clear();
				return throwError(() => error);
			}

			return oauthService.refreshToken(refreshToken).pipe(
				switchMap((response: any) => {
					storage.setAccessToken(response.access_token);

					storage.setRefreshToken(response.refresh_token);

					const retryRequest = req.clone({
						setHeaders: {
							Authorization: `Bearer ${response.access_token}`,
						},
					});

					return next(retryRequest);
				}),
				catchError((refreshError) => {
					storage.clear();

					return throwError(() => refreshError);
				}),
			);
		}),
	);
};
