import { mount } from '@vue/test-utils'
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import type { Mock } from 'vitest'
import MessagesView from '@/views/MessagesView.vue'
import type { ConversationDto, MessageDto, UserSearchResult } from '@/types'

type MockChatStore = {
	conversations: ConversationDto[]
	activeConversationId: string | null
	connect: Mock
	loadConversations: Mock
	loadMessages: Mock
	sendMessage: Mock
	startTyping: Mock
	stopTyping: Mock
	createConversation: Mock
	searchUsers: Mock
	getMessages: Mock
	getTypingUsers: Mock
	setActiveConversation: Mock
}

let mockChatStore: MockChatStore

vi.mock('@/stores/auth', () => ({
	useAuthStore: () => ({
		user: { username: 'alice', email: 'alice@example.com' },
		token: 'test-token',
		isAuthenticated: true,
	}),
}))

vi.mock('@/stores/chat', () => ({
	useChatStore: () => mockChatStore,
}))

const makeConversation = (overrides: Partial<ConversationDto> = {}): ConversationDto => ({
	id: 'conv-1',
	name: 'General',
	createdAt: '2024-01-01T00:00:00Z',
	members: [
		{ userId: 'user-1', username: 'alice' },
		{ userId: 'user-2', username: 'bob' },
	],
	lastMessage: null,
	...overrides,
})

const makeMessage = (overrides: Partial<MessageDto> = {}): MessageDto => ({
	id: 'msg-1',
	conversationId: 'conv-1',
	senderId: 'user-1',
	senderUsername: 'alice',
	content: 'Hello!',
	sentAt: new Date('2024-06-15T14:30:00Z').toISOString(),
	...overrides,
})

function mountView() {
	return mount(MessagesView, {
		global: { stubs: { ToolBar: true } },
	})
}

beforeEach(() => {
	mockChatStore = {
		conversations: [],
		activeConversationId: null,
		connect: vi.fn().mockResolvedValue(undefined),
		loadConversations: vi.fn().mockResolvedValue(undefined),
		loadMessages: vi.fn().mockResolvedValue(undefined),
		sendMessage: vi.fn().mockResolvedValue(undefined),
		startTyping: vi.fn().mockResolvedValue(undefined),
		stopTyping: vi.fn().mockResolvedValue(undefined),
		createConversation: vi.fn().mockResolvedValue(null),
		searchUsers: vi.fn().mockResolvedValue([]),
		getMessages: vi.fn().mockReturnValue([]),
		getTypingUsers: vi.fn().mockReturnValue([]),
		setActiveConversation: vi.fn((id: string | null) => {
			mockChatStore.activeConversationId = id
		}),
	}
})

