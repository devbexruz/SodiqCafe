# Sodiq Cafe - Seller Mobile App Arxitekturasi

Ushbu hujjatda **Sodiq Cafe** sotuvchilar (sellermobile) ilovasi uchun tanlangan **Clean Architecture (Toza Arxitektura)** va **Feature-first** yondashuvi tuzilmasi keltirilgan.

## 📂 Loyiha Tuzilmasi (Directory Structure)

```text
lib/
├── config/                  # Ilova konfiguratsiyalari (Routing, Theme)
│   ├── routes/              # Sahifalarga o'tish yo'llari (GoRouter yoki avtomatik)
│   └── theme/               # Ilova dizayn tizimi (Ranglar, Shriftlar)
├── core/                    # Barcha feature'lar uchun umumiy bo'lgan qismlar
│   ├── constants/           # O'zgarmas qiymatlar (API URL, stringlar, ikonka yo'llari)
│   ├── errors/              # Xatoliklarni boshqarish (Exceptions, Failures)
│   ├── network/             # Tarmoq so'rovlari (Dio/Http interceptorlar)
│   ├── utils/               # Yordamchi funksiyalar (Formatters, Validators)
│   └── usecases/            # Umumiy UseCase interfeyslari
├── di/                      # Dependency Injection (GetIt bilan bog'liqliklar)
├── features/                # Ilova funksionalliklari (Modullar)
│   ├── auth/                # Avtorizatsiya (Login, Parolni tiklash)
│   ├── dashboard/           # Asosiy oyna (Statistika, tezkor amallar)
│   ├── menu/                # Taomnoma (Kategoriyalar, mahsulotlar)
│   └── orders/              # Buyurtmalar (Yangi, jarayondagi, yakunlangan buyurtmalar)
└── main.dart                # Ilovani ishga tushirish nuqtasi
```

## 🧩 Feature Ichki Tuzilmasi

Har bir feature (masalan, `orders`) 3 ta asosiy qatlamdan iborat bo'ladi:

1. **Data Layer (`data/`)** - Ma'lumotlarni olish va saqlash:
   - `datasources/`: API (Remote) yoki Local (SharedPreferences/Hive) dan ma'lumot oluvchi klasslar.
   - `models/`: JSON'dan obyektga o'tkazuvchi modellar (DTOs).
   - `repositories/`: Domain qatlamidagi repository interfeyslarining implementatsiyasi.

2. **Domain Layer (`domain/`)** - Biznes mantiq (Eng muhim qism, boshqa qatlamlarga bog'liq bo'lmaydi):
   - `entities/`: Asosiy ma'lumot obyektlari (sof Dart klasslari).
   - `repositories/`: Ma'lumot olish uchun abstrakt interfeyslar.
   - `usecases/`: Bitta aniq amalni bajaruvchi klasslar (masalan, `GetOrdersUseCase`).

3. **Presentation Layer (`presentation/`)** - UI (Foydalanuvchi interfeysi):
   - `bloc/` (yoki `cubit` / `getx`): State management (Holatni boshqarish).
   - `pages/`: To'liq sahifalar (Ekranlar).
   - `widgets/`: Shu sahifaga tegishli bo'lgan mayda UI komponentlar (Tugmalar, kartochkalar).

## 🛠 Asosiy Texnologiyalar va Paketlar (Reja)
- **Tarmoq (Network):** `dio`
- **State Management:** BLoC / Cubit (yoki sizning xohishingizga ko'ra GetX/Riverpod)
- **Dependency Injection:** `get_it`
- **Local Storage:** `shared_preferences` yoki `hive`
- **Funksional Dasturlash (Error Handling):** `dartz` (Either uchun)
- **Tenglikni tekshirish:** `equatable`
- **Routing:** `go_router` (yoki `auto_route`)
