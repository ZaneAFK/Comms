import { mount, flushPromises } from '@vue/test-utils'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import type { Mock } from 'vitest'
import { useRoute } from 'vue-router'
import LoginForm from '@/components/login/LoginForm.vue'

type MockAuthStore = {
	login: Mock
}

let mockAuthStore: MockAuthStore
const mockPush = vi.hoisted(() => vi.fn())

vi.mock('@/stores/auth', () => ({
	useAuthStore: () => mockAuthStore,
}))

vi.mock('@/router', () => ({
	default: { push: mockPush },
}))

vi.mock('vue-router', () => ({
	useRoute: vi.fn(() => ({ query: {} })),
}))

function mountForm() {
	return mount(LoginForm, {
		global: { stubs: { RouterLink: true } },
	})
}

beforeEach(() => {
	vi.clearAllMocks()
	vi.mocked(useRoute).mockReturnValue({ query: {} } as ReturnType<typeof useRoute>)
	mockAuthStore = {
		login: vi.fn().mockResolvedValue({ success: true }),
	}
})

describe('LoginForm', () => {
	it('shows the registration success banner when ?registered=true', () => {
		vi.mocked(useRoute).mockReturnValue({ query: { registered: 'true' } } as ReturnType<typeof useRoute>)
		const wrapper = mountForm()
		expect(wrapper.find('[role="status"]').exists()).toBe(true)
	})

	it('does not show the banner without the registered query param', () => {
		const wrapper = mountForm()
		expect(wrapper.find('[role="status"]').exists()).toBe(false)
	})

	it('calls login with the entered email and password', async () => {
		const wrapper = mountForm()
		await wrapper.find('input[type="email"]').setValue('alice@example.com')
		await wrapper.find('input[type="password"]').setValue('Password123!')
		await wrapper.find('form').trigger('submit')
		expect(mockAuthStore.login).toHaveBeenCalledWith('alice@example.com', 'Password123!')
	})

	it('navigates to /messages on successful login', async () => {
		const wrapper = mountForm()
		await wrapper.find('form').trigger('submit')
		await flushPromises()
		expect(mockPush).toHaveBeenCalledWith('/messages')
	})

	it('shows an error message on failed login', async () => {
		mockAuthStore.login = vi.fn().mockResolvedValue({ success: false, error: 'Invalid credentials.' })
		const wrapper = mountForm()
		await wrapper.find('form').trigger('submit')
		await flushPromises()
		expect(wrapper.find('[role="alert"]').text()).toBe('Invalid credentials.')
	})

	it('does not navigate on failed login', async () => {
		mockAuthStore.login = vi.fn().mockResolvedValue({ success: false, error: 'Invalid credentials.' })
		const wrapper = mountForm()
		await wrapper.find('form').trigger('submit')
		await flushPromises()
		expect(mockPush).not.toHaveBeenCalled()
	})

	it('disables the button and shows "Signing in…" while the request is in flight', async () => {
		mockAuthStore.login = vi.fn().mockImplementation(() => new Promise(() => {}))
		const wrapper = mountForm()
		await wrapper.find('input[type="email"]').setValue('alice@example.com')
		await wrapper.find('input[type="password"]').setValue('Password123!')
		await wrapper.find('form').trigger('submit')
		expect(wrapper.find('button[type="submit"]').attributes('disabled')).toBeDefined()
		expect(wrapper.find('button[type="submit"]').text()).toBe('Signing in…')
	})
})
