export interface User {
	id: string,
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
