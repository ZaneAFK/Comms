import { setActivePinia, createPinia } from 'pinia'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useAuthStore } from '@/stores/auth'

interface MockUser {
	username: string
	email: string
}

// Reset localStorage and Pinia before each test
beforeEach(() => {
	localStorage.clear()
	setActivePinia(createPinia())
})

describe('Auth Store', () => {
	it('initializes with null user and token', () => {
		const store = useAuthStore()
		expect(store.user).toBeNull()
		expect(store.token).toBeNull()
		expect(store.isAuthenticated).toBe(false)
	})

	it('login sets user and token on success', async () => {
		const store = useAuthStore()

		const mockResponse = {
			succeeded: true,
			token: 'mock-token',
			user: { username: 'alice', email: 'alice@example.com' }
		}

		vi.stubGlobal('fetch', vi.fn(() =>
			Promise.resolve({
				ok: true,
				json: () => Promise.resolve(mockResponse)
			} as Response)
		))

		const result = await store.login('alice@example.com', 'Password123!')

		expect(result.success).toBe(true)
		expect(store.token).toBe(mockResponse.token)
		expect(store.user).toEqual(mockResponse.user)
		expect(localStorage.getItem('token')).toBe(mockResponse.token)
		expect(localStorage.getItem('user')).toBe(JSON.stringify(mockResponse.user))
		expect(store.isAuthenticated).toBe(true)
	})

	it('login returns error from body when credentials are invalid', async () => {
		const store = useAuthStore()

		vi.stubGlobal('fetch', vi.fn(() =>
			Promise.resolve({
				ok: true,
				json: () => Promise.resolve({ succeeded: false, error: 'Invalid email or password.' })
			} as Response)
		))

		const result = await store.login('bob@example.com', 'badpassword')

		expect(result.success).toBe(false)
		expect(result.error).toBe('Invalid email or password.')
		expect(store.user).toBeNull()
		expect(store.token).toBeNull()
		expect(store.isAuthenticated).toBe(false)
	})

	it('login returns generic error when request itself fails', async () => {
		const store = useAuthStore()

		vi.stubGlobal('fetch', vi.fn(() =>
			Promise.resolve({
				ok: false,
				json: () => Promise.resolve({})
			} as Response)
		))

		const result = await store.login('bob@example.com', 'badpassword')

		expect(result.success).toBe(false)
		expect(result.error).toBe('Login failed')
		expect(store.isAuthenticated).toBe(false)
	})

	it('register returns success when API succeeds', async () => {
		const store = useAuthStore()

		vi.stubGlobal('fetch', vi.fn(() =>
			Promise.resolve({
				ok: true,
				json: () => Promise.resolve({ succeeded: true })
			} as Response)
		))

		const result = await store.register('alice', 'alice@example.com', 'Password123!')

		expect(result.success).toBe(true)
	})

	it('register returns error from API body on failure', async () => {
		const store = useAuthStore()

		vi.stubGlobal('fetch', vi.fn(() =>
			Promise.resolve({
				ok: true,
				json: () => Promise.resolve({ succeeded: false, error: 'Email already taken' })
			} as Response)
		))

		const result = await store.register('alice', 'alice@example.com', 'Password123!')

		expect(result.success).toBe(false)
		expect(result.error).toBe('Email already taken')
	})

	it('register returns generic error when request itself fails', async () => {
		const store = useAuthStore()

		vi.stubGlobal('fetch', vi.fn(() =>
			Promise.resolve({
				ok: false,
				json: () => Promise.resolve({})
			} as Response)
		))

		const result = await store.register('alice', 'alice@example.com', 'Password123!')

		expect(result.success).toBe(false)
		expect(result.error).toBe('Registration failed')
	})

	it('logout clears user, token, and localStorage', async () => {
		const store = useAuthStore()
		store.user = { username: 'alice', email: 'alice@example.com' }
		store.token = 'mock-token'
		localStorage.setItem('user', JSON.stringify(store.user))
		localStorage.setItem('token', store.token!)

		store.logout()

		expect(store.user).toBeNull()
		expect(store.token).toBeNull()
		expect(localStorage.getItem('user')).toBeNull()
		expect(localStorage.getItem('token')).toBeNull()
		expect(store.isAuthenticated).toBe(false)
	})
})
