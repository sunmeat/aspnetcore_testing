using FluentAssertions;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Soccer.Domain.Entities;
using Soccer.Infrastructure.Repositories;

namespace Soccer.IntegrationTests.Repositories
{
    public class TeamRepositoryTests
    {
        private readonly TeamRepository repository;

        public TeamRepositoryTests()
        {
            // визначаємо шлях до конфігурації solution
            string solutionPath = Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "..",
                    "..",
                    "..",
                    ".."));

            // завантажуємо конфігурацію з appsettings.json
            IConfiguration configuration =
                new ConfigurationBuilder()
                    .SetBasePath(solutionPath)
                    .AddJsonFile("appsettings.json")
                    .Build();

            // отримуємо назву Firebase проєкту з конфігурації
            string projectId =
                configuration["Firebase:ProjectId"]
                ?? throw new InvalidOperationException(
                    "Firebase:ProjectId не задано в appsettings.json.");

            // визначаємо шлях до Firebase service account
            string firebasePath = Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "..",
                    "..",
                    "..",
                    "..",
                    "Soccer.Infrastructure",
                    "RealFirebase",
                    "firebase.json"));

            // створюємо credentials з Firebase service account
            GoogleCredential credential =
                CredentialFactory
                    .FromFile<ServiceAccountCredential>(firebasePath)
                    .ToGoogleCredential();

            // створюємо реальне підключення до Firestore
            FirestoreDb db = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                Credential = credential
            }.Build();

            // створюємо справжній Firestore repository
            repository = new TeamRepository(db);
        }

        [Fact]
        public async Task GetAll_ShouldReturnTeams()
        {
            // отримуємо команди безпосередньо з Firestore
            IEnumerable<Team> result =
                await repository.GetAll();

            // перевіряємо, що результат отримано
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Get_ShouldReturnTeam_WhenTeamExists()
        {
            // отримуємо наявні команди
            IEnumerable<Team> teams =
                await repository.GetAll();

            // перевіряємо, що тестові дані існують
            teams.Should().NotBeEmpty();

            Team expected = teams.First();

            // отримуємо цю саму команду за ідентифікатором
            Team? result =
                await repository.Get(expected.Id);

            // перевіряємо отримані дані
            result.Should().NotBeNull();
            result!.Id.Should().Be(expected.Id);
            result.Name.Should().Be(expected.Name);
            result.Coach.Should().Be(expected.Coach);
        }

        [Fact]
        public async Task Get_ShouldReturnNull_WhenTeamDoesNotExist()
        {
            // запитуємо команду, якої немає
            Team? result =
                await repository.Get(999999);

            // repository повинен повернути null
            result.Should().BeNull();
        }
    }
}
