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

export interface MessageDto {
	id: string
	conversationId: string
	senderId: string
	senderUsername: string
	content: string
	sentAt: string
}

export interface ConversationMemberDto {
	userId: string
	username: string
}

export interface ConversationDto {
	id: string
	name: string
	createdAt: string
	members: ConversationMemberDto[]
	lastMessage: MessageDto | null
}

export interface UserSearchResult {
	id: string
	username: string
}

export interface TypingUser {
	conversationId: string
	userId: string
	username: string
}
