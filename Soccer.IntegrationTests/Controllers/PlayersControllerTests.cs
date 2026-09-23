using FluentAssertions;       // dotnet add package FluentAssertions
using Google.Cloud.Firestore; // dotnet add package Google.Cloud.Firestore
using Soccer.Application.DTO;
using Soccer.Domain.Entities;
using Soccer.Infrastructure.Repositories;
using Soccer.IntegrationTests.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace Soccer.IntegrationTests.Controllers
{
    public class PlayersControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient client;

        public PlayersControllerTests(CustomWebApplicationFactory factory)
        {
            // створюємо HTTP-клієнт для тестового ASP.NET Core застосунку
            client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ShouldReturnOk()
        {
            // надсилаємо реальний HTTP-запит до API
            HttpResponseMessage response =
                await client.GetAsync("/api/players");

            // перевіряємо HTTP-статус відповіді
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_ShouldReturnPlayer_WhenPlayerExists()
        {
            // спочатку отримуємо список гравців з Firestore
            List<PlayerDTO>? players =
                await client.GetFromJsonAsync<List<PlayerDTO>>(
                    "/api/players");

            // перевіряємо, що в базі є хоча б один гравець
            players.Should().NotBeNull();
            players.Should().NotBeEmpty();

            PlayerDTO player = players!.First();

            // запитуємо конкретного гравця через API
            HttpResponseMessage response =
                await client.GetAsync(
                    $"/api/players/{player.Id}");

            // перевіряємо статус відповіді
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // читаємо DTO з HTTP-відповіді
            PlayerDTO? result =
                await response.Content
                    .ReadFromJsonAsync<PlayerDTO>();

            // перевіряємо, що повернувся саме цей гравець
            result.Should().NotBeNull();
            result!.Id.Should().Be(player.Id);
            result.Name.Should().Be(player.Name);
            result.Age.Should().Be(player.Age);
            result.Position.Should().Be(player.Position);
        }

        [Fact]
        public async Task Get_ShouldReturnNotFound_WhenPlayerDoesNotExist()
        {
            // використовуємо ідентифікатор, якого немає серед тестових даних
            HttpResponseMessage response =
                await client.GetAsync("/api/players/999999");

            // сервіс повинен перетворити ValidationException на 404
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // перевіряємо повідомлення, яке повернув контролер
            string message =
                await response.Content.ReadAsStringAsync();

            message.Should().Contain("Немає такого гравця!");
        }

        // ===============================================================================================
        // !!! тестовий метод, який показує, що ми реально звертаємось до Firestore і створюємо нового гравця:
        [Fact]
        public async Task Post_ShouldCreatePlayer()
        {
            // створюємо тестового гравця
            PlayerDTO player = new PlayerDTO
            {
                Name = "Integration Test Player",
                Age = 25,
                Position = "Test",
                TeamId = null
            };

            // надсилаємо реальний HTTP POST-запит до API
            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    "/api/players",
                    player);

            // контролер повинен успішно створити гравця
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // повторно отримуємо список гравців з Firestore
            List<PlayerDTO>? players =
                await client.GetFromJsonAsync<List<PlayerDTO>>(
                    "/api/players");

            // перевіряємо, що створений гравець реально з'явився в БД
            players.Should().NotBeNull();

            players.Should().Contain(player =>
                player.Name == "Integration Test Player" &&
                player.Age == 25 &&
                player.Position == "Test");
        }

        // звертатися під час інтеграційних тестів до реальної робочої бази даних не варто,
        // оскільки тести можуть не лише читати, а й змінювати або видаляти реальні дані.
        // крім того, результати тестів стають залежними від поточного стану бази:
        // зміна або видалення даних користувачем може призвести до випадкового падіння тестів.
        // це також ускладнює повторюваність тестування та може створювати проблеми при паралельному запуску тестів.

        // тому, є три альтернативні варіанти:
        // 1) окремий Firebase/Firestore проєкт тільки для тестів
        // - найпростіший і найбезпечніший варіант, якщо треба перевірити роботу з реальною Firestore.
        // 2) емулятор Firestore
        // - тести працюють із Firestore API, але дані фізично залишаються локальними
        // 3) підмінити Infrastructure у WebApplicationFactory
        // - тоді HTTP і контролери справжні, але замість реального Firestore використовується тестове сховище/mock.
        // щоб не нахлобучити, це рішення виніс в окремий гіст: https://gist.github.com/sunmeat/ec49c340abd8b3d3a6040d903a2c0555
        // там новий Program.cs, та налаштування WebApplicationFactory, які підміняють PlayerRepository на тестовий FakePlayerRepository.
        
        
        // підключення до локального Firestore Emulator:
        [Fact]
        public async Task Create_ShouldAddPlayer()
        {
            // одноразово встановити Firebase CLI (Command Line Interface)
            // npm install -g firebase-tools

            // перевірити встановлення:
            // firebase --version

            // одноразово авторизувати Firebase CLI:
            // firebase login

            // одноразово налаштувати Firestore Emulator:
            // cd Soccer.Infrastructure
            // mkdir FirebaseEmulator
            // cd FirebaseEmulator
            // firebase init emulators

            // перед запуском тестів запускати Firestore Emulator:
            // cd ..
            // firebase emulators:start --only firestore

            // підключаємося до локального Firestore Emulator
            Environment.SetEnvironmentVariable(
                "FIRESTORE_EMULATOR_HOST",
                "127.0.0.1:8080");

            FirestoreDb db = new FirestoreDbBuilder
            {
                ProjectId = "test-project",
                EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOnly // явно вказуємо, що підключаємося до емулятора
            }.Build();

            var repository = new PlayerRepository(db);

            var player = new Player
            {
                Name = "Integration Test Player",
                Age = 25,
                Position = "Test",
                TeamId = null
            };

            // створюємо гравця в локальному Firestore
            await repository.Create(player);

            // читаємо дані з локального Firestore
            IEnumerable<Player> players = await repository.GetAll();

            // перевіряємо, що створений гравець існує
            players.Should().Contain(player =>
                player.Name == "Integration Test Player" &&
                player.Age == 25 &&
                player.Position == "Test");
        }
    }
}