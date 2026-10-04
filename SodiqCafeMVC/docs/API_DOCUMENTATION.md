# Sodiq Cafe - REST API Hujjatlari (Documentation)

Ushbu hujjat Sodiq Cafe tizimining (MVC'dan ajratilgan holatda) Mobile ilovalar (Admin va Seller) hamda boshqa mijozlar bilan integratsiya qilinishi uchun mo'ljallangan REST API endpointlarini o'z ichiga oladi. Endpointlar kelgusida quriladigan API-larni rejalashtirish uchun tuzilgan.

**Asosiy URL (Base URL):** `https://api.sodiqcafe.uz/api/v1`
**Format:** `application/json`
**Autentifikatsiya:** Bearer Token (JWT)

---

## 1. 🔐 Avtorizatsiya (Authentication API)
*Barcha foydalanuvchilar (Admin, Owner, Mijoz) uchun.*

| Metod | Endpoint | Tavsif | Status |
|---|---|---|---|
| `POST` | `/auth/login` | Email/Telefon va parol orqali tizimga kirish (JWT qaytaradi) | Mavjudga asoslangan |
| `POST` | `/auth/login-with-token` | Telegram/Boshqa token orqali login | Mavjudga asoslangan |
| `POST` | `/auth/refresh-token` | **(Tavsiya etiladi)** Eskirgan JWT tokenni yangilash (Mobile uchun juda muhim) | 🌟 Yangi |
| `POST` | `/auth/logout` | Tizimdan chiqish (Tokenni qora ro'yxatga kiritish) | Mavjudga asoslangan |

---

## 2. 👨‍💻 Admin Ilovasi uchun API (Admin Mobile App)
*Faqat `Admin` roliga ega foydalanuvchilar kira oladi.*

### 🏢 Kafelar va Egalari (Owners & Cafes)
| Metod | Endpoint | Tavsif | Status |
|---|---|---|---|
| `GET`  | `/admin/dashboard` | **(Tavsiya etiladi)** Admin asosiy oynasi statistikasi (faol kafelar, tushumlar, qarzlar) | 🌟 Yangi |
| `GET`  | `/admin/owners` | Barcha kafe egalari (Owners) ro'yxati | Mavjud |
| `POST` | `/admin/owners` | Yangi kafe egasini ro'yxatdan o'tkazish | Mavjud |
| `GET`  | `/admin/owners/{id}` | Muayyan kafe egasining to'liq ma'lumotlari | Mavjud |
| `POST` | `/admin/owners/{id}/cafes` | Egasi uchun yangi kafe qo'shish | Mavjud |
| `GET`  | `/admin/cafes` | **(Tavsiya etiladi)** To'g'ridan-to'g'ri barcha kafelarni ro'yxatini olish (qidirish/filtr) | 🌟 Yangi |
| `POST` | `/admin/cafes/{id}/toggle-suspend` | Kafeni bloklash (Suspend) yoki blokdan chiqarish | Mavjud |
| `POST` | `/admin/cafes/{id}/extend` | Kafening faoliyat muddatini (oy) uzaytirish | Mavjud |

### 💰 Moliya va Hisob-kitob (Invoices & Balance)
| Metod | Endpoint | Tavsif | Status |
|---|---|---|---|
| `GET`  | `/admin/invoices` | Barcha hisob-fakturalar (Invoices) ro'yxati (To'langan/To'lanmagan) | Mavjud |
| `POST` | `/admin/invoices/generate` | Oylik hisob-fakturalarni ommaviy generatsiya qilish | Mavjud |
| `POST` | `/admin/invoices/{id}/pay` | Muayyan fakturani to'langan deb belgilash | Mavjud |
| `POST` | `/admin/owners/{id}/topup` | Kafe egasining balansini to'ldirish | Mavjud |

### ⚙️ Sozlamalar va Tizim (Settings & Sessions)
| Metod | Endpoint | Tavsif | Status |
|---|---|---|---|
| `GET`  | `/admin/settings` | Tizimning global sozlamalarini olish (narxlar, limitlar) | Mavjud |
| `PUT`  | `/admin/settings` | Tizim sozlamalarini yangilash | Mavjud |
| `GET`  | `/admin/sessions` | Aktiv sessiyalarni ko'rish | Mavjud |
| `DELETE`| `/admin/sessions/{sessionId}` | Muayyan sessiyani to'xtatish (Revoke) | Mavjud |

---

## 3. 🏪 Sotuvchi/Kafe Egasi API (Seller Mobile App)
*Faqat `CafeOwner` roliga ega foydalanuvchilar kira oladi.*

### 📊 Asosiy oyna va Kafelar (Dashboard & Cafes)
| Metod | Endpoint | Tavsif | Status |
|---|---|---|---|
| `GET`  | `/owner/cafes` | Egasi boshqaradigan kafelar ro'yxatini olish | Mavjud |
| `POST` | `/owner/cafes/select/{id}` | Joriy ishlash uchun kafeni tanlash (Session State o'rniga Token claims ga asoslanadi) | Mavjud |
| `GET`  | `/owner/dashboard` | Joriy kafening statistikasi va bugungi ko'rsatkichlar | Mavjud |
| `GET`  | `/owner/profile` | **(Tavsiya etiladi)** Shaxsiy profil ma'lumotlarini olish | 🌟 Yangi |

### 🍔 Taomnoma (Products & Menu)
| Metod | Endpoint | Tavsif | Status |
|---|---|---|---|
| `GET`  | `/owner/products` | Kafening mahsulotlar ro'yxati | Mavjud |
| `POST` | `/owner/products` | Yangi mahsulot qo'shish (Rasmi bilan `multipart/form-data`) | Mavjud |
| `PUT`  | `/owner/products/{id}` | Mahsulotni tahrirlash | Mavjud |
| `DELETE`| `/owner/products/{id}` | Mahsulotni o'chirish | Mavjud |
| `POST` | `/owner/products/from-template` | Tizim shablonidan mahsulot qo'shish | Mavjud |
| `POST` | `/owner/products/{id}/image` | **(Tavsiya etiladi)** Mahsulot rasmini alohida yuklash | 🌟 Yangi |

### 🎁 Bonus Kampaniyalar (Bonus Campaigns)
| Metod | Endpoint | Tavsif | Status |
|---|---|---|---|
| `GET`  | `/owner/bonus-campaigns` | Barcha bonus kampaniyalari | Mavjud |
| `POST` | `/owner/bonus-campaigns` | Yangi bonus kampaniyasi yaratish | Mavjud |
| `PUT`  | `/owner/bonus-campaigns/{id}/toggle` | Kampaniyani faol/nofaol qilish | Mavjud |
| `DELETE`| `/owner/bonus-campaigns/{id}` | Kampaniyani o'chirish | Mavjud |

### 🧑‍🤝‍🧑 Mijozlarga Xizmat Ko'rsatish (Queue & Serving)
| Metod | Endpoint | Tavsif | Status |
|---|---|---|---|
| `GET`  | `/owner/queue` | Hozirda kafeda navbat kutayotgan mijozlar (QR skaner qilganlar) | Mavjud |
| `GET`  | `/owner/queue/{id}` | **(Tavsiya etiladi)** Kutayotgan bitta mijoz haqida batafsil ma'lumot | 🌟 Yangi |
| `POST` | `/owner/queue/{id}/serve` | Mijozni xizmat ko'rsatishga qabul qilish | Mavjud |
| `POST` | `/owner/queue/{id}/issue-bonus` | Mijozga hisob bo'yicha bonus yozish / bonusini ishlatish | Mavjud |
| `GET`  | `/owner/history` | Bonus berilish tarixi (Tranzaksiyalar) | Mavjud |
| `GET`  | `/owner/customers` | Ushbu kafening doimiy mijozlari ro'yxati | Mavjud |

### 💳 Invoices & Settings
| Metod | Endpoint | Tavsif | Status |
|---|---|---|---|
| `GET`  | `/owner/invoices` | Kafe egasining o'z hisob-fakturalari (Admin yuborgan) | Mavjud |
| `POST` | `/owner/invoices/{id}/upload-receipt` | To'lov kvitansiyasini yuklash | Mavjud |
| `GET`  | `/owner/settings` | Kafega tegishli sozlamalar (Bot token va boshqalar) | Mavjud |
| `PUT`  | `/owner/settings/bot-token` | Telegram bot tokenini yangilash | Mavjud |

---

## 4. 📱 Mijozlar (Customer / Public API)
*Autentifikatsiya talab etilmaydi (yoki Customer JWT bilan kiradi).*

| Metod | Endpoint | Tavsif | Status |
|---|---|---|---|
| `GET`  | `/menu/cafe/{cafeId}` | Kafening taomnomasini ko'rish | Mavjud |
| `GET`  | `/menu/products/{id}` | Mahsulot batafsil ma'lumoti | Mavjud |
| `POST` | `/bonus/queue/create` | QR kod yaratish uchun navbatga so'rov | Mavjud |
| `POST` | `/bonus/queue/{id}/activate` | Telegram/App orqali mijoz o'zini identifikatsiya qilib navbatga kirishi | Mavjud |
| `POST` | `/bonus/my-bonuses` | Mijoz o'zining yig'ilgan bonuslarini ko'rishi | Mavjud |

---

## 5. 🔔 Bildirishnomalar (Push Notifications - Tavsiya)
*Foydalanuvchilarning telefonlariga Push xabarlar yuborish uchun (Firebase Cloud Messaging - FCM bilan).*

| Metod | Endpoint | Tavsif | Status |
|---|---|---|---|
| `POST` | `/notifications/register-token` | Qurilma (FCM) tokenini serverga saqlash (Yangiliklar va Statuslar kelishi uchun) | 🌟 Yangi |
| `DELETE`| `/notifications/remove-token` | Qurilma tokenini o'chirish (Logout qilinganda) | 🌟 Yangi |

---

## 🔄 Arxitekturani O'zgartirish (MVC dan API ga o'tish refaktoringi)
**API'ni qurishda amalga oshiriladigan ishlar:**
1. Hozirgi `Controllers` papkasi o'rniga API uchun alohida `ApiControllers` yoki barcha kontrollerlarga `[ApiController]` va `[Route("api/v1/[controller]")]` atributi qo'shiladi.
2. Form-data orqali qabul qilinayotgan `IActionResult` lar o'rniga `[FromBody]` bilan DTO'lar qabul qilinadi.
3. Kuki (Cookie) orqali avtorizatsiya qilingan joylar **JWT (JSON Web Token)** orqali autentifikatsiyaga o'tkaziladi.
4. Javoblar (Responses) standartlashtiriladi: `{ "success": true, "data": { ... }, "message": "" }` formatida.
