# CurrencyRates — Модуль курсів валют НБУ

Веб-застосунок для автоматичної синхронізації та відображення курсів валют Національного банку України.

## Зміст

- [Технологічний стек](#технологічний-стек)
- [Архітектура](#архітектура)
- [Структура проєкту](#структура-проєкту)
- [Вимоги](#вимоги)
- [Як запустити](#як-запустити)
- [НБУ API](#нбу-api)
- [Стратегія синхронізації](#стратегія-синхронізації)
- [База даних](#база-даних)
- [Логування](#логування)
- [Edge Cases](#edge-cases)

---

## Технологічний стек

| Компонент | Технологія |
|---|---|
| Backend | ASP.NET Core 9.0 Web API |
| Frontend | Blazor WebAssembly + Radzen Blazor |
| База даних | Microsoft SQL Server |
| ORM (міграції) | Entity Framework Core 9 |
| Запити до БД | SqlKata |
| HTTP клієнт | HttpClient |
| Retry політика | Polly |
| Логування | Serilog |

---

## Архітектура

Проєкт побудований за принципами **Clean Architecture** та **SOLID**:

```
CurrencyRates.Domain          ← Entities, Interfaces, Enums (без залежностей)
CurrencyRates.Application     ← Бізнес-логіка, сервіси, DTOs
CurrencyRates.Infrastructure  ← SqlKata, HttpClient, EF Core, Serilog
CurrencyRates.API             ← ASP.NET Core Web API (точка входу)
CurrencyRates.Client          ← Blazor WASM + Radzen UI
```

Кожен шар залежить тільки від шару нижче:
```
Client → API → Infrastructure → Application → Domain
```

---

## Структура проєкту

```
CurrencyRates/
├── CurrencyRates.Domain/
│   ├── Entities/
│   │   ├── Currency.cs          # Довідник валют
│   │   └── CurrencyRate.cs      # Курс валюти на дату
│   ├── Enums/
│   │   └── SourceType.cs        # Auto / Manual
│   └── Interfaces/
│       ├── ICurrencyRateRepository.cs
│       ├── ICurrencyRateService.cs
│       └── INbuApiClient.cs
│
├── CurrencyRates.Application/
│   ├── DTOs/
│   │   └── CurrencyRateDto.cs
│   ├── Options/
│   │   └── CurrencyOptions.cs   # Налаштування з appsettings
│   └── Services/
│       ├── CurrencyRateService.cs
│       └── AutoSyncHostedService.cs
│
├── CurrencyRates.Infrastructure/
│   ├── Data/
│   │   └── AppDbContext.cs      # EF Core (тільки для міграцій)
│   ├── Repositories/
│   │   └── CurrencyRateRepository.cs  # SqlKata
│   ├── ExternalServices/
│   │   ├── NbuApiClient.cs
│   │   └── NbuRateDto.cs
│   └── Extensions/
│       └── InfrastructureServiceExtensions.cs
│
├── CurrencyRates.API/
│   ├── Controllers/
│   │   └── CurrencyRatesController.cs
│   ├── appsettings.json
│   └── Program.cs
│
└── CurrencyRates.Client/
    ├── Pages/
    │   └── Home.razor           # Головна сторінка
    ├── Services/
    │   └── CurrencyRatesApiClient.cs
    └── wwwroot/
        └── appsettings.json     # Адреса API
```

---

## Вимоги

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Microsoft SQL Server (локальний або Express)
- Доступ до інтернету для запитів до НБУ API

---

## Як запустити

### 1. Клонуй репозиторій

```bash
git clone https://github.com/YOUR_USERNAME/CurrencyRates.git
cd CurrencyRates
docker compose up --build -d
```
Якщо немає докера, тоді наступні кроки

### 2. Налаштуй підключення до БД

Відкрий `CurrencyRates.API/appsettings.json` і заміни connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=CurrencyRatesDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Приклади `YOUR_SERVER`:
- Локальний SQL Server: `localhost`
- SQL Server Express: `.\SQLEXPRESS` або `localhost\SQLEXPRESS`

### 3. Налаштуй адресу API в клієнті

Відкрий `CurrencyRates.Client/wwwroot/appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:PORT/"
  }
}
```

Де `PORT` — порт API з `CurrencyRates.API/Properties/launchSettings.json`.

### 4. Налаштуй CORS

В `CurrencyRates.API/appsettings.json` вкажи порт клієнта:

```json
"AllowedOrigins": ["https://localhost:CLIENT_PORT", "http://localhost:CLIENT_PORT"]
```

### 5. Встанови dotnet-ef (один раз)

```bash
dotnet tool install --global dotnet-ef
```

### 6. Застосуй міграції

```bash
dotnet ef database update --project CurrencyRates.Infrastructure --startup-project CurrencyRates.API
```

При першому запуску автоматично створяться таблиці та заповниться довідник валют (USD, EUR, DKK, PLN).

### 7. Запусти застосунок

Відкрий два термінали:

**Термінал 1 — API:**
```bash
dotnet run --project CurrencyRates.API
```

**Термінал 2 — Client:**
```bash
dotnet run --project CurrencyRates.Client
```

### 8. Відкрий в браузері

| Сервіс | URL |
|---|---|
| Клієнт (UI) | https://localhost:CLIENT_PORT |
| Swagger (API документація) | https://localhost:API_PORT/swagger |

---

## НБУ API

Використовується офіційний API Національного банку України:

```
GET https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange
    ?valcode=USD
    &date=20250127
    &json
```

**Параметри:**
- `valcode` — код валюти (USD, EUR, DKK, PLN)
- `date` — дата у форматі `YYYYMMDD`
- `json` — формат відповіді

**Приклад відповіді:**
```json
[
  {
    "r030": 840,
    "txt": "Долар США",
    "rate": 41.5,
    "cc": "USD",
    "exchangedate": "27.01.2025"
  }
]
```

Запити виконуються **окремо для кожної валюти** — це дозволяє зберегти курси тих валют які прийшли, навіть якщо одна з них недоступна.

---

## Стратегія синхронізації

### Чому о 16:00?

НБУ публікує офіційні курси щоденно приблизно о **15:00–15:30** київського часу. Синхронізація о **16:00** дає 30-хвилинний запас щоб дані точно з'явились на сервері НБУ.

### Графік:

| Подія | Час |
|---|---|
| Старт застосунку | Одразу перевіряє чи є курс на сьогодні. Якщо немає — завантажує |
| Щоденна синхронізація | 16:00 за місцевим часом |

Час синхронізації можна змінити в `appsettings.json` без зміни коду:
```json
"CurrencySettings": {
  "DailySyncTime": "16:00"
}
```

### Retry політика (Polly)

При недоступності НБУ виконується **3 повторних спроби** з експоненційною затримкою:

| Спроба | Затримка |
|---|---|
| 1 | 30 секунд |
| 2 | 60 секунд |
| 3 | 120 секунд |

Загальний час очікування до 3.5 хвилин — достатньо для тимчасових мережевих проблем, але не занадто довго щоб блокувати роботу.

### Додавання нової валюти

Достатньо додати в `appsettings.json`:
```json
"SupportedCurrencies": {
  "USD": "Долар США",
  "EUR": "Євро",
  "DKK": "Данська крона",
  "PLN": "Польський злотий",
  "GBP": "Британський фунт"
}
```
При наступному запуску застосунок автоматично додасть валюту в довідник БД і завантажить курс.

---

## База даних

Схема відповідає **третій нормальній формі (3NF)**:

### Таблиця `Currencies` (довідник)

| Колонка | Тип | Опис |
|---|---|---|
| Id | int | Первинний ключ |
| Code | nvarchar(10) | Код валюти (USD, EUR...) |
| Name | nvarchar(100) | Назва українською |

Унікальний індекс: `Code`

### Таблиця `CurrencyRates` (курси)

| Колонка | Тип | Опис |
|---|---|---|
| Id | int | Первинний ключ |
| CurrencyId | int | FK → Currencies.Id |
| Rate | decimal(18,4) | Курс відносно UAH |
| RateDate | date | Дата курсу |
| Source | nvarchar(10) | Auto або Manual |
| CreatedAt | datetime | Час запису в БД |

Унікальний індекс: `(CurrencyId, RateDate)` — гарантує відсутність дублікатів.

**Пояснення Source:**
- `Auto` — завантажено автоматично по розкладу о 16:00
- `Manual` — завантажено за запитом користувача через UI

---

## Логування

Логи записуються в двох місцях одночасно:

- **Консоль** — для розробки
- **Файл** — `logs/app-YYYYMMDD.log` з щоденною ротацією

Приклад логів:
```
2025-01-27 12:30:00 [INF] Початок автоматичної синхронізації на дату 2025-01-27
2025-01-27 12:30:01 [INF] Запит до НБУ: ?valcode=USD&date=20250127&json
2025-01-27 12:30:01 [INF] Отримано курс USD = 41.5000 на 2025-01-27
2025-01-27 12:30:02 [INF] Збережено курс EUR на 2025-01-27
2025-01-27 12:30:02 [WRN] НБУ не повернув курс для DKK на дату 2025-01-27
2025-01-27 12:30:03 [INF] Автоматична синхронізація завершена на дату 2025-01-27
```

---

## Edge Cases

| Ситуація | Поведінка |
|---|---|
| НБУ недоступний при старті | Застосунок запускається, помилка пишеться в лог |
| НБУ недоступний при запиті користувача | Показується повідомлення про помилку в UI |
| Курс вже є в БД | Не дублюється (перевірка перед INSERT) |
| Одна з валют не повернулась від НБУ | Решта зберігаються, відсутня пишеться в лог |
| Нова валюта додана в appsettings | Автоматично додається в довідник БД при наступному запуску |
| Запит на майбутню дату | НБУ повертає порожню відповідь |
| Міграції при старті | Застосовуються автоматично, повторний запуск — без змін |
