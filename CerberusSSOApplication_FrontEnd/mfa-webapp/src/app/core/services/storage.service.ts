import { Injectable } from '@angular/core';

@Injectable({
	providedIn: 'root',
})
export class StorageService {
	private readonly ACCESS_TOKEN = 'OAuth-Token';
	private readonly REFRESH_TOKEN = 'Refresh-Token';
	private readonly PKCE_VERIFIER = 'pkce_verifier';

	setAccessToken(token: string): void {
		localStorage.setItem(this.ACCESS_TOKEN, token);
	}

	getAccessToken(): string | null {
		return localStorage.getItem(this.ACCESS_TOKEN);
	}

	setRefreshToken(token: string): void {
		localStorage.setItem(this.REFRESH_TOKEN, token);
	}

	getRefreshToken(): string | null {
		return localStorage.getItem(this.REFRESH_TOKEN);
	}

	setPkceVerifier(verifier: string): void {
		localStorage.setItem(this.PKCE_VERIFIER, verifier);
	}

	getPkceVerifier(): string | null {
		return localStorage.getItem(this.PKCE_VERIFIER);
	}

	clear(): void {
		localStorage.clear();
	}
}
