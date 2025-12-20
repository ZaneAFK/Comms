<template>
	<div class="flex flex-col">
		<ToolBar :username="currentUsername" />
	</div>
</template>

<script setup lang="ts">
	import { useAuthStore } from '@/stores/auth.ts'
	import ToolBar from '@/components/messages/ToolBar.vue'

	const authStore = useAuthStore()
	const currentUsername = authStore?.user?.username

</script>
