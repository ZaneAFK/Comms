<template>
	<div class="messages-layout">
		<ToolBar />
		<div class="messages-body">
			<aside class="conversations-sidebar">
				<SideBar />
			</aside>
			<main class="chat-area">
				<ChatArea />
			</main>
		</div>
	</div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useChatStore } from '@/stores/chat'
import ToolBar from '@/components/messages/ToolBar.vue'
import SideBar from '@/components/messages/SideBar.vue'
import ChatArea from '@/components/messages/ChatArea.vue'

const authStore = useAuthStore()
const chatStore = useChatStore()

onMounted(async () => {
	const token = authStore.token!
	await chatStore.loadConversations(token)
	await chatStore.connect(token)
})
</script>
