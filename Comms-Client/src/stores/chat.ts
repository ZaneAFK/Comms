import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as signalR from '@microsoft/signalr'
import type { ConversationDto, MessageDto, UserSearchResult, TypingUser } from '@/types'

export const useChatStore = defineStore('chat', () => {
	const connection = ref<signalR.HubConnection | null>(null)
	const conversations = ref<ConversationDto[]>([])
	const messages = ref<Map<string, MessageDto[]>>(new Map())
	const typingUsers = ref<Map<string, TypingUser[]>>(new Map())
	const activeConversationId = ref<string | null>(null)

	async function connect(token: string) {
		if (connection.value) return

		const hub = new signalR.HubConnectionBuilder()
			.withUrl('/hubs/chat', { accessTokenFactory: () => token })
			.withAutomaticReconnect()
			.build()

		hub.on('ReceiveMessage', (message: MessageDto) => {
			const list = messages.value.get(message.conversationId) ?? []
			messages.value.set(message.conversationId, [...list, message])

			const conv = conversations.value.find(c => c.id === message.conversationId)
			if (conv) conv.lastMessage = message
		})

		hub.on('UserTyping', (data: TypingUser) => {
			const list = typingUsers.value.get(data.conversationId) ?? []
			if (!list.find(u => u.userId === data.userId)) {
				typingUsers.value.set(data.conversationId, [...list, data])
			}
		})

		hub.on('UserStoppedTyping', (data: { conversationId: string; userId: string }) => {
			const list = typingUsers.value.get(data.conversationId) ?? []
			typingUsers.value.set(data.conversationId, list.filter(u => u.userId !== data.userId))
		})

		await hub.start()
		connection.value = hub
	}

	async function disconnect() {
		if (connection.value) {
			await connection.value.stop()
			connection.value = null
		}
	}

	async function loadConversations(token: string) {
		const res = await fetch('/api/conversations', {
			headers: { Authorization: `Bearer ${token}` }
		})
		if (res.ok) {
			conversations.value = await res.json()
		}
	}

	async function loadMessages(conversationId: string, token: string) {
		const res = await fetch(`/api/conversations/${conversationId}/messages`, {
			headers: { Authorization: `Bearer ${token}` }
		})
		if (res.ok) {
			const data: MessageDto[] = await res.json()
			messages.value.set(conversationId, data)
		}
	}

	async function sendMessage(conversationId: string, content: string) {
		if (!connection.value) return
		await connection.value.invoke('SendMessage', conversationId, content)
	}

	async function startTyping(conversationId: string) {
		if (!connection.value) return
		await connection.value.invoke('StartTyping', conversationId)
	}

	async function stopTyping(conversationId: string) {
		if (!connection.value) return
		await connection.value.invoke('StopTyping', conversationId)
	}

	async function createConversation(name: string, memberIds: string[], token: string): Promise<ConversationDto | null> {
		const res = await fetch('/api/conversations', {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			},
			body: JSON.stringify({ name, memberIds })
		})
		if (!res.ok) return null
		const conv: ConversationDto = await res.json()
		conversations.value.unshift(conv)
		if (connection.value) {
			await connection.value.invoke('JoinConversation', conv.id)
		}
		return conv
	}

	async function searchUsers(username: string, token: string): Promise<UserSearchResult[]> {
		const res = await fetch(`/api/users/search?username=${encodeURIComponent(username)}`, {
			headers: { Authorization: `Bearer ${token}` }
		})
		if (!res.ok) return []
		return res.json()
	}

	function getMessages(conversationId: string): MessageDto[] {
		return messages.value.get(conversationId) ?? []
	}

	function getTypingUsers(conversationId: string): TypingUser[] {
		return typingUsers.value.get(conversationId) ?? []
	}

	function setActiveConversation(id: string | null) {
		activeConversationId.value = id
	}

	return {
		connection,
		conversations,
		activeConversationId,
		connect,
		disconnect,
		loadConversations,
		loadMessages,
		sendMessage,
		startTyping,
		stopTyping,
		createConversation,
		searchUsers,
		getMessages,
		getTypingUsers,
		setActiveConversation
	}
})
