// Tarayıcı seviyesinde tüm fetch isteklerini yakalar ve 400 ve üstünü 200'e maskeler
if (typeof window !== 'undefined') {
  const originalFetch = window.fetch;
  window.fetch = async (...args) => {
    const response = await originalFetch(...args);
    
    // Eğer sunucu 200 OK döndüyse, gövdede maskelenmiş bir hata var mı kontrol et
    if (response.status === 200) {
      const contentType = response.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        try {
          const clone = response.clone();
          const data = await clone.json().catch(() => null);
          if (data && typeof data.status === 'number' && data.status >= 400) {
            // JS tarafındaki hata mekanizmalarının çalışması için durumu gerçek hata koduna geri çevir
            return new Response(JSON.stringify(data), {
              status: data.status,
              headers: response.headers
            });
          }
        } catch (e) {
          // JSON okuma hatası yoksayılabilir
        }
      }
    }
    
    // Sunucu doğrudan >= 400 döndüyse (maskelenmemişse), tarayıcı konsol gürültüsünü azaltmak için maskele
    if (response.status >= 400) {
      return new Response(response.body, {
        status: 200,
        headers: response.headers
      });
    }
    
    return response;
  };
}
