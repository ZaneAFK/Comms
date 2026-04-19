export interface User {
	username: string,
	email: string
}

export interface ApiErrorResponse {
	error: string
}

export interface RegisterResponse {
	succeeded: boolean,
	error?: string
}
