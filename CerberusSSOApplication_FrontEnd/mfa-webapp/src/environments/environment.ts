export const environment = {
	production: false,

	// Plain http, and the same origin the Synapse client uses.
	//
	// The API calls UseHttpsRedirection, so running the `https` launch profile makes it
	// 307 every http request to https://localhost:7077. A browser will not follow a
	// redirect on a CORS preflight, so the cross-origin login and token calls fail
	// outright. Running the `http` profile leaves nothing to redirect to and the whole
	// flow stays on one scheme.
	apiUrl: 'http://localhost:5211',

	oauth: {
		clientId: 'angular-spa',
		redirectUri: 'http://localhost:4200/callback',
	},
};
