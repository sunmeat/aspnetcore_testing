using FluentAssertions;
using NSubstitute;
using Soccer.Application.DTO;
using Soccer.Application.Services;
using Soccer.Common.Exceptions;
using Soccer.Domain.Entities;
using Soccer.Domain.Interfaces;

namespace Soccer.UnitTests.Application
{
    public class TeamServiceTests
    {
        private readonly IRepository<Team> teams;
        private readonly TeamService service;

        public TeamServiceTests()
        {
            teams = Substitute.For<IRepository<Team>>();

            service = new TeamService(teams);
        }

        [Fact]
        public async Task Create_ShouldCreateTeam()
        {
            // створюємо DTO команди
            var teamDto = new TeamDTO
            {
                Id = 1,
                Name = "Barcelona",
                Coach = "Coach 1"
            };

            await service.Create(teamDto);

            // перевіряємо, що DTO правильно перетворено на entity
            await teams.Received(1).Create(
                Arg.Is<Team>(team =>
                    team.Id == teamDto.Id &&
                    team.Name == teamDto.Name &&
                    team.Coach == teamDto.Coach));
        }

        [Fact]
        public async Task Update_ShouldUpdateTeam()
        {
            // створюємо DTO для оновлення
            var teamDto = new TeamDTO
            {
                Id = 1,
                Name = "Real Madrid",
                Coach = "Coach 2"
            };

            await service.Update(teamDto);

            // перевіряємо передані в репозиторій дані
            await teams.Received(1).Update(
                Arg.Is<Team>(team =>
                    team.Id == teamDto.Id &&
                    team.Name == teamDto.Name &&
                    team.Coach == teamDto.Coach));
        }

        [Fact]
        public async Task Delete_ShouldDeleteTeam()
        {
            // видаляємо команду
            await service.Delete(10);

            // перевіряємо переданий ідентифікатор
            await teams.Received(1).Delete(10);
        }

        [Fact]
        public async Task Get_ShouldReturnTeam()
        {
            // створюємо команду в тестових даних
            var team = new Team
            {
                Id = 10,
                Name = "Barcelona",
                Coach = "Coach 1"
            };

            teams.Get(10).Returns(team);

            var result = await service.Get(10);

            // перевіряємо, що entity правильно перетворено на DTO
            result.Should().BeEquivalentTo(new TeamDTO
            {
                Id = 10,
                Name = "Barcelona",
                Coach = "Coach 1"
            });
        }

        [Fact]
        public async Task Get_ShouldThrowValidationException_WhenTeamDoesNotExist()
        {
            // налаштовуємо репозиторій на відсутню команду
            teams.Get(100).Returns((Team?)null);

            var action = async () => await service.Get(100);

            // перевіряємо тип і текст винятку
            await action.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Немає такого клуба!");
        }

        [Fact]
        public async Task GetAll_ShouldReturnTeams()
        {
            // створюємо тестовий список команд
            var teamsList = new List<Team>
            {
                new Team
                {
                    Id = 1,
                    Name = "Barcelona",
                    Coach = "Coach 1"
                },
                new Team
                {
                    Id = 2,
                    Name = "Real Madrid",
                    Coach = "Coach 2"
                }
            };

            teams.GetAll().Returns(teamsList);

            var result = (await service.GetAll()).ToList();

            // перевіряємо кількість команд
            result.Should().HaveCount(2);

            // перевіряємо першу команду
            result[0].Should().BeEquivalentTo(new TeamDTO
            {
                Id = 1,
                Name = "Barcelona",
                Coach = "Coach 1"
            });

            // перевіряємо другу команду
            result[1].Should().BeEquivalentTo(new TeamDTO
            {
                Id = 2,
                Name = "Real Madrid",
                Coach = "Coach 2"
            });
        }

        [Fact]
        public async Task GetAll_ShouldReturnEmptyCollection_WhenThereAreNoTeams()
        {
            // репозиторій не містить жодної команди
            teams.GetAll().Returns([]);

            var result = await service.GetAll();

            // порожній результат є коректною поведінкою
            result.Should().BeEmpty();
        }

        // ============================================================================
        // навмисно неправильні тести для демонстрації помилок:

        [Fact]
        public async Task Get_ShouldReturnWrongTeamName()
        {
            // створюємо команду в тестових даних
            var team = new Team
            {
                Id = 10,
                Name = "Barcelona",
                Coach = "Coach 1"
            };

            teams.Get(10).Returns(team);

            var result = await service.Get(10);

            // навмисно неправильне очікування
            result.Name.Should().Be("Real Madrid");
        }

        [Fact]
        public async Task Get_ShouldReturnWrongCoach()
        {
            // створюємо команду в тестових даних
            var team = new Team
            {
                Id = 10,
                Name = "Barcelona",
                Coach = "Coach 1"
            };

            teams.Get(10).Returns(team);

            var result = await service.Get(10);

            // навмисно неправильне очікування
            result.Coach.Should().Be("Coach 2");
        }

        [Fact]
        public async Task GetAll_ShouldReturnThreeTeams()
        {
            // створюємо дві команди
            var teamsList = new List<Team>
            {
                new Team
                {
                    Id = 1,
                    Name = "Barcelona",
                    Coach = "Coach 1"
                },
                new Team
                {
                    Id = 2,
                    Name = "Real Madrid",
                    Coach = "Coach 2"
                }
            };

            teams.GetAll().Returns(teamsList);

            var result = (await service.GetAll()).ToList();

            // навмисно неправильне очікування
            result.Should().HaveCount(3);
        }
    }
}