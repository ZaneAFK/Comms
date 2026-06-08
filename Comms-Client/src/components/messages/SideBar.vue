<template>
	<div class="sidebar-header">
		<span class="sidebar-title">Conversations</span>
		<button class="btn btn-primary new-conv-btn" @click="showNewConvModal = true">+</button>
		<Teleport to="body">
			<NewConvModal v-if="showNewConvModal" @close="showNewConvModal = false" />
		</Teleport>
	</div>
	<div class="conversations-list">
		<button
			v-for="conv in chatStore.conversations"
			:key="conv.id"
			class="conversation-item"
			:class="{ active: chatStore.activeConversationId === conv.id }"
			@click="selectConversation(conv.id)"
		>
			<span class="conv-name">{{ conv.name }}</span>
			<span class="conv-last-msg">{{ conv.lastMessage?.content ?? 'No messages yet' }}</span>
		</button>
		<p v-if="chatStore.conversations.length === 0" class="no-convs">No conversations yet.</p>
	</div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useChatStore } from '@/stores/chat'
import { useAuthStore } from '@/stores/auth'
import NewConvModal from './NewConvModal.vue'
	
const chatStore = useChatStore()
const authStore = useAuthStore()

const showNewConvModal = ref(false)
    
async function selectConversation(id: string) {
	await chatStore.selectConversation(id, authStore.token!)
}
</script>