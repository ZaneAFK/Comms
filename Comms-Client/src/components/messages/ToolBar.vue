<template>
	<div class="tool-bar">
		<div class="toolbar-left">
			<div class="workspace">
				<div class="workspace-mark"><div class="workspace-mark-dot"></div></div>
				<span class="workspace-name">Comms</span>
			</div>
			<div class="toolbar-divider"></div>
			<div class="search-pill">
				<svg class="search-icn" viewBox="0 0 24 24"><circle cx="10" cy="10" r="6"/><line x1="14.5" y1="14.5" x2="20" y2="20"/></svg>
				<input
					type="text"
					class="search-pill-input"
					placeholder="Search…"
				/>
				<span class="key-hint">Shift + /</span>
			</div>
			<button class="icon-btn" @click="showNewConvModal = true" title="New conversation">
				<svg viewBox="0 0 24 24"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
			</button>
			<Teleport to="body">
				<NewConvModal v-if="showNewConvModal" @close="showNewConvModal = false" />
			</Teleport>
		</div>
		<div class="toolbar-right">
			<button class="icon-btn" @click="toggleTheme" :title="isDark ? 'Switch to light mode' : 'Switch to dark mode'">
				<!-- Sun: shown in dark mode to switch to light -->
				<svg v-if="isDark" viewBox="0 0 24 24"><circle cx="12" cy="12" r="4"/><line x1="12" y1="2" x2="12" y2="4"/><line x1="12" y1="20" x2="12" y2="22"/><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/><line x1="2" y1="12" x2="4" y2="12"/><line x1="20" y1="12" x2="22" y2="12"/><line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/></svg>
				<!-- Moon: shown in light mode to switch to dark -->
				<svg v-else viewBox="0 0 24 24"><path d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z"/></svg>
			</button>
			<button class="user-avatar" @click="handleLogout" title="Log out">{{ initial }}</button>
		</div>
	</div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
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
