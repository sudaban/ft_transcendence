# 📊 Güncelleme Sonrası Kaynak Kullanım Raporu

> [!TIP]
> Tüm 15 container başarıyla çalışıyor ✅ | Toplam **9.54 GB** disk alanı geri kazanıldı

---

## RAM Kullanımı (Tüm Container'lar Çalışırken)

| Container | RAM Kullanımı | Eski Limit | Yeni Limit | Doluluk | Durum |
|-----------|--------------|------------|------------|---------|-------|
| elasticsearch | 823 MB | ~~768 MB~~ | **1024 MB** | 80% | ✅ Rahat |
| logstash | 727 MB | ~~512 MB~~ | **768 MB** | 95% | ⚠️ Sınırda |
| grafana | 152 MB | 256 MB | 256 MB | 59% | ✅ Rahat |
| filebeat | 85 MB | 128 MB | 128 MB | 66% | ✅ Normal |
| prometheus | 82 MB | 256 MB | 256 MB | 32% | ✅ Rahat |
| backend | 57 MB | 512 MB | 512 MB | 11% | ✅ Rahat |
| cadvisor | 37 MB | 128 MB | 128 MB | 29% | ✅ Rahat |
| database | 36 MB | 512 MB | 512 MB | 7% | ✅ Rahat |
| node-exporter | 17 MB | 64 MB | 64 MB | 26% | ✅ Rahat |
| frontend | 14 MB | 256 MB | 256 MB | 6% | ✅ Rahat |
| postgres-exporter | 13 MB | 64 MB | 64 MB | 20% | ✅ Rahat |
| nginx-exporter | 11 MB | 64 MB | 64 MB | 18% | ✅ Rahat |
| nginx | 11 MB | 128 MB | 128 MB | 8% | ✅ Rahat |
| autoheal | 6 MB | ~~32 MB~~ | **64 MB** | 10% | ✅ Rahat |
| kibana | ~300 MB | 512 MB | 512 MB | ~59% | ✅ Normal |
| **TOPLAM** | **~2.4 GB** | | **4.4 GB** | | |

> **Sistem RAM:** 7.6 GB toplam → 3.3 GB kullanımda → **4.2 GB boş**

---

## Disk Kullanımı (Image Boyutları)

| Container | Image Boyutu |
|-----------|-------------|
| kibana | 1.51 GB |
| elasticsearch | 1.32 GB |
| grafana | 1.20 GB |
| logstash | 944 MB |
| filebeat | 372 MB |
| database (postgres) | 303 MB |
| prometheus | 252 MB |
| autoheal | 168 MB |
| frontend | 163 MB |
| backend | 140 MB |
| cadvisor | 76 MB |
| nginx | 68 MB |
| node-exporter | 28 MB |
| postgres-exporter | 25 MB |
| nginx-exporter | 15 MB |
| **TOPLAM IMAGE** | **~6.3 GB** |

---

## Docker Depolama Özeti (Önce → Sonra)

| Tür | Önce | Sonra | Kazanım |
|-----|------|-------|---------|
| Images | 19.1 GB | 10.1 GB | **-9.0 GB** |
| Build Cache | 5.5 GB | 2.2 GB | **-3.3 GB** |
| Volumes | 515 MB | 104 MB | **-411 MB** |
| Containers | 619 KB | 725 KB | — |
| **TOPLAM** | **25.1 GB** | **12.4 GB** | **🎉 -12.7 GB** |

> [!NOTE]
> Silinen kaynaklar:
> - `momez_mysql_data`, `momez_uploads` — eski proje volume'ları
> - `transendence_*` (v1) — eski proje volume'ları
> - 2 adet anonim volume
> - 3 adet hayalet container (6 ay+ önce çıkmış)
> - 8 adet kullanılmayan eski Docker imajı
> - 148 adet build cache girişi

---

## Yapılan Değişiklikler

### docker-compose.yml limit güncellemeleri:
```diff
 # elasticsearch
-          memory: 768M
+          memory: 1024M

 # logstash
-          memory: 512M
+          memory: 768M

 # autoheal
-          memory: 32M
+          memory: 64M
```

> [!WARNING]
> **Logstash** hâlâ %95 dolulukta. Eğer ileride sorun çıkarırsa `memory: 1024M`'e yükseltmeyi düşünebilirsin.
