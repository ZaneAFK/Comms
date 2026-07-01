<template>
	<div class="tool-bar">
		<div class="toolbar-left">
			<div class="workspace">
				<div class="workspace-mark"><div class="workspace-mark-dot"></div></div>
				<span class="workspace-name">Comms</span>
			</div>
			<div class="toolbar-divider"></div>
			<div class="search-pill">
				<Search class="search-icn" />
				<input
					type="text"
					class="search-pill-input"
					placeholder="Search…"
				/>
				<span class="key-hint">Shift + /</span>
			</div>
			<button class="icon-btn" @click="showNewConvModal = true" title="New conversation">
				<Plus />
			</button>
			<Teleport to="body">
				<NewConvModal v-if="showNewConvModal" @close="showNewConvModal = false" />
			</Teleport>
		</div>
		<div class="toolbar-right">
			<button class="icon-btn" @click="toggleTheme" :title="isDark ? 'Switch to light mode' : 'Switch to dark mode'">
				<Sun v-if="isDark" />
				<Moon v-else />
			</button>
			<button class="user-avatar" @click="handleLogout" title="Log out">{{ initial }}</button>
		</div>
	</div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Search, Plus, Sun, Moon } from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'
import NewConvModal from './NewConvModal.vue'

const authStore = useAuthStore()
const router = useRouter()

const showNewConvModal = ref(false)
const initial = (authStore.user?.username ?? '?').charAt(0).toUpperCase()
const isDark = ref(true)

onMounted(() => {
	if (localStorage.getItem('theme') === 'light') {
		isDark.value = false
		document.body.classList.add('light')
	}
})

function toggleTheme() {
	isDark.value = !isDark.value
	document.body.classList.toggle('light', !isDark.value)
	localStorage.setItem('theme', isDark.value ? 'dark' : 'light')

	console.log(showNewConvModal)
}

function handleLogout() {
	authStore.logout()
	router.push('/login')
}
</script>
