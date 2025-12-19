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
    	  		<div class="relative">
    	    		<input v-model="password" :type="show ? 'text' : 'password'" autocomplete="current-password" required
    	           	class="p-2 border rounded pr-10" placeholder="Password" />
    	    		<button type="button" @click="show = !show" class="absolute right-2 top-2 text-sm">
    	      			{{ show ? 'Hide' : 'Show' }}
    	    		</button>
    	  		</div>
    		</label>

			<button type="submit" :disabled="loading" class="btn-primary text-white p-2 rounded">
    	  		<span v-if="loading">Signing in…</span><span v-else>Sign in</span>
    		</button>

			<p v-if="error" class="text-sm text-red-600 mt-3" role="alert" aria-live="polite">{{ error }}</p>
		</form>
	</div>
</template>

<script setup>
	import { ref } from 'vue';
	const email = ref('')
	const password = ref('')
	const show = ref(false)
	const loading = ref(false)
	const error = ref('')
</script>
