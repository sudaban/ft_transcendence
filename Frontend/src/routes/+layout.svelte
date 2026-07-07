<script lang="ts">
	import './layout.css';
	import favicon from '$lib/assets/favicon.svg';
	import { onMount } from 'svelte';
	import { authStore } from '$lib/stores/auth.svelte';
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import { browser } from '$app/environment';

	let { children } = $props();
	let initialized = $state(false);

	onMount(() => {
		authStore.init();
		initialized = true;
	});

	$effect(() => {
		if (!initialized || !browser) return;
		
		const currentPath = $page.url.pathname;
		const isAuthRoute = currentPath === '/login' || currentPath === '/register';
		const isProtectedRoute = currentPath.startsWith('/profile'); // Add more here later

		if (authStore.isAuthenticated && isAuthRoute) {
			goto('/');
		} else if (!authStore.isAuthenticated && isProtectedRoute) {
			goto('/login');
		}
	});
</script>

<svelte:head><link rel="icon" href={favicon} /></svelte:head>
{#if initialized}
	{@render children()}
{/if}
