import { mount } from '@vue/test-utils'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import type { Mock } from 'vitest'
import MessagesView from '@/views/MessagesView.vue'

type MockChatStore = {
	connect: Mock
	loadConversations: Mock
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

beforeEach(() => {
	mockChatStore = {
		connect: vi.fn().mockResolvedValue(undefined),
		loadConversations: vi.fn().mockResolvedValue(undefined),
	}
})

describe('MessagesView', () => {
	describe('onMounted', () => {
		it('loads conversations and connects to the hub', async () => {
			mount(MessagesView, {
				global: { stubs: { ToolBar: true, SideBar: true, ChatArea: true } },
			})
			await Promise.resolve()
			expect(mockChatStore.loadConversations).toHaveBeenCalledWith('test-token')
			expect(mockChatStore.connect).toHaveBeenCalledWith('test-token')
		})
	})
})
