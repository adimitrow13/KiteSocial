# KiteSocial.API

A modern backend API for a kitesurfing social network, built with **ASP.NET Core (.NET 10)**, **Entity Framework Core (SQLite)**, and **JWT Bearer Authentication**.

---

## 🏄 Features & Endpoints

### 1. Authentication (`/api/auth`)
- `POST /api/auth/register` – Регистрация на нов потребител (Email, Username, Password, FullName, SkillLevel).
- `POST /api/auth/login` – Вход с потребителско име или имейл + парола, генериращ JWT токен.
- `GET /api/auth/me` – Връща профилната информация на автентикирания потребител `[Authorize]`.

### 2. Kite Spots (`/api/spots`)
- `GET /api/spots` – Търсене и филтриране на спотове (по държава, ключова дума, тип вода: Flat, Chop, Waves).
- `GET /api/spots/{id}` – Детайли за конкретен спот и брой сесии на него.
- `POST /api/spots` – Добавяне на нов спот с координати и препоръчителни ветрове `[Authorize]`.
- `GET /api/spots/{id}/sessions` – Всички записани сесии на дадения спот.

### 3. Kite Sessions (`/api/sessions`)
- `POST /api/sessions` – Логване на сесия (спот, продължителност, възли вятър, размер крило, тип борд, височина на скок WOO) `[Authorize]`.
- `GET /api/sessions` – Търсене на сесии по спот или потребител.
- `GET /api/sessions/my` – Личните сесии на карача `[Authorize]`.
- `GET /api/sessions/stats` – Обобщена статистика (общ брой сесии, часове във водата, рекорд за най-висок скок, любим спот) `[Authorize]`.
- `DELETE /api/sessions/{id}` – Изтриване на сесия `[Authorize]`.

### 4. Social Feed & Posts (`/api/posts`)
- `GET /api/posts` – Основен социален фийд (пагинация, свързан спот/сесия, брой харесвания и коментари).
- `POST /api/posts` – Публикуване на пост `[Authorize]`.
- `POST /api/posts/{id}/like` – Харесване / премахване на харесване (Like toggle) `[Authorize]`.
- `GET /api/posts/{id}/comments` – Преглед на коментарите към пост.
- `POST /api/posts/{id}/comments` – Добавяне на коментар `[Authorize]`.
- `DELETE /api/posts/{id}` – Изтриване на собствен пост `[Authorize]`.

### 5. Health Check (`/api/health`)
- `GET /api/health` – Проверка на състоянието и версията на услугата.

---

## 🛠️ Architecture & Structure

```
KiteSocial.API/
├── Controllers/         # API контролери (Auth, Spots, Sessions, Posts, Health)
├── Models/
│   └── Entities/        # Домейн субекти (ApplicationUser, Spot, KiteSession, Post, Comment, Like)
├── Data/
│   ├── KiteSocialDbContext.cs  # EF Core контекст с Identity и ограничения
│   └── Migrations/             # EF Core миграции
├── DTOs/                # Request & Response модели (Auth, Spots, Sessions, Posts, Comments)
├── Services/            # Бизнес логика (TokenService за JWT генерация)
├── Program.cs           # Конфигурация на DI, JWT, Identity и Middleware
└── KiteSocial.API.http  # Готови HTTP заявки за цялостно тестване
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run Locally
```bash
dotnet restore
dotnet run
```
API-то стартира на `http://localhost:5067` и `https://localhost:7249`.

### Тестване с готовите заявки
Отворете [KiteSocial.API.http](file:///Users/alexdimitrov/KiteSocial.API/KiteSocial.API.http) във вашия редактор (поддържа се от Visual Studio, VS Code / REST Client и Antigravity) за тестване на целия потребителски поток с един клик.
