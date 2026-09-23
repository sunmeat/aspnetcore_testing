using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Soccer.Application.DTO;
using Soccer.Application.Interfaces;
using Soccer.Common.Exceptions;
using Soccer.Presentation.Controllers;

namespace Soccer.UnitTests.Controllers
{
    public class PlayersControllerTests
    {
        private readonly IEntityService<PlayerDTO> playerService;
        private readonly PlayersController controller;

        public PlayersControllerTests()
        {
            playerService = Substitute.For<IEntityService<PlayerDTO>>();

            controller = new PlayersController(playerService);
        }

        /* тести контролерів перевіряють HTTP-поведінку контролера незалежно від реального PlayerService та Firestore:
         * чи повертає контролер правильні статуси 200, 204, 400, 404, чи передає отримані дані у відповідь,
         * чи правильно перетворює виняток сервісу на 404, і головне, чи не викликає сервіс, коли ModelState невалідний.
         * тобто тестується відповідність контролера контракту API, а не повторно тестується логіка сервісу або бази даних. */
       
        [Fact]
        public async Task Get_ShouldReturnOk()
        {
            // створюємо тестовий список гравців
            var players = new List<PlayerDTO>
            {
                new PlayerDTO
                {
                    Id = 1,
                    Name = "Player 1",
                    Age = 25,
                    Position = "Forward"
                }
            };

            playerService.GetAll().Returns(players);

            var result = await controller.Get();

            // перевіряємо HTTP 200 OK
            result.Result.Should().BeOfType<OkObjectResult>();

            // перевіряємо дані всередині відповіді
            var okResult = result.Result as OkObjectResult;

            okResult!.Value.Should().BeEquivalentTo(players);
        }

        [Fact]
        public async Task Get_ShouldReturnOk_WhenPlayerExists()
        {
            // створюємо знайденого гравця
            var player = new PlayerDTO
            {
                Id = 1,
                Name = "Player 1",
                Age = 25,
                Position = "Forward"
            };

            playerService.Get(1).Returns(player);

            var result = await controller.Get(1);

            // перевіряємо HTTP 200 OK
            result.Result.Should().BeOfType<OkObjectResult>();

            var okResult = result.Result as OkObjectResult;

            okResult!.Value.Should().BeEquivalentTo(player);
        }

        [Fact]
        public async Task Get_ShouldReturnNotFound_WhenPlayerDoesNotExist()
        {
            // імітуємо ситуацію, коли сервіс не знаходить гравця
            playerService
                .Get(100)
                .Returns(Task.FromException<PlayerDTO>(
                    new ValidationException("Немає такого гравця!")));

            var result = await controller.Get(100);

            // контролер повинен перетворити виняток на HTTP 404
            result.Result.Should().BeOfType<NotFoundObjectResult>();

            var notFoundResult = result.Result as NotFoundObjectResult;

            notFoundResult!.Value.Should().Be("Немає такого гравця!");
        }

        [Fact]
        public async Task Create_ShouldReturnOk()
        {
            // створюємо валідного гравця
            var player = new PlayerDTO
            {
                Id = 1,
                Name = "Player 1",
                Age = 25,
                Position = "Forward"
            };

            var result = await controller.Create(player);

            // контролер повинен повернути HTTP 200 OK
            result.Should().BeOfType<OkResult>();

            // перевіряємо, що сервіс був викликаний
            await playerService.Received(1).Create(player);
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // вручну додаємо помилку ModelState
            controller.ModelState.AddModelError(
                "Name",
                "поле має бути заповнене.");

            var player = new PlayerDTO();

            var result = await controller.Create(player);

            // при невалідній моделі сервіс не повинен викликатися
            result.Should().BeOfType<BadRequestObjectResult>();

            await playerService.DidNotReceive().Create(Arg.Any<PlayerDTO>());
        }

        [Fact]
        public async Task Update_ShouldReturnNoContent()
        {
            // створюємо валідного гравця
            var player = new PlayerDTO
            {
                Id = 1,
                Name = "Player 1",
                Age = 25,
                Position = "Defender"
            };

            var result = await controller.Update(1, player);

            // PUT повинен повернути HTTP 204 No Content
            result.Should().BeOfType<NoContentResult>();

            // перевіряємо виклик сервісу
            await playerService.Received(1).Update(player);
        }

        [Fact]
        public async Task Update_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // додаємо помилку в ModelState
            controller.ModelState.AddModelError(
                "Name",
                "поле має бути заповнене.");

            var player = new PlayerDTO();

            var result = await controller.Update(1, player);

            // невалідна модель повинна завершитися HTTP 400
            result.Should().BeOfType<BadRequestObjectResult>();

            await playerService.DidNotReceive().Update(Arg.Any<PlayerDTO>());
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent()
        {
            // сервіс успішно виконує видалення
            var result = await controller.Delete(1);

            // успішне DELETE повертає HTTP 204
            result.Should().BeOfType<NoContentResult>();

            await playerService.Received(1).Delete(1);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenPlayerDoesNotExist()
        {
            // імітуємо відсутнього гравця
            playerService
                .Delete(100)
                .Returns(Task.FromException(
                    new ValidationException("Немає такого гравця!")));

            var result = await controller.Delete(100);

            // контролер перетворює виняток на HTTP 404
            result.Should().BeOfType<NotFoundObjectResult>();

            var notFoundResult = result as NotFoundObjectResult;

            notFoundResult!.Value.Should().Be("Немає такого гравця!");
        }
    }
}