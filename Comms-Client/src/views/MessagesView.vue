<template>
	<div class="messages-layout">
		<ToolBar :username="currentUsername" />

		<div class="messages-body">
			<!-- Sidebar -->
			<aside class="conversations-sidebar">
				<div class="sidebar-header">
					<span class="sidebar-title">Conversations</span>
					<button class="btn btn-primary new-conv-btn" @click="showNewConvModal = true">+</button>
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
			</aside>

			<!-- Chat area -->
			<main class="chat-area">
				<template v-if="activeConversation">
					<div class="chat-header">
						<span class="chat-title">{{ activeConversation.name }}</span>
						<span class="chat-members">{{ activeConversation.members.map(m => m.username).join(', ') }}</span>
					</div>

					<div ref="messagesContainer" class="messages-container">
						<div
							v-for="msg in currentMessages"
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
							placeholder="Type a message…"
							autocomplete="off"
							@input="onInputChange"
						/>
						<button class="btn btn-primary send-btn" type="submit" :disabled="!messageInput.trim()">Send</button>
					</form>
				</template>

				<div v-else class="chat-empty">
					<p>Select a conversation or create a new one.</p>
				</div>
			</main>
		</div>

		<!-- New conversation modal -->
		<div v-if="showNewConvModal" class="modal-backdrop" @click.self="closeModal">
			<div class="modal">
				<h2>New Conversation</h2>
				<div class="form-stack">
					<input v-model="newConvName" placeholder="Conversation name" />
					<input
						v-model="userSearch"
						placeholder="Search users…"
						@input="onUserSearch"
					/>
					<div class="user-results" v-if="userResults.length">
						<button
							v-for="u in userResults"
							:key="u.id"
							class="user-result-item"
							@click="addMember(u)"
						>
							{{ u.username }}
						</button>
					</div>
					<div class="selected-members" v-if="selectedMembers.length">
						<span v-for="m in selectedMembers" :key="m.id" class="member-tag">
							{{ m.username }}
							<button @click="removeMember(m.id)">×</button>
						</span>
					</div>
					<div class="modal-actions">
						<button class="btn btn-outline" @click="closeModal">Cancel</button>
						<button class="btn btn-primary" @click="createConversation" :disabled="!newConvName.trim()">Create</button>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
	import { ref, computed, onMounted, nextTick, watch } from 'vue'
	import { useAuthStore } from '@/stores/auth'
	import { useChatStore } from '@/stores/chat'
	import ToolBar from '@/components/messages/ToolBar.vue'
	import type { UserSearchResult } from '@/types'

	const authStore = useAuthStore()
	const chatStore = useChatStore()

	const currentUsername = authStore.user?.username

	const messageInput = ref('')
	const messagesContainer = ref<HTMLElement | null>(null)
	const showNewConvModal = ref(false)
	const newConvName = ref('')
	const userSearch = ref('')
	const userResults = ref<UserSearchResult[]>([])
	const selectedMembers = ref<UserSearchResult[]>([])
	let typingTimeout: ReturnType<typeof setTimeout> | null = null

	const activeConversation = computed(() =>
		chatStore.conversations.find(c => c.id === chatStore.activeConversationId) ?? null
	)

	const currentMessages = computed(() =>
		chatStore.activeConversationId ? chatStore.getMessages(chatStore.activeConversationId) : []
	)

	const typingText = computed(() => {
		if (!chatStore.activeConversationId) return ''
		const users = chatStore.getTypingUsers(chatStore.activeConversationId)
		if (users.length === 0) return ''
		if (users.length === 1) return `${users[0]!.username} is typing…`
		return `${users.map(u => u.username).join(', ')} are typing…`
	})

	onMounted(async () => {
		const token = authStore.token!
		await chatStore.loadConversations(token)
		await chatStore.connect(token)
	})

	watch(currentMessages, async () => {
		await nextTick()
		if (messagesContainer.value) {
			messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
		}
	})

	async function selectConversation(id: string) {
		chatStore.setActiveConversation(id)
		if (!chatStore.getMessages(id).length) {
			await chatStore.loadMessages(id, authStore.token!)
		}
	}

	async function sendMessage() {
		const content = messageInput.value.trim()
		if (!content || !chatStore.activeConversationId) return
		messageInput.value = ''
		if (typingTimeout) {
			clearTimeout(typingTimeout)
			typingTimeout = null
			await chatStore.stopTyping(chatStore.activeConversationId)
		}
		await chatStore.sendMessage(chatStore.activeConversationId, content)
	}

	async function onInputChange() {
		if (!chatStore.activeConversationId) return
		if (typingTimeout) clearTimeout(typingTimeout)
		else await chatStore.startTyping(chatStore.activeConversationId)
		typingTimeout = setTimeout(async () => {
			typingTimeout = null
			await chatStore.stopTyping(chatStore.activeConversationId!)
		}, 2000)
	}

	let searchTimeout: ReturnType<typeof setTimeout> | null = null
	async function onUserSearch() {
		if (searchTimeout) clearTimeout(searchTimeout)
		if (!userSearch.value.trim()) { userResults.value = []; return }
		searchTimeout = setTimeout(async () => {
			userResults.value = await chatStore.searchUsers(userSearch.value, authStore.token!)
		}, 300)
	}

	function addMember(user: UserSearchResult) {
		if (!selectedMembers.value.find(m => m.id === user.id)) {
			selectedMembers.value.push(user)
		}
		userSearch.value = ''
		userResults.value = []
	}

	function removeMember(id: string) {
		selectedMembers.value = selectedMembers.value.filter(m => m.id !== id)
	}

	async function createConversation() {
		const conv = await chatStore.createConversation(
			newConvName.value.trim(),
			selectedMembers.value.map(m => m.id),
			authStore.token!
		)
		if (conv) {
			closeModal()
			await selectConversation(conv.id)
		}
	}

	function closeModal() {
		showNewConvModal.value = false
		newConvName.value = ''
		userSearch.value = ''
		userResults.value = []
		selectedMembers.value = []
	}

	function formatTime(iso: string) {
		return new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
	}
</script>
