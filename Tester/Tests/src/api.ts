var _baseAddress: string

export interface Credentials {
	email: string,
	password?: string,
	xdc?: string
}

export function setBaseAddress(baseAddress: string) {
	_baseAddress = baseAddress;
}

export function ping() {
	return fetch(_baseAddress + "Authorization/Ping")
}

export function whoAmI(token: string) {
	return fetch(_baseAddress + "Authorization/WhoAmI", {
		method: "GET",
		headers: new Headers({
			Authentication: "Bearer token",
			Authorization: `Bearer ${token}`
		})
	})
}

export function getUsersCount(token: string) {
	return fetch(_baseAddress + "Authorization/GetUsersCount", {
		method: "GET",
		headers: new Headers({
			Authentication: "Bearer token",
			Authorization: `Bearer ${token}`
		})
	})
}

export function signUp(cred: Credentials) {
	return fetch(_baseAddress + "Authorization/SignUp", {
		method: "POST",
		headers: {
			'Accept': 'application/json',
			'Content-Type': 'application/json'
		},
		body: JSON.stringify(cred)
	})
}

export function signIn(cred: Credentials) {
	return fetch(_baseAddress + "Authorization/SignIn", {
		method: "POST",
		headers: {
			'Accept': 'application/json',
			'Content-Type': 'application/json'
		},
		body: JSON.stringify(cred)
	})
}

export function sendEmailConfirmation(cred: Credentials) {
	return fetch(_baseAddress + "Authorization/SendEmailConfirmation", {
		method: "POST",
		headers: {
			'Accept': 'application/json',
			'Content-Type': 'application/json'
		},
		body: JSON.stringify(cred)
	})
}

export function sendPasswordRedefinition(cred: Credentials) {
	return fetch(_baseAddress + "Authorization/SendPasswordRedefinition", {
		method: "POST",
		headers: {
			'Accept': 'application/json',
			'Content-Type': 'application/json'
		},
		body: JSON.stringify(cred)
	})
}

export function confirmEmail(cred: Credentials) {
	return fetch(_baseAddress + "Authorization/ConfirmEmail", {
		method: "POST",
		headers: {
			'Accept': 'application/json',
			'Content-Type': 'application/json'
		},
		body: JSON.stringify(cred)
	})
}

export function redefinePassword(cred: Credentials) {
	return fetch(_baseAddress + "Authorization/RedefinePassword", {
		method: "POST",
		headers: {
			'Accept': 'application/json',
			'Content-Type': 'application/json'
		},
		body: JSON.stringify(cred)
	})
}