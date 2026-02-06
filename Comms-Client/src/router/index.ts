import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import Login from '@/views/LoginView.vue'
import Messages from '@/views/MessagesView.vue'

const router = createRouter({
	history: createWebHistory(import.meta.env.BASE_URL),
	routes: [
		{
			path: '/',
			redirect: () => { return '/login' }
		},
		{
			path: '/login',
			name: 'login',
			component: Login
		},
		{
			path: '/messages',
			name: 'messages',
			component: Messages,
			meta: { requiresAuth: true }
		}
	]
})

router.beforeEach((to, from, next) => {
	const authStore = useAuthStore()

	if (to.meta.requiresAuth && !authStore.isAuthenticated) {
		next('/login')
	} else if (to.path === '/login' && authStore.isAuthenticated) {
		next('/messages')
	} else {
		next()
	}
})

export default router;
