using FluentAssertions; // dotnet add package FluentAssertions - бібліотека для зручної перевірки результатів тестів
using NSubstitute;      // dotnet add package NSubstitute - бібліотека для створення підставних об'єктів (mocking) у тестах
using Soccer.Application.DTO;
using Soccer.Application.Services;
using Soccer.Common.Exceptions;
using Soccer.Domain.Entities;
using Soccer.Domain.Interfaces;

namespace Soccer.UnitTests.Application
{
    public class PlayerServiceTests
    {
        private readonly IRepository<Player> players;
        private readonly IRepository<Team> teams;
        private readonly PlayerService service;

        // xUnit створює новий екземпляр PlayerServiceTests для кожного тесту,
        // конструктор виконується перед кожним [Fact] або кожним запуском [Theory]
        public PlayerServiceTests()
        {
            players = Substitute.For<IRepository<Player>>(); // створюється підставний репозиторій, який NSubstitute реалізує динамічно
            teams = Substitute.For<IRepository<Team>>();

            service = new PlayerService(players, teams); 
        }

        public void Dispose()
        {
            // виконується після кожного тесту, можна використовувати для очищення ресурсів, якщо потрібно
        }

        // всі тести нижче перевіряють,
        // чи правильно працює логіка PlayerService (незалежно від Firestore)

        [Fact] // атрибут, який позначає метод як тестовий
        public async Task Create_ShouldCreatePlayer() // назва методу тесту описує, що саме перевіряється
        {
            // створюється DTO, який передається в сервіс
            var playerDto = new PlayerDTO
            {
                Id = 1,
                Name = "Lionel Messi",
                Age = 39,
                Position = "Forward",
                TeamId = 10
            };

            await service.Create(playerDto);

            // перевірка, що метод Create був викликаний рівно один раз із об’єктом Player,
            // усі властивості якого (Id, Name, Age, Position, TeamId) відповідають даним із playerDto
            await players.Received(1).Create(
                Arg.Is<Player>(player =>
                    player.Id == playerDto.Id &&
                    player.Name == playerDto.Name &&
                    player.Age == playerDto.Age &&
                    player.Position == playerDto.Position &&
                    player.TeamId == playerDto.TeamId));

            /*
             >> NSubstitute методи:
                Substitute.For<T>() - створює підставний об'єкт типу T
                Returns(value) - задає значення, яке повертає підставний метод
                Returns(_ => value) - задає значення через функцію
                Arg.Any<T>() - задає будь-яке значення типу T
                Arg.Is<T>(predicate) - перевіряє аргумент за заданою умовою
                Received() - перевіряє, що метод був викликаний
                Received(1) - перевіряє, що метод був викликаний один раз
                DidNotReceive() - перевіряє, що метод не викликався
                Arg.Do<T>(action) - виконує дію з аргументом під час виклику
                Arg.Invoke() - викликає переданий callback-аргумент
                ClearReceivedCalls() - очищає інформацію про попередні виклики
            */
        }

        [Fact]
        public async Task Update_ShouldUpdatePlayer()
        {
            // створюємо DTO для оновлення
            var playerDto = new PlayerDTO
            {
                Id = 1,
                Name = "Cristiano Ronaldo",
                Age = 41,
                Position = "Forward",
                TeamId = 20
            };

            await service.Update(playerDto);

            // перевіряємо, що сервіс передав оновленого гравця в репозиторій
            await players.Received(1).Update( // Received(1) перевіряє, що метод Update був викликаний один раз
                Arg.Is<Player>(player => // Arg.Is<Player> перевіряє, що переданий об'єкт Player відповідає умовам
                    player.Id == playerDto.Id &&
                    player.Name == playerDto.Name &&
                    player.Age == playerDto.Age &&
                    player.Position == playerDto.Position &&
                    player.TeamId == playerDto.TeamId));
        }

        [Theory] // атрибут, який дозволяє запускати один тест з різними наборами даних
        [InlineData(1, "Lionel Messi", 39, "Forward")]
        [InlineData(2, "Cristiano Ronaldo", 41, "Forward")]
        [InlineData(3, "Luka Modric", 40, "Midfielder")]
        public async Task Get_ShouldReturnCorrectPlayer(
            int id,
            string name,
            int age,
            string position)
        {
            // створюємо гравця з тестовими даними
            var player = new Player
            {
                Id = id,
                Name = name,
                Age = age,
                Position = position,
                TeamId = null
            };

            // налаштовуємо репозиторій на повернення цього гравця
            players.Get(id).Returns(player);

            var result = await service.Get(id);

            // перевіряємо, що дані правильно перенесені з entity у DTO
            result.Should().BeEquivalentTo(new PlayerDTO
            {
                Id = id,
                Name = name,
                Age = age,
                Position = position,
                TeamId = null,
                Team = null
            });

            // команда не повинна запитуватися, оскільки TeamId відсутній
            await teams.DidNotReceive().Get(Arg.Any<int>());
        }

        [Fact]
        public async Task Delete_ShouldDeletePlayer()
        {
            // видаляємо гравця з конкретним ідентифікатором
            await service.Delete(15);

            // перевіряємо, що репозиторій отримав правильний ідентифікатор
            await players.Received(1).Delete(15);
        }

