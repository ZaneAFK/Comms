import { mount } from '@vue/test-utils'
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import type { Mock } from 'vitest'
import ChatArea from '@/components/messages/ChatArea.vue'
import type { ConversationDto, MessageDto } from '@/types'

type MockChatStore = {
	activeConversation: ConversationDto | null
	activeMessages: MessageDto[]
	activeConversationId: string | null
	getTypingUsers: Mock
	sendMessage: Mock
	startTyping: Mock
	stopTyping: Mock
	loadConversations: Mock
	connect: Mock
}

let mockChatStore: MockChatStore

vi.mock('@/stores/auth', () => ({
	useAuthStore: () => ({
		user: { username: 'alice', email: 'alice@example.com' },
		token: 'test-token',
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

function mountChatArea() {
	return mount(ChatArea)
}

beforeEach(() => {
	mockChatStore = {
		activeConversation: makeConversation(),
		activeMessages: [],
		activeConversationId: 'conv-1',
		getTypingUsers: vi.fn().mockReturnValue([]),
		sendMessage: vi.fn().mockResolvedValue(undefined),
		startTyping: vi.fn().mockResolvedValue(undefined),
		stopTyping: vi.fn().mockResolvedValue(undefined),
		loadConversations: vi.fn().mockResolvedValue(undefined),
		connect: vi.fn().mockResolvedValue(undefined),
	}
})

afterEach(() => {
	vi.useRealTimers()
})

describe('ChatArea', () => {
	describe('chat header', () => {
		beforeEach(() => {
			mockChatStore.activeConversation = makeConversation()
			mockChatStore.activeConversationId = 'conv-1'
		})

		it('displays the conversation name', () => {
			const wrapper = mountChatArea()
			expect(wrapper.find('.chat-title').text()).toBe('General')
		})

		it('lists member usernames', () => {
			const wrapper = mountChatArea()
			const members = wrapper.find('.chat-members').text()
			expect(members).toContain('alice')
			expect(members).toContain('bob')
		})
	})

	describe('message bubbles', () => {
		beforeEach(() => {
			mockChatStore.activeConversation = makeConversation()
			mockChatStore.activeConversationId = 'conv-1'
		})

		it('applies message-mine to messages sent by the current user', () => {
			mockChatStore.activeMessages = [makeMessage({ senderUsername: 'alice' })]
			const wrapper = mountChatArea()
			expect(wrapper.find('.message-bubble').classes()).toContain('message-mine')
		})

		it('applies message-theirs to messages from other users', () => {
			mockChatStore.activeMessages = [makeMessage({ senderUsername: 'bob' })]
			const wrapper = mountChatArea()
			expect(wrapper.find('.message-bubble').classes()).toContain('message-theirs')
		})

		it('renders message content and sender', () => {
			mockChatStore.activeMessages = [makeMessage({ senderUsername: 'bob', content: 'Hey there!' })]
			const wrapper = mountChatArea()
			expect(wrapper.find('.message-content').text()).toBe('Hey there!')
			expect(wrapper.find('.message-sender').text()).toBe('bob')
		})

		it('renders the message timestamp in HH:MM format', () => {
			mockChatStore.activeMessages = [makeMessage()]
			const wrapper = mountChatArea()
			expect(wrapper.find('.message-time').text()).toMatch(/^\d{1,2}:\d{2}/)
		})
	})

	describe('typing indicator', () => {
		beforeEach(() => {
			mockChatStore.activeConversation = makeConversation()
			mockChatStore.activeConversationId = 'conv-1'
		})

		it('is hidden when nobody is typing', () => {
			const wrapper = mountChatArea()
			expect(wrapper.find('.typing-indicator').exists()).toBe(false)
		})

		it('shows singular form when one user is typing', () => {
			mockChatStore.getTypingUsers = vi.fn().mockReturnValue([
				{ conversationId: 'conv-1', userId: 'user-2', username: 'bob' },
			])
			const wrapper = mountChatArea()
			expect(wrapper.find('.typing-indicator').text()).toBe('bob is typing…')
		})

		it('shows plural form when multiple users are typing', () => {
			mockChatStore.getTypingUsers = vi.fn().mockReturnValue([
				{ conversationId: 'conv-1', userId: 'user-2', username: 'bob' },
				{ conversationId: 'conv-1', userId: 'user-3', username: 'carol' },
			])
			const wrapper = mountChatArea()
			expect(wrapper.find('.typing-indicator').text()).toBe('bob, carol are typing…')
		})
	})

	describe('message input', () => {
		beforeEach(() => {
			mockChatStore.activeConversation = makeConversation()
			mockChatStore.activeConversationId = 'conv-1'
		})

		it('Send button is disabled when the input is empty', () => {
			const wrapper = mountChatArea()
			expect(wrapper.find('.send-btn').attributes('disabled')).toBeDefined()
		})

		it('Send button is enabled when the input has content', async () => {
			const wrapper = mountChatArea()
			await wrapper.find('.message-input').setValue('Hello')
			expect(wrapper.find('.send-btn').attributes('disabled')).toBeUndefined()
		})

		it('clears the input after sending', async () => {
			const wrapper = mountChatArea()
			const input = wrapper.find('.message-input')
			await input.setValue('Hello')
			await wrapper.find('.message-input-bar').trigger('submit')
			expect((input.element as HTMLInputElement).value).toBe('')
		})

		it('calls sendMessage with the correct arguments', async () => {
			const wrapper = mountChatArea()
			await wrapper.find('.message-input').setValue('Hello')
			await wrapper.find('.message-input-bar').trigger('submit')
			expect(mockChatStore.sendMessage).toHaveBeenCalledWith('conv-1', 'Hello')
		})

		it('does not send when the input is only whitespace', async () => {
			const wrapper = mountChatArea()
			await wrapper.find('.message-input').setValue('   ')
			await wrapper.find('.message-input-bar').trigger('submit')
			expect(mockChatStore.sendMessage).not.toHaveBeenCalled()
		})
	})
})
