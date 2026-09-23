# ASP.NET Core Testing

Навчальний проєкт для демонстрації підходів до тестування ASP.NET Core Web API на прикладі застосунку для роботи з футболістами та командами.

Проєкт побудований з використанням Clean Architecture та містить окремі проєкти для основної логіки застосунку, unit-тестів та integration-тестів.

## Структура проєкту

```text
Soccer
├── Soccer.Domain
│   ├── Entities
│   │   ├── Player
│   │   └── Team
│   └── Interfaces
│
├── Soccer.Application
│   └── Services
│       ├── PlayerService
│       └── TeamService
│
├── Soccer.Infrastructure
│   └── Firestore repositories
│
├── Soccer.WebAPI
│   └── Controllers
│       ├── PlayersController
│       └── TeamsController
│
├── Soccer.UnitTests
│   ├── Services
│   └── Controllers
│
└── Soccer.IntegrationTests
    ├── Controllers
    ├── Fixtures
    └── Fakes
```

## Технології

- .NET 10
- ASP.NET Core Web API
- C#
- xUnit
- FluentAssertions
- NSubstitute
- WebApplicationFactory
- Google Cloud Firestore
- Firebase Emulator Suite

## Unit-тести

Проєкт `Soccer.UnitTests` містить модульні тести окремих компонентів застосунку.

### Тестування сервісів

`PlayerServiceTests` перевіряє:

- створення гравця;
- оновлення гравця;
- отримання гравця;
- отримання списку гравців;
- видалення гравця;
- роботу з командою гравця;
- ситуацію, коли команда відсутня;
- ситуацію, коли гравець не знайдений;
- взаємодію сервісу з репозиторіями.

Для ізоляції сервісу використовуються NSubstitute та тестові замінники `IRepository<Player>` і `IRepository<Team>`.

### Тестування контролерів

`PlayersControllerTests` тестує контролер без запуску HTTP-сервера.

Перевіряються:

- HTTP-результати;
- `OkResult`;
- `NoContentResult`;
- `NotFoundObjectResult`;
- перевірка `ModelState`;
- передача даних до сервісу;
- обробка винятків сервісу.

Для сервісу контролера використовується NSubstitute.

## Integration-тести

Проєкт `Soccer.IntegrationTests` призначений для тестування взаємодії декількох компонентів застосунку.

Для запуску ASP.NET Core застосунку використовується `WebApplicationFactory<Program>`.

### HTTP-тести

Тести контролерів виконують реальні HTTP-запити через `HttpClient` та перевіряють роботу API на рівні HTTP.

Приклад сценарію:

```text
HttpClient
    ↓
ASP.NET Core
    ↓
Controller
    ↓
Service
    ↓
Repository
```

### InMemory репозиторій

У тестах використовується `InMemoryPlayerRepository`, який реалізує `IRepository<Player>`.

Репозиторій зберігає тестові дані в пам'яті та дозволяє виконувати CRUD-операції без підключення до реальної бази даних.

### Firestore Emulator

Для тестування роботи `PlayerRepository` передбачена можливість використання Firebase Firestore Emulator.

Емулятор дозволяє виконувати операції з Firestore локально, без використання реального Firebase-проєкту.

Для запуску емулятора використовується Firebase CLI:

```bash
npm install -g firebase-tools
```

Після встановлення Firebase CLI необхідно запустити Firestore Emulator:

```bash
firebase emulators:start --only firestore
```

У тестах для підключення до локального емулятора використовується:

```csharp
Environment.SetEnvironmentVariable(
    "FIRESTORE_EMULATOR_HOST",
    "127.0.0.1:8080");
```

## Інструменти тестування

### xUnit

Основний framework для написання тестів.

### FluentAssertions

Використовується для більш читабельних перевірок:

```csharp
result.Should().NotBeNull();
result.Name.Should().Be("Test Player");
```

### NSubstitute

Використовується для створення substitute-об'єктів:

```csharp
var repository = Substitute.For<IRepository<Player>>();
```

Наприклад, можна задати результат виклику:

```csharp
repository
    .Get(1)
    .Returns(player);
```

А також перевірити взаємодію:

```csharp
repository.Received(1).Get(1);
```

## Що демонструє проєкт

```text
                    TESTING
                       │
          ┌────────────┴────────────┐
          │                         │
     UNIT TESTS               INTEGRATION TESTS
          │                         │
     ┌────┴────┐              ┌─────┴─────┐
     │         │              │           │
  Services  Controllers      HTTP      Firestore
     │         │              │           │
 NSubstitute NSubstitute   WebApplicationFactory
 FluentAssertions            InMemory / Emulator
```

Проєкт показує різницю між модульним та інтеграційним тестуванням, а також різні способи ізоляції залежностей під час тестів.

## Запуск тестів

У Visual Studio тести можна запускати через Test Explorer.

Або з командного рядка:

```bash
dotnet test
```

Для окремого проєкту:

```bash
dotnet test Soccer.UnitTests
```

або:

```bash
dotnet test Soccer.IntegrationTests
```

## Призначення

Проєкт створений як практичний приклад тестування ASP.NET Core застосунку та може використовуватися як навчальний матеріал для вивчення:

- unit-тестування;
- integration-тестування;
- тестування ASP.NET Core контролерів;
- mock/substitute-об'єктів;
- WebApplicationFactory;
- тестових репозиторіїв;
- Firestore Emulator.
