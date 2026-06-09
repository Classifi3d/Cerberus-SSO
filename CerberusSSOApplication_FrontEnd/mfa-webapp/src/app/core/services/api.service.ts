import { inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../models/user.model';
import { MfaVerificationDto } from '../models/mfa-verification.model';

@Injectable({
	providedIn: 'root',
})
export class ApiService {
	private http = inject(HttpClient);
	private url = 'https://localhost:7077';
	private headers = new HttpHeaders({
		'Content-Type': 'application/json',
		'Access-Control-Allow-Origin': '*',
	});

	private getAuthHeaders(): HttpHeaders {
		return new HttpHeaders({
			'Content-Type': 'application/json',
			Authorization: `Bearer ${localStorage.getItem('OAuth-Token')}`,
		});
	}

	private paramsUsers = new HttpParams();

	// ========== LOGIN ==========
	public loginUser(user: User): Observable<any> {
		return this.http.post<any>(`${this.url}/user/login`, user, {
			headers: this.getAuthHeaders(),
		});
	}

	// ========== MFA ==========
	public verifyMfaCode(mfaVerification: MfaVerificationDto): Observable<any> {
		console.log(mfaVerification);
		return this.http.post<any>(
			`${this.url}/user/verify-mfa`,
			mfaVerification,
			{
				headers: this.getAuthHeaders(),
			},
		);
	}

	public generateMfa(): Observable<Blob> {
		return this.http.post<Blob>(
			`${this.url}/user/enable-mfa`,
			{},
			{
				headers: this.getAuthHeaders(),
				responseType: 'blob' as 'json', // Make sure the response type is Blob
			},
		);
	}

	public disableMfa(): Observable<any> {
		return this.http.post<any>(
			`${this.url}/user/disable-mfa`,
			{},
			{ headers: this.getAuthHeaders() },
		);
	}

	// ========== SIGN UP ==========
	public signUpUser(user: User): Observable<void> {
		console.log(user);
		return this.http.post<void>(`${this.url}/user/sign-up`, user, {
			headers: this.headers,
		});
	}

	// ========== USER MENU ==========
	public getUserData(): Observable<any> {
		return this.http.get<any>(`${this.url}/user/user-data`, {
			headers: this.getAuthHeaders(),
		});
	}

	// ========== OAUTH ==========

	// Redirect user to authorization endpoint
	public authorizeClient(params: {
		client_id: string;
		redirect_uri: string;
		response_type: string;
		scope?: string;
		state?: string;
	}): void {
		const httpParams = new HttpParams()
			.set('client_id', params.client_id)
			.set('redirect_uri', params.redirect_uri)
			.set('response_type', params.response_type)
			.set('scope', params.scope || '')
			.set('state', params.state || '');

		// Browser redirect
		window.location.href = `${this.url}/OAuth/authorize?${httpParams.toString()}`;
	}

	// Exchange authorization code for tokens
	public exchangeToken(data: {
		grant_type: string; // "authorization_code"
		code: string;
		redirect_uri: string;
		client_id: string;
		client_secret: string;
	}): Observable<any> {
		const body = new HttpParams()
			.set('grant_type', data.grant_type)
			.set('code', data.code)
			.set('redirect_uri', data.redirect_uri)
			.set('client_id', data.client_id)
			.set('client_secret', data.client_secret);

		const headers = new HttpHeaders({
			'Content-Type': 'application/x-www-form-urlencoded',
		});

		return this.http.post<any>(`${this.url}/OAuth/token`, body.toString(), {
			headers,
		});
	}

	// Create OAuth client
	public createOAuthClient(client: {
		clientId: string;
		clientSecret: string;
		redirectUris: string[];
	}): Observable<any> {
		return this.http.post<any>(`${this.url}/OAuth/clients`, client, {
			headers: this.headers,
		});
	}
}
