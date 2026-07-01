import { mount } from '@vue/test-utils'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import type { Mock } from 'vitest'
import SideBar from '@/components/messages/SideBar.vue'
import type { ConversationDto, MessageDto } from '@/types'

type MockChatStore = {
	conversations: ConversationDto[]
	activeConversationId: string | null
	selectConversation: Mock
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
	members: [],
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

function mountSideBar() {
	return mount(SideBar, {
		global: { stubs: { teleport: true } },
	})
}

beforeEach(() => {
	mockChatStore = {
		conversations: [],
		activeConversationId: null,
		selectConversation: vi.fn().mockResolvedValue(undefined),
	}
})

describe('SideBar', () => {
	describe('conversation list', () => {
		it('shows the empty state when there are no conversations', () => {
			const wrapper = mountSideBar()
			expect(wrapper.find('.no-convs').text()).toBe('No conversations yet.')
		})

		it('renders a button for each conversation', () => {
			mockChatStore.conversations = [
				makeConversation({ id: 'conv-1', name: 'General' }),
				makeConversation({ id: 'conv-2', name: 'Random' }),
			]
			const wrapper = mountSideBar()
			const items = wrapper.findAll('.conversation-item')
			expect(items).toHaveLength(2)
			expect(items[0]!.text()).toContain('General')
			expect(items[1]!.text()).toContain('Random')
		})

		it('shows the last message preview', () => {
			mockChatStore.conversations = [
				makeConversation({ lastMessage: makeMessage({ content: 'Latest' }) }),
			]
			const wrapper = mountSideBar()
			expect(wrapper.find('.conv-last-msg').text()).toBe('Latest')
		})

		it('shows "No messages yet" when the conversation has no last message', () => {
			mockChatStore.conversations = [makeConversation({ lastMessage: null })]
			const wrapper = mountSideBar()
			expect(wrapper.find('.conv-last-msg').text()).toBe('No messages yet')
		})

		it('marks the active conversation with the active class', () => {
			mockChatStore.conversations = [makeConversation({ id: 'conv-1' })]
			mockChatStore.activeConversationId = 'conv-1'
			const wrapper = mountSideBar()
			expect(wrapper.find('.conversation-item').classes()).toContain('active')
		})

		it('calls selectConversation when a conversation is clicked', async () => {
			mockChatStore.conversations = [makeConversation({ id: 'conv-1' })]
			const wrapper = mountSideBar()
			await wrapper.find('.conversation-item').trigger('click')
			expect(mockChatStore.selectConversation).toHaveBeenCalledWith('conv-1', 'test-token')
		})
	})

})
