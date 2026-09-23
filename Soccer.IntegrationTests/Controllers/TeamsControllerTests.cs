using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Soccer.Application.DTO;
using Soccer.IntegrationTests.Fixtures;

namespace Soccer.IntegrationTests.Controllers
{
    public class TeamsControllerTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient client;

        public TeamsControllerTests(
            CustomWebApplicationFactory factory)
        {
            // створюємо HTTP-клієнт для тестового ASP.NET Core застосунку
            client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ShouldReturnOk()
        {
            // надсилаємо реальний HTTP-запит до API
            HttpResponseMessage response =
                await client.GetAsync("/api/teams");

            // перевіряємо HTTP-статус відповіді
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Get_ShouldReturnTeam_WhenTeamExists()
        {
            // отримуємо список команд з Firestore через API
            List<TeamDTO>? teams =
                await client.GetFromJsonAsync<List<TeamDTO>>(
                    "/api/teams");

            // перевіряємо, що в базі є хоча б одна команда
            teams.Should().NotBeNull();
            teams.Should().NotBeEmpty();

            TeamDTO team = teams!.First();

            // запитуємо конкретну команду
            HttpResponseMessage response =
                await client.GetAsync(
                    $"/api/teams/{team.Id}");

            // перевіряємо статус відповіді
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // читаємо DTO з HTTP-відповіді
            TeamDTO? result =
                await response.Content
                    .ReadFromJsonAsync<TeamDTO>();

            // перевіряємо дані отриманої команди
            result.Should().NotBeNull();
            result!.Id.Should().Be(team.Id);
            result.Name.Should().Be(team.Name);
            result.Coach.Should().Be(team.Coach);
        }

        [Fact]
        public async Task Get_ShouldReturnNotFound_WhenTeamDoesNotExist()
        {
            // використовуємо ідентифікатор, якого немає в Firestore
            HttpResponseMessage response =
                await client.GetAsync("/api/teams/999999");

            // контролер повинен повернути 404
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // перевіряємо текст помилки
            string message =
                await response.Content.ReadAsStringAsync();

            message.Should().Contain("Немає такого клуба!");
        }
    }
}