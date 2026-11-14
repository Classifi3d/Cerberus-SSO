import { Component } from '@angular/core';
import {
	Validators,
	ReactiveFormsModule,
	FormBuilder,
	FormGroup,
	FormsModule,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { User } from '../../core/models/user.model';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { ApiService } from '../../core/services/api.service';
@Component({
	selector: 'app-login',
	standalone: true,
	imports: [
		MatFormFieldModule,
		MatInputModule,
		MatButtonModule,
		MatIconModule,
		FormsModule,
		ReactiveFormsModule,
	],
	templateUrl: './login.component.html',
	styleUrl: './login.component.scss',
})
export class LoginComponent {
	public hide = true;
	public loginForm!: FormGroup;
	public loginResponse: string | undefined;

	/**
	 * Set when this page is the authorization step of another application's OAuth flow.
	 * /OAuth/authorize caches the authorization request and redirects here with its id.
	 */
	private requestId: string | null = null;

	constructor(
		private formBuilder: FormBuilder,
		private apiService: ApiService,
		private router: Router,
		private route: ActivatedRoute,
	) {
		this.loginForm = this.formBuilder.group({
			email: ['', [Validators.required, Validators.email]],
			password: ['', [Validators.required, Validators.minLength(8)]],
		});
	}

	public ngOnInit(): void {
		localStorage.removeItem('OAuth-Token');
		localStorage.removeItem('Challenge-Token');

		this.requestId = this.route.snapshot.queryParamMap.get('requestId');
	}

	public onSubmit(): void {
		if (this.loginForm.valid) {
			const user = new User();
			user.email = this.loginForm.value.email;
			user.password = this.loginForm.value.password;
			this.apiService.loginUser(user, this.requestId).subscribe({
				next: (res) => {
					// An OAuth login resolves to a redirect back to the calling
					// application, carrying the authorization code. It is checked first
					// because no token is issued on this path - `res.token` is null, and
					// treating that as a failure is what left the flow stuck here.
					if (!!res.redirectUrl) {
						// A full page navigation, not router.navigate: the target is a
						// different origin and Angular's router cannot leave the app.
						window.location.href = res.redirectUrl;
						return;
					}

					const challengeToken = res.challengeId;
					if (!!challengeToken) {
						// The server keeps the pending authorization request alongside
						// the challenge, so the OAuth flow resumes after verification
						// without the requestId needing to survive in the browser.
						localStorage.setItem('Challenge-Token', challengeToken);
						this.router.navigate(['/multi-factor-auth']);
						return;
					}

					const oAuthToken = res.token;
					if (!!oAuthToken) {
						localStorage.setItem('OAuth-Token', oAuthToken);
						this.router.navigate(['/user-menu']);
					}
				},
				error: () => {
					console.log('Login Error!');
				},
			});
		}
	}

	public getEmailErrorMessage(): string {
		if (this.loginForm.get('email')?.hasError('required')) {
			return 'You must enter an email';
		}
		return this.loginForm.get('email')?.hasError('email')
			? 'Not a valid email'
			: '';
	}
	public getPasswordErrorMessage(): string {
		if (this.loginForm.get('password')?.hasError('required')) {
			return 'You must enter a password';
		}
		return this.loginForm.get('password')?.hasError('minLength')
			? ''
			: 'Password too short';
	}
}
