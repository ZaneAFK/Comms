import { mount, flushPromises } from '@vue/test-utils'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import type { Mock } from 'vitest'
import RegisterForm from '@/components/registration/RegisterForm.vue'

type MockAuthStore = {
	register: Mock
}

let mockAuthStore: MockAuthStore
const mockPush = vi.hoisted(() => vi.fn())

vi.mock('@/stores/auth', () => ({
	useAuthStore: () => mockAuthStore,
}))

vi.mock('@/router', () => ({
	default: { push: mockPush },
}))

function mountForm() {
	return mount(RegisterForm, {
		global: { stubs: { RouterLink: true } },
	})
}

async function fillForm(wrapper: ReturnType<typeof mountForm>, opts: {
	username?: string
	email?: string
	password?: string
	confirm?: string
} = {}) {
	const {
		username = 'alice',
		email = 'alice@example.com',
		password = 'Password123!',
		confirm = 'Password123!',
	} = opts
	await wrapper.find('input[placeholder="Username"]').setValue(username)
	await wrapper.find('input[placeholder="you@example.com"]').setValue(email)
	await wrapper.findAll('input[type="password"]')[0]!.setValue(password)
	await wrapper.findAll('input[type="password"]')[1]!.setValue(confirm)
}

beforeEach(() => {
	vi.clearAllMocks()
	mockAuthStore = {
		register: vi.fn().mockResolvedValue({ success: true }),
	}
})

describe('RegisterForm', () => {
	it('shows a password mismatch error without calling the API', async () => {
		const wrapper = mountForm()
		await fillForm(wrapper, { password: 'abc', confirm: 'xyz' })
		await wrapper.find('form').trigger('submit')
		expect(wrapper.find('[role="alert"]').text()).toBe('Passwords do not match.')
		expect(mockAuthStore.register).not.toHaveBeenCalled()
	})

	it('calls register with the entered username, email, and password', async () => {
		const wrapper = mountForm()
		await fillForm(wrapper)
		await wrapper.find('form').trigger('submit')
		expect(mockAuthStore.register).toHaveBeenCalledWith('alice', 'alice@example.com', 'Password123!')
	})

	it('navigates to /login?registered=true on successful registration', async () => {
		const wrapper = mountForm()
		await fillForm(wrapper)
		await wrapper.find('form').trigger('submit')
		await flushPromises()
		expect(mockPush).toHaveBeenCalledWith('/login?registered=true')
	})

	it('shows an error message on failed registration', async () => {
		mockAuthStore.register = vi.fn().mockResolvedValue({ success: false, error: 'Email already taken.' })
		const wrapper = mountForm()
		await fillForm(wrapper)
		await wrapper.find('form').trigger('submit')
		await flushPromises()
		expect(wrapper.find('[role="alert"]').text()).toBe('Email already taken.')
	})

	it('shows a fallback error when the API returns no message', async () => {
		mockAuthStore.register = vi.fn().mockResolvedValue({ success: false })
		const wrapper = mountForm()
		await fillForm(wrapper)
		await wrapper.find('form').trigger('submit')
		await flushPromises()
		expect(wrapper.find('[role="alert"]').text()).toBe('Registration failed.')
	})

	it('disables the button and shows "Creating account…" while the request is in flight', async () => {
		mockAuthStore.register = vi.fn().mockImplementation(() => new Promise(() => {}))
		const wrapper = mountForm()
		await fillForm(wrapper)
		await wrapper.find('form').trigger('submit')
		expect(wrapper.find('button[type="submit"]').attributes('disabled')).toBeDefined()
		expect(wrapper.find('button[type="submit"]').text()).toBe('Creating account…')
	})
})
