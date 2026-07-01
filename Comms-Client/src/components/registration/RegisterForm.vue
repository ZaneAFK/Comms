<template>
	<div class="auth-card">
		<form @submit.prevent="submit" class="form-stack">
			<h2>Create an account</h2>

			<label>
				<span class="sr-only">Username</span>
				<input v-model="username" type="text" autocomplete="username" required placeholder="Username" />
			</label>

			<label>
				<span class="sr-only">Email</span>
				<input v-model="email" type="email" autocomplete="email" required placeholder="you@example.com" />
			</label>

			<label>
				<span class="sr-only">Password</span>
				<div class="password-wrap">
					<input v-model="password" :type="show ? 'text' : 'password'" autocomplete="new-password" required placeholder="Password" />
					<button type="button" class="eye-btn" @click="show = !show">
						<Eye v-if="show" :size="15" />
						<EyeOff v-else :size="15" />
					</button>
				</div>
			</label>

			<label>
				<span class="sr-only">Confirm password</span>
				<div class="password-wrap">
					<input v-model="confirmPassword" :type="show ? 'text' : 'password'" autocomplete="new-password" required placeholder="Confirm password" />
				</div>
			</label>

			<button type="submit" :disabled="loading" class="btn btn-primary btn-lg">
				<span v-if="loading">Creating account…</span><span v-else>Create account</span>
			</button>

			<p v-if="error" class="alert alert-error" role="alert" aria-live="polite">{{ error }}</p>

			<p class="auth-footer">
				Already have an account?
				<RouterLink to="/login" class="auth-link">Sign in</RouterLink>
			</p>
		</form>
	</div>
</template>

<script setup lang="ts">
import { Eye, EyeOff } from 'lucide-vue-next'
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'

const username = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const show = ref(false)
const loading = ref(false)
const error = ref('')
const authStore = useAuthStore()

async function submit() {
	if (password.value !== confirmPassword.value) {
		error.value = 'Passwords do not match.'
		return
	}

	loading.value = true
	error.value = ''

	try {
		const result = await authStore.register(username.value, email.value, password.value)

		if (!result.success) {
			error.value = result.error ?? 'Registration failed.'
			return
		}

		router.push('/login?registered=true')
	}
	finally {
		loading.value = false
	}
}
</script>
