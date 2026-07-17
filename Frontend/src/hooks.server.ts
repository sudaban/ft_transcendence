import type { Handle } from '@sveltejs/kit';

export const handle: Handle = async ({ event, resolve }) => {
	const response = await resolve(event);
	
	if (response.status >= 400) {
		return new Response(response.body, {
			status: 200,
			headers: response.headers
		});
	}
	
	return response;
};
