export interface User {
	username: string,
	email: string
}

export interface ApiErrorResponse {
	error: string
}

export interface LoginSuccessResponse {
	token: string,
	user: User
}

export interface RegisterResponse {
	succeeded: boolean,
	error?: string
}
