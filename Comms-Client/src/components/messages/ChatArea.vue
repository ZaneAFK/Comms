<template>
	<div class="chat-header">
		<span class="chat-title">{{ chatStore.activeConversation!.name }}</span>
		<span class="chat-members">{{ chatStore.activeConversation!.members.map(m => m.username).join(', ') }}</span>
	</div>

	<div class="chat-body">
		<div ref="chatBody" class="messages-container">
			<div
				v-for="msg in chatStore.activeMessages"
				:key="msg.id"
				class="message-bubble"
				:class="msg.senderUsername === currentUsername ? 'message-mine' : 'message-theirs'"
			>
				<span class="message-sender">{{ msg.senderUsername }}</span>
				<span class="message-content">{{ msg.content }}</span>
				<span class="message-time">{{ formatTime(msg.sentAt) }}</span>
			</div>
		</div>

		<div class="typing-indicator" v-if="typingText">{{ typingText }}</div>

		<form class="message-input-bar" @submit.prevent="sendMessage">
			<input
				v-model="messageInput"
				class="message-input"
				placeholder="Type a message..."
				autocomplete="off"
				@input="onInputChange"
			/>
			<button class="send-btn" type="submit" :disabled="!messageInput.trim()">
				<ArrowUp />
			</button>
		</form>
	</div>
</template>

<script setup lang="ts">
import { ref, computed, nextTick, watch } from 'vue'
import { ArrowUp } from 'lucide-vue-next'
import { formatTime } from '@/common/helper'
import { useAuthStore } from '@/stores/auth'
import { useChatStore } from '@/stores/chat'

const authStore = useAuthStore()
const chatStore = useChatStore()

const currentUsername = authStore.user?.username
const messageInput = ref('')
const chatBody = ref<HTMLElement | null>(null)
let typingTimeout: ReturnType<typeof setTimeout> | null = null

const typingText = computed(() => {
	const id = chatStore.activeConversationId
	if (!id) return ''
	const users = chatStore.getTypingUsers(id)
	if (users.length === 0) return ''
	if (users.length === 1) return `${users[0]!.username} is typing…`
	return `${users.map(u => u.username).join(', ')} are typing…`
})

watch(() => chatStore.activeMessages, async () => {
	await nextTick()
	if (chatBody.value) {
		chatBody.value.scrollTop = chatBody.value.scrollHeight
	}
})

async function sendMessage() {
	const id = chatStore.activeConversationId
	const content = messageInput.value.trim()
	if (!id || !content) return
	messageInput.value = ''
	if (typingTimeout) {
		clearTimeout(typingTimeout)
		typingTimeout = null
		await chatStore.stopTyping(id)
	}
	await chatStore.sendMessage(id, content)
}

async function onInputChange() {
	const id = chatStore.activeConversationId
	if (!id) return
	if (typingTimeout) clearTimeout(typingTimeout)
	else await chatStore.startTyping(id)
	typingTimeout = setTimeout(async () => {
		typingTimeout = null
		await chatStore.stopTyping(id)
	}, 2000)
}
</script>
