<template>
	<div class="tool-bar flex items-center">
		<div class="flex items-center gap-4">
			<h1 class="text-white text-2xl font-bold">Comms</h1>
		</div>
		<div class="flex items-center gap-4">
			<h2 v-if="props.username">{{ props.username }}</h2>
			<h2 v-else>NULL USER</h2>
			<!-- TODO: This logout feature should be in a settings menu. Move to there once available. -->
			<button @click="handleLogout" class="logout-btn">Logout</button>
		</div>
	</div>
</template>

<script setup lang="ts">
	import { useAuthStore } from '@/stores/auth'
	import { useRouter } from 'vue-router'

	const props = defineProps(['username'])
	const authStore = useAuthStore()
	const router = useRouter()

	function handleLogout() {
		authStore.logout()
		router.push('/login')
	}
</script>
