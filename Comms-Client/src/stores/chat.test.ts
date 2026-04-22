import { setActivePinia, createPinia } from 'pinia'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useChatStore } from '@/stores/chat'
import type { ConversationDto, MessageDto } from '@/types'

// ---------------------------------------------------------------------------
// SignalR mock
// HubConnectionBuilder must be a real class — arrow functions are not
// constructable with `new`, and vi.clearAllMocks() would strip the
// vi.fn() wrapper leaving only the bare arrow function.
// ---------------------------------------------------------------------------

interface HubHandlers {
	ReceiveMessage?: (message: MessageDto) => void
	UserTyping?: (data: { conversationId: string; userId: string; username: string }) => void
	UserStoppedTyping?: (data: { conversationId: string; userId: string }) => void
}

const hubHandlers: HubHandlers = {}

const mockHub = {
	on: vi.fn(),
	start: vi.fn(),
	stop: vi.fn(),
	invoke: vi.fn(),
}

vi.mock('@microsoft/signalr', () => ({
	HubConnectionBuilder: class {
		withUrl() { return this }
		withAutomaticReconnect() { return this }
		build() { return mockHub }
	},
}))

const TOKEN = 'test-token'

const makeConversation = (overrides: Partial<ConversationDto> = {}): ConversationDto => ({
	id: 'conv-1',
	name: 'Test Chat',
	createdAt: '2024-01-01T00:00:00Z',
	members: [],
	lastMessage: null,
	...overrides,
})

const makeMessage = (overrides: Partial<MessageDto> = {}): MessageDto => ({
	id: 'msg-1',
	conversationId: 'conv-1',
	senderId: 'user-1',
	senderUsername: 'alice',
	content: 'Hello',
	sentAt: '2024-01-01T00:00:00Z',
	...overrides,
})

beforeEach(() => {
	setActivePinia(createPinia())
	vi.clearAllMocks()

	// Re-establish mockHub behaviour after clearAllMocks wipes call history
	mockHub.start.mockResolvedValue(undefined)
	mockHub.stop.mockResolvedValue(undefined)
	mockHub.invoke.mockResolvedValue(undefined)
	mockHub.on.mockImplementation((event: string, handler: unknown) => {
		;(hubHandlers as Record<string, unknown>)[event] = handler
	})

	delete hubHandlers.ReceiveMessage
	delete hubHandlers.UserTyping
	delete hubHandlers.UserStoppedTyping
})

