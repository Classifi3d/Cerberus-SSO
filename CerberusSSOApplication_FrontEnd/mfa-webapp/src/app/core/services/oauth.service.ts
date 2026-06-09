import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PkceService } from './pkce.service';
import { StorageService } from './storage.service';

@Injectable({
	providedIn: 'root',
})
export class OAuthService {
	private http = inject(HttpClient);
	private pkceService = inject(PkceService);
	private storage = inject(StorageService);

	private readonly apiUrl = 'https://localhost:7077';

	async authorize(): Promise<void> {
		const verifier = this.pkceService.generateCodeVerifier();

		const challenge =
			await this.pkceService.generateCodeChallenge(verifier);

		this.storage.setPkceVerifier(verifier);

		const params = new HttpParams()
			.set('client_id', 'angular-spa')
			.set('redirect_uri', 'http://localhost:4200/callback')
			.set('response_type', 'code')
			.set('scope', 'openid profile')
			.set('code_challenge', challenge)
			.set('code_challenge_method', 'S256');

		window.location.href = `${this.apiUrl}/OAuth/authorize?${params.toString()}`;
	}

	exchangeToken(code: string): Observable<any> {
		const verifier = this.storage.getPkceVerifier() ?? '';

		const body = new HttpParams()
			.set('grant_type', 'authorization_code')
			.set('code', code)
			.set('redirect_uri', 'http://localhost:4200/callback')
			.set('client_id', 'angular-spa')
			.set('code_verifier', verifier);

		const headers = new HttpHeaders({
			'Content-Type': 'application/x-www-form-urlencoded',
		});

		return this.http.post(`${this.apiUrl}/OAuth/token`, body.toString(), {
			headers,
		});
	}

	refreshToken(refreshToken: string): Observable<any> {
		const body = new HttpParams()
			.set('grant_type', 'refresh_token')
			.set('refresh_token', refreshToken)
			.set('client_id', 'angular-spa');

		const headers = new HttpHeaders({
			'Content-Type': 'application/x-www-form-urlencoded',
		});

		return this.http.post(`${this.apiUrl}/OAuth/token`, body.toString(), {
			headers,
		});
	}

	createClient(request: any): Observable<any> {
		return this.http.post(`${this.apiUrl}/OAuth/clients`, request);
	}
}
