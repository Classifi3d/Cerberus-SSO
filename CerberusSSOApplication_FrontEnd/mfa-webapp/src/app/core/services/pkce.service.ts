import { Injectable } from '@angular/core';

@Injectable({
	providedIn: 'root',
})
export class PkceService {
	generateCodeVerifier(length: number = 128): string {
		const chars =
			'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~';

		let result = '';

		for (let i = 0; i < length; i++) {
			result += chars.charAt(Math.floor(Math.random() * chars.length));
		}

		return result;
	}

	async generateCodeChallenge(codeVerifier: string): Promise<string> {
		const data = new TextEncoder().encode(codeVerifier);

		const digest = await crypto.subtle.digest('SHA-256', data);

		return this.base64UrlEncode(digest);
	}

	private base64UrlEncode(buffer: ArrayBuffer): string {
		return btoa(String.fromCharCode(...new Uint8Array(buffer)))
			.replace(/\+/g, '-')
			.replace(/\//g, '_')
			.replace(/=+$/, '');
	}
}