afterEach(() => {
	vi.useRealTimers()
})

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('MessagesView', () => {
	describe('sidebar rendering', () => {
		it('shows the empty state when there are no conversations', () => {
			const wrapper = mountView()
			expect(wrapper.find('.no-convs').text()).toBe('No conversations yet.')
		})

		it('renders a button for each conversation', () => {
			mockChatStore.conversations = [
				makeConversation({ id: 'conv-1', name: 'General' }),
				makeConversation({ id: 'conv-2', name: 'Random' }),
			]
			const wrapper = mountView()
			const items = wrapper.findAll('.conversation-item')
			expect(items).toHaveLength(2)
			expect(items[0]!.text()).toContain('General')
			expect(items[1]!.text()).toContain('Random')
		})

		it('shows the last message preview', () => {
			mockChatStore.conversations = [
				makeConversation({ lastMessage: makeMessage({ content: 'Latest' }) }),
			]
			const wrapper = mountView()
			expect(wrapper.find('.conv-last-msg').text()).toBe('Latest')
		})

		it('shows "No messages yet" when the conversation has no last message', () => {
			mockChatStore.conversations = [makeConversation({ lastMessage: null })]
			const wrapper = mountView()
			expect(wrapper.find('.conv-last-msg').text()).toBe('No messages yet')
		})

		it('marks the active conversation with the active class', () => {
			mockChatStore.conversations = [makeConversation({ id: 'conv-1' })]
			mockChatStore.activeConversationId = 'conv-1'
			const wrapper = mountView()
			expect(wrapper.find('.conversation-item').classes()).toContain('active')
		})
	})

	describe('chat area', () => {
		it('shows the empty state when no conversation is selected', () => {
			const wrapper = mountView()
			expect(wrapper.find('.chat-empty').exists()).toBe(true)
		})

		it('shows the chat area when an active conversation is selected', () => {
			mockChatStore.conversations = [makeConversation()]
			mockChatStore.activeConversationId = 'conv-1'
			const wrapper = mountView()
			expect(wrapper.find('.chat-header').exists()).toBe(true)
			expect(wrapper.find('.chat-empty').exists()).toBe(false)
		})

		it('displays the conversation name in the header', () => {
			mockChatStore.conversations = [makeConversation({ name: 'General' })]
			mockChatStore.activeConversationId = 'conv-1'
			const wrapper = mountView()
			expect(wrapper.find('.chat-title').text()).toBe('General')
		})

		it('lists member usernames in the header', () => {
			mockChatStore.conversations = [makeConversation()]
			mockChatStore.activeConversationId = 'conv-1'
			const wrapper = mountView()
			const members = wrapper.find('.chat-members').text()
			expect(members).toContain('alice')
			expect(members).toContain('bob')
		})
	})

	describe('message bubbles', () => {
		beforeEach(() => {
			mockChatStore.conversations = [makeConversation()]
			mockChatStore.activeConversationId = 'conv-1'
		})

		it('applies message-mine to messages sent by the current user', () => {
			mockChatStore.getMessages = vi.fn().mockReturnValue([makeMessage({ senderUsername: 'alice' })])
			const wrapper = mountView()
			expect(wrapper.find('.message-bubble').classes()).toContain('message-mine')
		})

		it('applies message-theirs to messages from other users', () => {
			mockChatStore.getMessages = vi.fn().mockReturnValue([makeMessage({ senderUsername: 'bob' })])
			const wrapper = mountView()
			expect(wrapper.find('.message-bubble').classes()).toContain('message-theirs')
		})

		it('renders message content and sender', () => {
			mockChatStore.getMessages = vi.fn().mockReturnValue([makeMessage({ senderUsername: 'bob', content: 'Hey there!' })])
			const wrapper = mountView()
			expect(wrapper.find('.message-content').text()).toBe('Hey there!')
			expect(wrapper.find('.message-sender').text()).toBe('bob')
		})

		it('renders the message timestamp in HH:MM format', () => {
			mockChatStore.getMessages = vi.fn().mockReturnValue([makeMessage()])
			const wrapper = mountView()
			expect(wrapper.find('.message-time').text()).toMatch(/^\d{1,2}:\d{2}/)
		})
	})

	describe('typing indicator', () => {
		beforeEach(() => {
			mockChatStore.conversations = [makeConversation()]
			mockChatStore.activeConversationId = 'conv-1'
			mockChatStore.getMessages = vi.fn().mockReturnValue([])
		})

		it('is hidden when nobody is typing', () => {
			mockChatStore.getTypingUsers = vi.fn().mockReturnValue([])
			const wrapper = mountView()
			expect(wrapper.find('.typing-indicator').exists()).toBe(false)
		})

		it('shows singular form when one user is typing', () => {
			mockChatStore.getTypingUsers = vi.fn().mockReturnValue([
				{ conversationId: 'conv-1', userId: 'user-2', username: 'bob' },
			])
			const wrapper = mountView()
			expect(wrapper.find('.typing-indicator').text()).toBe('bob is typing…')
		})

		it('shows plural form when multiple users are typing', () => {
			mockChatStore.getTypingUsers = vi.fn().mockReturnValue([
				{ conversationId: 'conv-1', userId: 'user-2', username: 'bob' },
				{ conversationId: 'conv-1', userId: 'user-3', username: 'carol' },
			])
			const wrapper = mountView()
			expect(wrapper.find('.typing-indicator').text()).toBe('bob, carol are typing…')
		})
	})

	describe('new conversation modal', () => {
		it('is hidden by default', () => {
			const wrapper = mountView()
			expect(wrapper.find('.modal').exists()).toBe(false)
		})

		it('opens when the + button is clicked', async () => {
			const wrapper = mountView()
			await wrapper.find('.new-conv-btn').trigger('click')
			expect(wrapper.find('.modal').exists()).toBe(true)
		})

		it('closes when Cancel is clicked', async () => {
			const wrapper = mountView()
			await wrapper.find('.new-conv-btn').trigger('click')
			await wrapper.find('.btn-outline').trigger('click')
			expect(wrapper.find('.modal').exists()).toBe(false)
		})

		it('keeps the Create button disabled when the conversation name is empty', async () => {
			const wrapper = mountView()
			await wrapper.find('.new-conv-btn').trigger('click')
			expect(wrapper.find('.modal-actions .btn-primary').attributes('disabled')).toBeDefined()
		})

		it('enables the Create button once a name is entered', async () => {
			const wrapper = mountView()
			await wrapper.find('.new-conv-btn').trigger('click')
			await wrapper.find('input[placeholder="Conversation name"]').setValue('Test Chat')
			expect(wrapper.find('.modal-actions .btn-primary').attributes('disabled')).toBeUndefined()
		})

		it('calls createConversation with the name and selected member IDs', async () => {
			const conv = makeConversation()
			mockChatStore.createConversation = vi.fn().mockResolvedValue(conv)
			mockChatStore.loadMessages = vi.fn().mockResolvedValue(undefined)

			const wrapper = mountView()
			await wrapper.find('.new-conv-btn').trigger('click')
			await wrapper.find('input[placeholder="Conversation name"]').setValue('Test Chat')
			await wrapper.find('.modal-actions .btn-primary').trigger('click')
			expect(mockChatStore.createConversation).toHaveBeenCalledWith('Test Chat', [], 'test-token')
		})

		it('closes the modal after a conversation is successfully created', async () => {
			const conv = makeConversation()
			mockChatStore.createConversation = vi.fn().mockResolvedValue(conv)
			mockChatStore.loadMessages = vi.fn().mockResolvedValue(undefined)

			const wrapper = mountView()
			await wrapper.find('.new-conv-btn').trigger('click')
			await wrapper.find('input[placeholder="Conversation name"]').setValue('Test Chat')
			await wrapper.find('.modal-actions .btn-primary').trigger('click')
			await wrapper.vm.$nextTick()
			expect(wrapper.find('.modal').exists()).toBe(false)
		})

		it('adds a user to the selected members list via the search results', async () => {
			vi.useFakeTimers()
			const searchResult: UserSearchResult = { id: 'user-2', username: 'bob' }
			mockChatStore.searchUsers = vi.fn().mockResolvedValue([searchResult])

			const wrapper = mountView()
			await wrapper.find('.new-conv-btn').trigger('click')
			await wrapper.find('input[placeholder="Search users…"]').setValue('bob')
			await wrapper.find('input[placeholder="Search users…"]').trigger('input')

			// Advance past the 300ms debounce
			await vi.advanceTimersByTimeAsync(300)
			await wrapper.vm.$nextTick()

			await wrapper.find('.user-result-item').trigger('click')
			expect(wrapper.find('.member-tag').text()).toContain('bob')
		})

		it('does not add the same user to selected members twice', async () => {
			vi.useFakeTimers()
			const searchResult: UserSearchResult = { id: 'user-2', username: 'bob' }
			mockChatStore.searchUsers = vi.fn().mockResolvedValue([searchResult])

			const wrapper = mountView()
			await wrapper.find('.new-conv-btn').trigger('click')

			// Add bob once
			await wrapper.find('input[placeholder="Search users…"]').setValue('bob')
			await wrapper.find('input[placeholder="Search users…"]').trigger('input')
			await vi.advanceTimersByTimeAsync(300)
			await wrapper.vm.$nextTick()
			await wrapper.find('.user-result-item').trigger('click')

			// Search again and attempt to add bob a second time
			await wrapper.find('input[placeholder="Search users…"]').setValue('bob')
			await wrapper.find('input[placeholder="Search users…"]').trigger('input')
			await vi.advanceTimersByTimeAsync(300)
			await wrapper.vm.$nextTick()
			await wrapper.find('.user-result-item').trigger('click')

			expect(wrapper.findAll('.member-tag')).toHaveLength(1)
		})

		it('removes a member when the × button is clicked', async () => {
			vi.useFakeTimers()
			const searchResult: UserSearchResult = { id: 'user-2', username: 'bob' }
			mockChatStore.searchUsers = vi.fn().mockResolvedValue([searchResult])

			const wrapper = mountView()
			await wrapper.find('.new-conv-btn').trigger('click')
			await wrapper.find('input[placeholder="Search users…"]').setValue('bob')
			await wrapper.find('input[placeholder="Search users…"]').trigger('input')
			await vi.advanceTimersByTimeAsync(300)
			await wrapper.vm.$nextTick()
			await wrapper.find('.user-result-item').trigger('click')

			expect(wrapper.findAll('.member-tag')).toHaveLength(1)
			await wrapper.find('.member-tag button').trigger('click')
			expect(wrapper.findAll('.member-tag')).toHaveLength(0)
		})
	})

	describe('message input', () => {
		beforeEach(() => {
			mockChatStore.conversations = [makeConversation()]
			mockChatStore.activeConversationId = 'conv-1'
			mockChatStore.getMessages = vi.fn().mockReturnValue([])
			mockChatStore.getTypingUsers = vi.fn().mockReturnValue([])
		})

		it('Send button is disabled when the input is empty', () => {
			const wrapper = mountView()
			expect(wrapper.find('.send-btn').attributes('disabled')).toBeDefined()
		})

		it('Send button is enabled when the input has content', async () => {
			const wrapper = mountView()
			await wrapper.find('.message-input').setValue('Hello')
			expect(wrapper.find('.send-btn').attributes('disabled')).toBeUndefined()
		})

		it('clears the input after sending', async () => {
			const wrapper = mountView()
			const input = wrapper.find('.message-input')
			await input.setValue('Hello')
			await wrapper.find('.message-input-bar').trigger('submit')
			expect((input.element as HTMLInputElement).value).toBe('')
		})

		it('calls sendMessage on the store with the correct arguments', async () => {
			const wrapper = mountView()
			await wrapper.find('.message-input').setValue('Hello')
			await wrapper.find('.message-input-bar').trigger('submit')
			expect(mockChatStore.sendMessage).toHaveBeenCalledWith('conv-1', 'Hello')
		})

		it('does not send when the input is only whitespace', async () => {
			const wrapper = mountView()
			await wrapper.find('.message-input').setValue('   ')
			await wrapper.find('.message-input-bar').trigger('submit')
			expect(mockChatStore.sendMessage).not.toHaveBeenCalled()
		})
	})

	describe('onMounted', () => {
		it('loads conversations and connects to the hub', async () => {
			mountView()
			await Promise.resolve()
			expect(mockChatStore.loadConversations).toHaveBeenCalledWith('test-token')
			expect(mockChatStore.connect).toHaveBeenCalledWith('test-token')
		})
	})
})
