<script setup lang="ts">
</script>

<template>
	<nav>
		<RouterLink to='/'>Go to Home</RouterLink>
		<RouterLink to='/login'>Go to Login</RouterLink>
	</nav>

	<div class="greetings">
		<h1 class="green">THIS IS THE HOME PAGE</h1>
	</div>
</template>

<style scoped>
header {
	line-height: 1.5;
}

.logo {
	display: block;
	margin: 0 auto 2rem;
}

@media (width >= 1024px) {
	header {
		display: flex;
		place-items: center;
		padding-right: calc(var(--section-gap) / 2);
	}

	.logo {
		margin: 0 2rem 0 0;
	}

	header .wrapper {
		display: flex;
		place-items: flex-start;
		flex-wrap: wrap;
	}
}
</style>
