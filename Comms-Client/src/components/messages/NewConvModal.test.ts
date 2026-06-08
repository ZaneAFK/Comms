import { mount } from '@vue/test-utils'
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import type { Mock } from 'vitest'
import NewConvModal from '@/components/messages/NewConvModal.vue'
import type { ConversationDto, UserSearchResult } from '@/types'

type MockChatStore = {
	createConversation: Mock
	selectConversation: Mock
	searchUsers: Mock
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

function mountModal() {
	return mount(NewConvModal)
}

beforeEach(() => {
	mockChatStore = {
		createConversation: vi.fn().mockResolvedValue(null),
		selectConversation: vi.fn().mockResolvedValue(undefined),
		searchUsers: vi.fn().mockResolvedValue([]),
	}
})

afterEach(() => {
	vi.useRealTimers()
})

describe('NewConvModal', () => {
	it('renders the modal', () => {
		const wrapper = mountModal()
		expect(wrapper.find('.modal').exists()).toBe(true)
		expect(wrapper.find('h2').text()).toBe('New Conversation')
	})

	it('emits close when Cancel is clicked', async () => {
		const wrapper = mountModal()
		await wrapper.find('.btn-outline').trigger('click')
		expect(wrapper.emitted('close')).toBeTruthy()
	})

	it('keeps the Create button disabled when the conversation name is empty', () => {
		const wrapper = mountModal()
		expect(wrapper.find('.modal-actions .btn-primary').attributes('disabled')).toBeDefined()
	})

	it('enables the Create button once a name is entered', async () => {
		const wrapper = mountModal()
		await wrapper.find('input[placeholder="Conversation name"]').setValue('Test Chat')
		expect(wrapper.find('.modal-actions .btn-primary').attributes('disabled')).toBeUndefined()
	})

	it('calls createConversation with the name and selected member IDs', async () => {
		const conv = makeConversation()
		mockChatStore.createConversation = vi.fn().mockResolvedValue(conv)

		const wrapper = mountModal()
		await wrapper.find('input[placeholder="Conversation name"]').setValue('Test Chat')
		await wrapper.find('.modal-actions .btn-primary').trigger('click')
		expect(mockChatStore.createConversation).toHaveBeenCalledWith('Test Chat', [], 'test-token')
	})

	it('emits close after a conversation is successfully created', async () => {
		const conv = makeConversation()
		mockChatStore.createConversation = vi.fn().mockResolvedValue(conv)

		const wrapper = mountModal()
		await wrapper.find('input[placeholder="Conversation name"]').setValue('Test Chat')
		await wrapper.find('.modal-actions .btn-primary').trigger('click')
		await wrapper.vm.$nextTick()
		expect(wrapper.emitted('close')).toBeTruthy()
	})

	it('adds a user to the selected members list via search results', async () => {
		vi.useFakeTimers()
		const searchResult: UserSearchResult = { id: 'user-2', username: 'bob' }
		mockChatStore.searchUsers = vi.fn().mockResolvedValue([searchResult])

		const wrapper = mountModal()
		await wrapper.find('input[placeholder="Search users…"]').setValue('bob')
		await wrapper.find('input[placeholder="Search users…"]').trigger('input')
		await vi.advanceTimersByTimeAsync(300)
		await wrapper.vm.$nextTick()

		await wrapper.find('.user-result-item').trigger('click')
		expect(wrapper.find('.member-tag').text()).toContain('bob')
	})

	it('does not add the same user to selected members twice', async () => {
		vi.useFakeTimers()
		const searchResult: UserSearchResult = { id: 'user-2', username: 'bob' }
		mockChatStore.searchUsers = vi.fn().mockResolvedValue([searchResult])

		const wrapper = mountModal()

		for (let i = 0; i < 2; i++) {
			await wrapper.find('input[placeholder="Search users…"]').setValue('bob')
			await wrapper.find('input[placeholder="Search users…"]').trigger('input')
			await vi.advanceTimersByTimeAsync(300)
			await wrapper.vm.$nextTick()
			await wrapper.find('.user-result-item').trigger('click')
		}

		expect(wrapper.findAll('.member-tag')).toHaveLength(1)
	})

	it('removes a member when the × button is clicked', async () => {
		vi.useFakeTimers()
		const searchResult: UserSearchResult = { id: 'user-2', username: 'bob' }
		mockChatStore.searchUsers = vi.fn().mockResolvedValue([searchResult])

		const wrapper = mountModal()
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