describe('Chat Store', () => {
	describe('initial state', () => {
		it('has no connection, empty conversations, and no active conversation', () => {
			const store = useChatStore()
			expect(store.connection).toBeNull()
			expect(store.conversations).toEqual([])
			expect(store.activeConversationId).toBeNull()
		})
	})

	describe('connect', () => {
		it('builds and starts a SignalR hub connection', async () => {
			const store = useChatStore()
			await store.connect(TOKEN)
			expect(mockHub.start).toHaveBeenCalledOnce()
			expect(store.connection).not.toBeNull()
		})

		it('registers ReceiveMessage, UserTyping, and UserStoppedTyping handlers', async () => {
			const store = useChatStore()
			await store.connect(TOKEN)
			expect(mockHub.on).toHaveBeenCalledWith('ReceiveMessage', expect.any(Function))
			expect(mockHub.on).toHaveBeenCalledWith('UserTyping', expect.any(Function))
			expect(mockHub.on).toHaveBeenCalledWith('UserStoppedTyping', expect.any(Function))
		})

		it('does not reconnect if already connected', async () => {
			const store = useChatStore()
			await store.connect(TOKEN)
			await store.connect(TOKEN)
			expect(mockHub.start).toHaveBeenCalledOnce()
		})
	})

	describe('disconnect', () => {
		it('stops the connection and clears it', async () => {
			const store = useChatStore()
			await store.connect(TOKEN)
			await store.disconnect()
			expect(mockHub.stop).toHaveBeenCalledOnce()
			expect(store.connection).toBeNull()
		})

		it('does nothing when not connected', async () => {
			const store = useChatStore()
			await store.disconnect()
			expect(mockHub.stop).not.toHaveBeenCalled()
		})
	})

	describe('loadConversations', () => {
		it('populates conversations on success', async () => {
			const store = useChatStore()
			const convs = [makeConversation()]
			vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
				ok: true,
				json: () => Promise.resolve(convs),
			}))
			await store.loadConversations(TOKEN)
			expect(store.conversations).toEqual(convs)
		})

		it('leaves conversations unchanged on a failed response', async () => {
			const store = useChatStore()
			vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false }))
			await store.loadConversations(TOKEN)
			expect(store.conversations).toEqual([])
		})
	})

	describe('loadMessages', () => {
		it('stores messages for the conversation on success', async () => {
			const store = useChatStore()
			const msgs = [makeMessage()]
			vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
				ok: true,
				json: () => Promise.resolve(msgs),
			}))
			await store.loadMessages('conv-1', TOKEN)
			expect(store.getMessages('conv-1')).toEqual(msgs)
		})

		it('leaves messages unchanged on a failed response', async () => {
			const store = useChatStore()
			vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false }))
			await store.loadMessages('conv-1', TOKEN)
			expect(store.getMessages('conv-1')).toEqual([])
		})
	})

	describe('sendMessage', () => {
		it('invokes SendMessage on the hub with correct arguments', async () => {
			const store = useChatStore()
			await store.connect(TOKEN)
			await store.sendMessage('conv-1', 'Hello')
			expect(mockHub.invoke).toHaveBeenCalledWith('SendMessage', 'conv-1', 'Hello')
		})

		it('does nothing when not connected', async () => {
			const store = useChatStore()
			await store.sendMessage('conv-1', 'Hello')
			expect(mockHub.invoke).not.toHaveBeenCalled()
		})
	})

	describe('startTyping', () => {
		it('invokes StartTyping on the hub', async () => {
			const store = useChatStore()
			await store.connect(TOKEN)
			await store.startTyping('conv-1')
			expect(mockHub.invoke).toHaveBeenCalledWith('StartTyping', 'conv-1')
		})

		it('does nothing when not connected', async () => {
			const store = useChatStore()
			await store.startTyping('conv-1')
			expect(mockHub.invoke).not.toHaveBeenCalled()
		})
	})

	describe('stopTyping', () => {
		it('invokes StopTyping on the hub', async () => {
			const store = useChatStore()
			await store.connect(TOKEN)
			await store.stopTyping('conv-1')
			expect(mockHub.invoke).toHaveBeenCalledWith('StopTyping', 'conv-1')
		})

		it('does nothing when not connected', async () => {
			const store = useChatStore()
			await store.stopTyping('conv-1')
			expect(mockHub.invoke).not.toHaveBeenCalled()
		})
	})

	describe('createConversation', () => {
		it('posts to the API, prepends to conversations, and joins the hub group', async () => {
			const store = useChatStore()
			await store.connect(TOKEN)
			const conv = makeConversation()
			vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
				ok: true,
				json: () => Promise.resolve(conv),
			}))
			const result = await store.createConversation('Test Chat', ['user-2'], TOKEN)
			expect(result).toEqual(conv)
			expect(store.conversations[0]).toEqual(conv)
			expect(mockHub.invoke).toHaveBeenCalledWith('JoinConversation', conv.id)
		})

		it('returns null and does not modify conversations on failure', async () => {
			const store = useChatStore()
			vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false }))
			const result = await store.createConversation('Test Chat', [], TOKEN)
			expect(result).toBeNull()
			expect(store.conversations).toEqual([])
		})
	})

	describe('searchUsers', () => {
		it('returns matching users on success', async () => {
			const store = useChatStore()
			const results = [{ id: 'user-2', username: 'bob' }]
			vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
				ok: true,
				json: () => Promise.resolve(results),
			}))
			const found = await store.searchUsers('bob', TOKEN)
			expect(found).toEqual(results)
		})

		it('returns an empty array on failure', async () => {
			const store = useChatStore()
			vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false }))
			const found = await store.searchUsers('bob', TOKEN)
			expect(found).toEqual([])
		})
	})

	describe('getMessages', () => {
		it('returns an empty array for an unknown conversation', () => {
			const store = useChatStore()
			expect(store.getMessages('unknown')).toEqual([])
		})
	})

	describe('getTypingUsers', () => {
		it('returns an empty array for an unknown conversation', () => {
			const store = useChatStore()
			expect(store.getTypingUsers('unknown')).toEqual([])
		})
	})

	describe('setActiveConversation', () => {
		it('sets the active conversation ID', () => {
			const store = useChatStore()
			store.setActiveConversation('conv-1')
			expect(store.activeConversationId).toBe('conv-1')
		})

		it('clears the active conversation when passed null', () => {
			const store = useChatStore()
			store.setActiveConversation('conv-1')
			store.setActiveConversation(null)
			expect(store.activeConversationId).toBeNull()
		})
	})

	describe('ReceiveMessage handler', () => {
		it('appends the incoming message to the correct conversation', async () => {
			const store = useChatStore()
			await store.connect(TOKEN)
			const msg = makeMessage()
			hubHandlers.ReceiveMessage!(msg)
			expect(store.getMessages('conv-1')).toContainEqual(msg)
		})

		it('appends to existing messages rather than replacing them', async () => {
			const store = useChatStore()
			vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
				ok: true,
				json: () => Promise.resolve([makeMessage({ id: 'msg-1' })]),
			}))
			await store.loadMessages('conv-1', TOKEN)
			await store.connect(TOKEN)
			hubHandlers.ReceiveMessage!(makeMessage({ id: 'msg-2', content: 'World' }))
			expect(store.getMessages('conv-1')).toHaveLength(2)
		})

		it('updates lastMessage on the matching conversation', async () => {
			const store = useChatStore()
			store.conversations.push(makeConversation())
			await store.connect(TOKEN)
			const msg = makeMessage()
			hubHandlers.ReceiveMessage!(msg)
			expect(store.conversations[0]!.lastMessage).toEqual(msg)
		})
	})

	describe('UserTyping handler', () => {
		it('adds a typing user for the conversation', async () => {
			const store = useChatStore()
			await store.connect(TOKEN)
			hubHandlers.UserTyping!({ conversationId: 'conv-1', userId: 'user-2', username: 'bob' })
			expect(store.getTypingUsers('conv-1')).toHaveLength(1)
			expect(store.getTypingUsers('conv-1')[0]!.username).toBe('bob')
		})

		it('does not add the same user twice', async () => {
			const store = useChatStore()
			await store.connect(TOKEN)
			const event = { conversationId: 'conv-1', userId: 'user-2', username: 'bob' }
			hubHandlers.UserTyping!(event)
			hubHandlers.UserTyping!(event)
			expect(store.getTypingUsers('conv-1')).toHaveLength(1)
		})

		it('tracks typing users across multiple conversations independently', async () => {
			const store = useChatStore()
			await store.connect(TOKEN)
			hubHandlers.UserTyping!({ conversationId: 'conv-1', userId: 'user-2', username: 'bob' })
			hubHandlers.UserTyping!({ conversationId: 'conv-2', userId: 'user-3', username: 'carol' })
			expect(store.getTypingUsers('conv-1')).toHaveLength(1)
			expect(store.getTypingUsers('conv-2')).toHaveLength(1)
		})
	})

	describe('UserStoppedTyping handler', () => {
		it('removes the typing user from the conversation', async () => {
			const store = useChatStore()
			await store.connect(TOKEN)
			hubHandlers.UserTyping!({ conversationId: 'conv-1', userId: 'user-2', username: 'bob' })
			hubHandlers.UserStoppedTyping!({ conversationId: 'conv-1', userId: 'user-2' })
			expect(store.getTypingUsers('conv-1')).toHaveLength(0)
		})

		it('only removes the specified user, leaving others intact', async () => {
			const store = useChatStore()
			await store.connect(TOKEN)
			hubHandlers.UserTyping!({ conversationId: 'conv-1', userId: 'user-2', username: 'bob' })
			hubHandlers.UserTyping!({ conversationId: 'conv-1', userId: 'user-3', username: 'carol' })
			hubHandlers.UserStoppedTyping!({ conversationId: 'conv-1', userId: 'user-2' })
			expect(store.getTypingUsers('conv-1')).toHaveLength(1)
			expect(store.getTypingUsers('conv-1')[0]!.username).toBe('carol')
		})
	})
})
