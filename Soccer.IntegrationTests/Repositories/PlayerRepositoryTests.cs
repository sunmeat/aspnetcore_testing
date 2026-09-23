using FluentAssertions;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Soccer.Domain.Entities;
using Soccer.Infrastructure.Repositories;

namespace Soccer.IntegrationTests.Repositories
{
    public class PlayerRepositoryTests
    {
        private readonly PlayerRepository repository;

        public PlayerRepositoryTests()
        {
            // визначаємо шлях до Firebase service account
            string firebasePath = Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "..",
                    "..",
                    "..",
                    "..",
                    "Soccer.Infrastructure",
                    "firebase.json"));

            // створюємо credentials з Firebase service account
            GoogleCredential credential =
                CredentialFactory
                    .FromFile<ServiceAccountCredential>(firebasePath)
                    .ToGoogleCredential();

            // створюємо реальне підключення до Firestore
            FirestoreDb db = new FirestoreDbBuilder
            {
                ProjectId = "alex-odesa",
                Credential = credential
            }.Build();

            // створюємо справжній Firestore repository
            repository = new PlayerRepository(db);
        }

        [Fact]
        public async Task GetAll_ShouldReturnPlayers()
        {
            // отримуємо гравців безпосередньо з Firestore
            IEnumerable<Player> result =
                await repository.GetAll();

            // перевіряємо, що результат отримано
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Get_ShouldReturnPlayer_WhenPlayerExists()
        {
            // спочатку отримуємо наявних гравців
            IEnumerable<Player> players =
                await repository.GetAll();

            // перевіряємо, що тестові дані існують
            players.Should().NotBeEmpty();

            Player expected = players.First();

            // читаємо цього самого гравця за ідентифікатором
            Player? result =
                await repository.Get(expected.Id);

            // перевіряємо отримані дані
            result.Should().NotBeNull();
            result!.Id.Should().Be(expected.Id);
            result.Name.Should().Be(expected.Name);
            result.Age.Should().Be(expected.Age);
            result.Position.Should().Be(expected.Position);
        }

        [Fact]
        public async Task Get_ShouldReturnNull_WhenPlayerDoesNotExist()
        {
            // запитуємо гравця, якого гарантовано немає
            Player? result =
                await repository.Get(999999);

            // repository повинен повернути null
            result.Should().BeNull();
        }
    }
}