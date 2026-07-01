<template>
	<div class="sidebar-header">
		<span class="sidebar-title">Conversations</span>
	</div>
	<div class="conversations-list">
		<button
			v-for="conv in chatStore.conversations"
			:key="conv.id"
			class="conversation-item"
			:class="{ active: chatStore.activeConversationId === conv.id }"
			@click="selectConversation(conv.id)"
		>
			<div class="conv-avatar">{{ getInitials(conv.name) }}</div>
			<div class="conv-info">
				<div class="conv-top">
					<span class="conv-name">{{ conv.name }}</span>
					<span class="conv-time">{{ conv.lastMessage ? formatTime(conv.lastMessage.sentAt) : '' }}</span>
				</div>
				<span class="conv-last-msg">{{ conv.lastMessage?.content ?? 'No messages yet' }}</span>
			</div>
		</button>
		<p v-if="chatStore.conversations.length === 0" class="no-convs">No conversations yet.</p>
	</div>
</template>

<script setup lang="ts">
import { useChatStore } from '@/stores/chat'
import { useAuthStore } from '@/stores/auth'
import { formatTime } from '@/common/helper'

const chatStore = useChatStore()
const authStore = useAuthStore()

function getInitials(name: string): string {
	const words = name.trim().split(/\s+/)
	if (words.length >= 2) return (words[0]![0]! + words[1]![0]!).toUpperCase()
	return name.slice(0, 2).toUpperCase()
}

async function selectConversation(id: string) {
	await chatStore.selectConversation(id, authStore.token!)
}
</script>
