<template>
	<div class="modal-backdrop" @click.self="closeModal">
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
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { UserSearchResult } from '@/types'

import { useChatStore } from '@/stores/chat'
import { useAuthStore } from '@/stores/auth'

const emit = defineEmits<{ close: [] }>()

const chatStore = useChatStore()
const authStore = useAuthStore()

const newConvName = ref('')
const userSearch = ref('')
const userResults = ref<UserSearchResult[]>([])
const selectedMembers = ref<UserSearchResult[]>([])

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

async function selectConversation(id: string) {
	await chatStore.selectConversation(id, authStore.token!)
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

let searchTimeout: ReturnType<typeof setTimeout> | null = null
async function onUserSearch() {
	if (searchTimeout) clearTimeout(searchTimeout)
	if (!userSearch.value.trim()) { userResults.value = []; return }
	searchTimeout = setTimeout(async () => {
		userResults.value = await chatStore.searchUsers(userSearch.value, authStore.token!)
	}, 300)
}

function closeModal() {
	newConvName.value = ''
	userSearch.value = ''
	userResults.value = []
	selectedMembers.value = []
	emit('close')
}
</script>