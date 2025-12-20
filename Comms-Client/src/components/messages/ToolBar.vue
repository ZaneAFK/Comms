<template>
	<div class="tool-bar flex items-center">
		<div class="flex items-center gap-4">
			<h1 class="text-white text-2xl font-bold">Comms</h1>
		</div>
		<div class="flex">
			<h2 v-if="props.username">{{ props.username }}</h2>
			<h2 v-else>NULL USER</h2>
		</div>
	</div>
</template>

<script setup lang="ts">
	const props = defineProps(['username'])
</script>
