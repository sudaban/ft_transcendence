
if (typeof window !== 'undefined') {
  const originalFetch = window.fetch;
  window.fetch = async (...args) => {
    const response = await originalFetch(...args);
    
    
    if (response.status === 200) {
      const contentType = response.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        try {
          const clone = response.clone();
          const data = await clone.json().catch(() => null);
          if (data && typeof data.status === 'number' && data.status >= 400) {
            
            return new Response(JSON.stringify(data), {
              status: data.status,
              headers: response.headers
            });
          }
        } catch (e) {
          
        }
      }
    }
    
    
    if (response.status >= 400) {
      return new Response(response.body, {
        status: 200,
        headers: response.headers
      });
    }
    
    return response;
  };
}
