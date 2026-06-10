import { inject, Injectable } from '@angular/core';
import { OAuthService } from './oauth.service';
import { StorageService } from './storage.service';

@Injectable({
	providedIn: 'root',
})
export class AuthService {
	private oauthService = inject(OAuthService);
	private storage = inject(StorageService);

	login(): Promise<void> {
		return this.oauthService.authorize();
	}

	logout(): void {
		this.storage.clear();
	}

	isAuthenticated(): boolean {
		return !!this.storage.getAccessToken();
	}

	getAccessToken(): string | null {
		return this.storage.getAccessToken();
	}

	saveTokens(response: any): void {
		this.storage.setAccessToken(response.access_token);

		this.storage.setRefreshToken(response.refresh_token);
	}
}
