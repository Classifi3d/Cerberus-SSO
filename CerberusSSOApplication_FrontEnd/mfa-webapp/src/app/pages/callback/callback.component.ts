import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { OAuthService } from '../../core/services/oauth.service';

@Component({
	selector: 'app-callback',
	template: '',
})
export class CallbackComponent implements OnInit {
	private oauthService = inject(OAuthService);
	private authService = inject(AuthService);
	private router = inject(Router);

	ngOnInit(): void {
		const params = new URLSearchParams(window.location.search);

		const code = params.get('code');

		if (!code) {
			this.router.navigate(['/login']);
			return;
		}

		this.oauthService.exchangeToken(code).subscribe({
			next: (response) => {
				this.authService.saveTokens(response);

				this.router.navigate(['/dashboard']);
			},
			error: () => {
				this.router.navigate(['/login']);
			},
		});
	}
}
