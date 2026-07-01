<template>
	<div class="auth-card">
		<form @submit.prevent="submit" class="form-stack">
			<h2>Sign in</h2>

			<p v-if="registered" class="alert alert-info" role="status">
				Account created! You can now sign in.
			</p>

			<label>
				<span class="sr-only">Email</span>
				<input v-model="email" type="email" autocomplete="username" required placeholder="you@example.com" />
			</label>

			<label>
				<span class="sr-only">Password</span>
				<div class="password-wrap">
					<input v-model="password" :type="show ? 'text' : 'password'" autocomplete="current-password" required placeholder="Password" />
					<button type="button" class="eye-btn" @click="show = !show">
						<Eye v-if="show" :size="15" />
						<EyeOff v-else :size="15" />
					</button>
				</div>
			</label>

			<button type="submit" :disabled="loading" class="btn btn-primary btn-lg">
				<span v-if="loading">Signing in…</span><span v-else>Sign in</span>
			</button>

			<p v-if="error" class="alert alert-error" role="alert" aria-live="polite">{{ error }}</p>

			<p class="auth-footer">
				Don't have an account?
				<RouterLink to="/register" class="auth-link">Create one</RouterLink>
			</p>
		</form>
	</div>
</template>

<script setup lang="ts">
import { Eye, EyeOff } from 'lucide-vue-next'
import { ref, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRoute } from 'vue-router'
import router from '@/router'
const route = useRoute()
const registered = computed(() => route.query.registered === 'true')
const email = ref('')
const password = ref('')
const show = ref(false)
const loading = ref(false)
const error = ref('')
const authStore = useAuthStore()

async function submit() {
	loading.value = true
	error.value = ''

	try {
		const result = await authStore.login(email.value, password.value)

		if (!result.success) {
			error.value = result.error
			return
		}

		router.push('/messages')
	}
	finally {
		loading.value = false
	}
}
</script>
