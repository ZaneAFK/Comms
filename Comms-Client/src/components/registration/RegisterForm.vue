<template>
	<div class="login-form rounded">
		<form @submit.prevent="submit" class="flex flex-col gap-[30px] items-center">
			<h2 class="text-xl font-semibold">Create an account</h2>

			<label class="block mb-3 w-full">
				<span class="sr-only">Username</span>
				<input v-model="username" type="text" autocomplete="username" required
					class="w-full p-2 border rounded" placeholder="Username" />
			</label>

			<label class="block mb-3 w-full">
				<span class="sr-only">Email</span>
				<input v-model="email" type="email" autocomplete="email" required
					class="w-full p-2 border rounded" placeholder="you@example.com" />
			</label>

			<label class="block mb-3 w-full">
				<span class="sr-only">Password</span>
				<div class="relative w-full">
					<input v-model="password" :type="show ? 'text' : 'password'" autocomplete="new-password" required
						class="w-full p-2 border rounded pr-10" placeholder="Password" />
					<button type="button" @click="show = !show">
						<Eye v-if="show" :size="15" />
						<EyeOff v-else :size="15" />
					</button>
				</div>
			</label>

			<label class="block mb-3 w-full">
				<span class="sr-only">Confirm password</span>
				<div class="relative w-full">
					<input v-model="confirmPassword" :type="show ? 'text' : 'password'" autocomplete="new-password" required
						class="w-full p-2 border rounded pr-10" placeholder="Confirm password" />
				</div>
			</label>

			<button type="submit" :disabled="loading" class="btn-primary text-white p-2 rounded">
				<span v-if="loading">Creating account…</span><span v-else>Create account</span>
			</button>

			<p v-if="error" class="text-sm text-red-600 mt-3" role="alert" aria-live="polite">{{ error }}</p>

			<p class="text-sm">
				Already have an account?
				<RouterLink to="/login" class="text-blue-600 hover:underline">Sign in</RouterLink>
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
