<template>
	<div class="login-form rounded">
		<form @submit.prevent="submit" class="flex flex-col gap-[30px] items-center">
			<h2 class="text-xl font-semibold">Sign in to your account</h2>

			<label class="block mb-3 w-full">
    	  		<span class="sr-only">Email</span>
    	  		<input v-model="email" type="email" autocomplete="username" required
    	        class="w-full p-2 border rounded" placeholder="you@example.com" />
    		</label>

			<label class="block mb-3 w-full">
    	  		<span class="sr-only">Password</span>
    	  		<div class="relative w-full">
    	    		<input v-model="password" :type="show ? 'text' : 'password'" autocomplete="current-password" required
    	           	class="w-full p-2 border rounded pr-10" placeholder="Password" />
    	    		<button type="button" @click="show = !show">
						<Eye v-if="show" :size="15" />
						<EyeOff v-else :size="15"/>
    	    		</button>
    	  		</div>
    		</label>

			<button type="submit" v-on:click="submit" :disabled="loading" class="btn-primary text-white p-2 rounded">
    	  		<span v-if="loading">Signing in…</span><span v-else>Sign in</span>
    		</button>

			<p v-if="error" class="text-sm text-red-600 mt-3" role="alert" aria-live="polite">{{ error }}</p>
		</form>
	</div>
</template>

<script setup lang="ts">
	import { Eye, EyeOff } from 'lucide-vue-next'
	import { ref } from 'vue'
	import { useAuthStore } from '@/stores/auth'
	import router from '@/router'
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
