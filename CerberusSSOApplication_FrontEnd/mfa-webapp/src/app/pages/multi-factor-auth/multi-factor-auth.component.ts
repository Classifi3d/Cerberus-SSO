import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MfaVerificationDto } from '../../core/models/mfa-verification.model';
import { ApiService } from '../../core/services/api.service';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
	selector: 'app-multi-factor-auth',
	standalone: true,
	imports: [FormsModule, CommonModule],
	templateUrl: './multi-factor-auth.component.html',
	styleUrls: ['./multi-factor-auth.component.scss'],
})
export class MultiFactorAuthComponent {
	error: string | null = null;
	constructor(
		private apiService: ApiService,
		private router: Router,
	) {}
	pin: string[] = ['', '', '', '', '', ''];

	public onInput(currentIndex: number, event: any): void {
		const value = event.target.value;
		if (value.length > 1) {
			event.target.value = value.charAt(0);
		}

		this.pin[currentIndex] = event.target.value;

		if (event.target.value && currentIndex < 5) {
			setTimeout(() => {
				const nextInput = document.getElementsByName(
					`pin${currentIndex + 1}`,
				)[0] as HTMLInputElement;
				if (nextInput) {
					nextInput.value = '';
					nextInput.focus();
				}
			}, 0);
		}
	}

	public onFocus(currentIndex: number): void {
		setTimeout(() => {
			const inputElement = document.getElementsByName(
				`pin${currentIndex}`,
			)[0] as HTMLInputElement;
			if (inputElement && inputElement.value === '') {
				inputElement.value = '';
			}
		}, 100);
	}

	public verifyMfa(): void {
		const pinCode = this.pin.join('');

		if (pinCode.length === 6) {
			console.log('PIN code is valid:', pinCode);
		} else {
			console.log('Invalid PIN code');
		}

		const challengeId = localStorage.getItem('Challenge-Token');
		if (!!challengeId) {
			const mfaVerificationDto: MfaVerificationDto = {
				challengeId: challengeId,
				code: pinCode,
			};
			console.log(mfaVerificationDto);
			this.apiService.verifyMfaCode(mfaVerificationDto).subscribe({
				next: (res) => {
					localStorage.removeItem('Challenge-Token');

					// Same as the login page: when this verification completes another
					// application's OAuth flow, the result is a redirect carrying the
					// authorization code rather than a token of our own.
					if (!!res.redirectUrl) {
						window.location.href = res.redirectUrl;
						return;
					}

					const oAuthToken = res.token;
					if (!!oAuthToken) {
						localStorage.setItem('OAuth-Token', oAuthToken);
						this.router.navigate(['/user-menu']);
					} else {
						console.error('No token or redirect in the MFA response');
						this.error = 'Invalid PIN!';
					}
				},
				error: (err) => {
					this.error = 'Invalid PIN!';

					console.error('Error verifying MFA', err);
				},
			});
		} else {
			console.error('ChallengeId not found in localStorage');
		}
	}
}
