import { setActivePinia, createPinia } from 'pinia'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useAuthStore } from '@/stores/auth'

interface MockUser {
	id: string
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
			token: 'mock-token',
			user: { id: '1', username: 'alice', email: 'alice@example.com' }
		}

		// Mock fetch to return a successful response
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

	it('login returns error when fetch fails', async () => {
		const store = useAuthStore()

		const mockError = { error: 'Invalid credentials' }

		vi.stubGlobal('fetch', vi.fn(() =>
			Promise.resolve({
				ok: false,
				json: () => Promise.resolve(mockError)
			} as Response)
		))

		const result = await store.login('bob@example.com', 'badpassword')

		expect(result.success).toBe(false)
		expect(result.error).toBe('Invalid credentials')
		expect(store.user).toBeNull()
		expect(store.token).toBeNull()
		expect(store.isAuthenticated).toBe(false)
	})

	it('logout clears user, token, and localStorage', async () => {
		const store = useAuthStore()
		store.user = { id: '1', username: 'alice', email: 'alice@example.com' }
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
