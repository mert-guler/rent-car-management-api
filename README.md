# Rent Car Management API

`.NET 10` ve SQL Server tabanlı araç kiralama Web API'sidir.

## Çalıştırma

1. `appsettings.json` içindeki `DefaultConnection` değerini SQL Server ortamınıza göre güncelleyin.
2. Veritabanını oluşturun: `dotnet ef database update`
3. API'yi başlatın: `dotnet run`

Web arayüzü `http://localhost:5165` adresinde açılır. Uygulama başlangıçta migration'ları uygular ve araç tablosu boşsa deneme için üç örnek araç ekler. Arayüzden hesap oluşturup araçları görüntüleyebilir, kiralama başlatabilir, süre uzatabilir, iade yapabilir ve profil/ehliyet bilgilerini düzenleyebilirsiniz.

OpenAPI belgesi geliştirme ortamında `/openapi/v1.json` adresinden açılır.

## Endpoint özeti

- `POST /api/auth/register`, `POST /api/auth/login`: kullanıcı kaydı ve JWT girişi
- `GET/PUT /api/profile`: profil ve ehliyet bilgileri
- `GET /api/vehicles`: araçları ve uygunluk durumlarını listeleme
- `POST/PUT/DELETE /api/vehicles`: `Admin` rolü ile araç yönetimi
- `POST /api/vehicles/{id}/maintenance`: bakım tamamlandı olarak işaretleme (`Admin`)
- `POST /api/rentals`, `PUT /api/rentals/{id}/extend`, `POST /api/rentals/{id}/return`: kiralama akışı
- `GET /api/rentals`: kullanıcının kiralama geçmişi
- `POST /api/damages/rentals/{rentalId}`, `POST /api/damages/{id}/pay`: hasar bildirimi ve ödeme

Kiralama ücretleri günlük araç ücreti, 200 km/gün dahil kilometre, ekstra kilometre ücreti ve geç iade ücretiyle hesaplanır. İade sırasında eşik kilometresi aşılmışsa araç otomatik olarak `Maintenance` durumuna alınır. Tüm kritik hatalar Serilog ile konsola ve `logs/` klasörüne yazılır.

İlk admin hesabı için kayıt sonrası SQL Server'da ilgili kullanıcının `Role` alanını `Admin` yapın; üretimde JWT anahtarını mutlaka güvenli bir secret store üzerinden sağlayın.
