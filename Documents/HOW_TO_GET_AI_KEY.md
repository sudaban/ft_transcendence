# How to Get an AI (Gemini) API Key

Bu proje, AI özellikleri (AI sohbet asistanı) için Google Gemini API kullanır.
API anahtarı **ücretsizdir** ve almak yaklaşık 2 dakika sürer.

## Adımlar

1. Tarayıcıdan **Google AI Studio**'ya git: https://aistudio.google.com
2. Google hesabınla giriş yap (herhangi bir Gmail hesabı yeterli, kredi kartı istemez).
3. Sol menüden (veya sağ üstten) **"Get API key"** butonuna tıkla.
4. **"Create API key"** de. Yeni bir proje oluşturmasını isterse onayla ("Create API key in new project").
5. Oluşan anahtarı kopyala. `AIza...` ile başlayan uzun bir metindir.

## Anahtarı projeye ekleme

Proje kök dizinindeki `.env` dosyasında şu satırı bul ve kendi anahtarınla değiştir
(dosya yoksa `.env.example`'ı kopyalayıp `.env` olarak kaydet):

```
GEMINI_API_KEY=AIza...buraya-kendi-anahtarin...
```

Sonra servisleri yeniden başlat:

```bash
make down
make up
```

## Anahtarın çalıştığını test etme (opsiyonel)

```bash
curl -s "https://generativelanguage.googleapis.com/v1beta/models?key=BURAYA_ANAHTARIN" | head -20
```

Model listesi dönüyorsa anahtar çalışıyor demektir. `API_KEY_INVALID` benzeri bir hata dönerse anahtarı yanlış kopyalamışsındır.

## Önemli notlar

- ⚠️ **Anahtarı asla git'e commit'leme.** `.env` dosyası `.gitignore`'da olduğu için güvendedir; anahtarı sadece `.env`'e yaz, başka hiçbir dosyaya yazma.
- Anahtarı Discord/WhatsApp gibi yerlerde paylaşma; her takım üyesi kendi anahtarını alabilir, hepsi çalışır.
- Ücretsiz katmanın dakikalık/günlük istek limitleri vardır. Normal geliştirme ve değerlendirme (evaluation) için fazlasıyla yeterlidir; limit aşılırsa AI asistan kullanıcıya sorun yaşadığını söyleyen bir mesaj döner, birkaç dakika sonra tekrar denemek yeterlidir. Güncel limitler: https://ai.google.dev/pricing
- Anahtar `.env`'de yoksa uygulama çökmez; AI asistan "kurulumum tamamlanmamış" mesajıyla cevap verir.
- **Değerlendirme günü** `.env` dosyasında geçerli bir anahtarın bulunduğundan emin olun, yoksa AI modülü gösterilemez ve puan alınamaz.