        [Fact]
        public async Task Get_ShouldThrowValidationException_WhenPlayerDoesNotExist()
        {
            // налаштовуємо репозиторій так, щоб гравця не було знайдено
            players.Get(100).Returns((Player?)null);

            // перевіряємо, що сервіс повідомляє про відсутність гравця
            var action = async () => await service.Get(100);

            await action.Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("Немає такого гравця!");
        }

        [Fact]
        public async Task Get_ShouldReturnPlayerWithoutTeam()
        {
            // створюємо гравця без команди
            var player = new Player
            {
                Id = 1,
                Name = "Player 1",
                Age = 25,
                Position = "Defender",
                TeamId = null
            };

            players.Get(1).Returns(player); // налаштовуємо репозиторій так, щоб він повертав цього гравця

            var result = await service.Get(1);

            // перевіряємо перенесення даних з entity у DTO
            result.Should().BeEquivalentTo(new PlayerDTO
            {
                Id = 1,
                Name = "Player 1",
                Age = 25,
                Position = "Defender",
                TeamId = null,
                Team = null
            });

            // якщо в гравця немає команди, репозиторій команд не повинен викликатися
            await teams.DidNotReceive().Get(Arg.Any<int>());
        }

        [Fact]
        public async Task Get_ShouldReturnPlayerWithTeamName()
        {
            // створюємо гравця, який належить до команди
            var player = new Player
            {
                Id = 1,
                Name = "Player 1",
                Age = 25,
                Position = "Midfielder",
                TeamId = 10
            };

            var team = new Team
            {
                Id = 10,
                Name = "Real Madrid",
                Coach = "Coach 1"
            };

            players.Get(1).Returns(player);
            teams.Get(10).Returns(team);

            var result = await service.Get(1);

            // перевіряємо, що назва команди потрапила в DTO
            result.Team.Should().Be("Real Madrid");
            result.TeamId.Should().Be(10);
        }

        [Fact]
        public async Task Get_ShouldReturnNullTeamName_WhenTeamDoesNotExist()
        {
            // гравець має TeamId, але самої команди немає
            var player = new Player
            {
                Id = 1,
                Name = "Player 1",
                Age = 25,
                Position = "Goalkeeper",
                TeamId = 10
            };

            players.Get(1).Returns(player);
            teams.Get(10).Returns((Team?)null);

            var result = await service.Get(1);

            // сервіс не повинен падати через відсутню команду
            result.Team.Should().BeNull();
            result.TeamId.Should().Be(10);
        }

        [Fact]
        public async Task GetAll_ShouldReturnEmptyCollection_WhenThereAreNoPlayers()
        {
            // репозиторії повертають порожні колекції
            players.GetAll().Returns([]);
            teams.GetAll().Returns([]);

            var result = await service.GetAll();

            // перевіряємо результат
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAll_ShouldMapPlayersWithTeamNames()
        {
            // створюємо кілька гравців
            var playersList = new List<Player>
            {
                new Player
                {
                    Id = 1,
                    Name = "Player 1",
                    Age = 25,
                    Position = "Forward",
                    TeamId = 10
                },
                new Player
                {
                    Id = 2,
                    Name = "Player 2",
                    Age = 28,
                    Position = "Defender",
                    TeamId = 20
                },
                new Player
                {
                    Id = 3,
                    Name = "Player 3",
                    Age = 30,
                    Position = "Midfielder",
                    TeamId = null
                }
            };

            // створюємо команди
            var teamsList = new List<Team>
            {
                new Team
                {
                    Id = 10,
                    Name = "Barcelona"
                },
                new Team
                {
                    Id = 20,
                    Name = "Real Madrid"
                }
            };

            players.GetAll().Returns(playersList);
            teams.GetAll().Returns(teamsList);

            var result = (await service.GetAll()).ToList();

            // перевіряємо кількість отриманих DTO
            result.Should().HaveCount(3);

            // перевіряємо зв'язок гравців з командами
            result[0].Team.Should().Be("Barcelona");
            result[1].Team.Should().Be("Real Madrid");
            result[2].Team.Should().BeNull();

            // перевіряємо основні дані гравців
            result[0].Name.Should().Be("Player 1");
            result[1].Name.Should().Be("Player 2");
            result[2].Name.Should().Be("Player 3");
        }

        [Fact]
        public async Task GetAll_ShouldReturnNullTeamName_WhenTeamDoesNotExist()
        {
            // створюємо гравця з TeamId, для якого немає команди
            var playersList = new List<Player>
            {
                new Player
                {
                    Id = 1,
                    Name = "Player 1",
                    Age = 25,
                    Position = "Forward",
                    TeamId = 999
                }
            };

            // команда з таким Id відсутня
            var teamsList = new List<Team>();

            players.GetAll().Returns(playersList);
            teams.GetAll().Returns(teamsList);

            var result = (await service.GetAll()).ToList();

            // перевіряємо, що сервіс повернув гравця без назви команди
            result.Should().ContainSingle();
            result[0].Team.Should().BeNull();
            result[0].TeamId.Should().Be(999);
        }
    }
}
