using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Soccer.Application.DTO;
using Soccer.Application.Interfaces;
using Soccer.Common.Exceptions;
using Soccer.Presentation.Controllers;

namespace Soccer.UnitTests.Controllers
{
    public class TeamsControllerTests
    {
        private readonly IEntityService<TeamDTO> teamService;
        private readonly TeamsController controller;

        public TeamsControllerTests()
        {
            teamService = Substitute.For<IEntityService<TeamDTO>>();

            controller = new TeamsController(teamService);
        }

        [Fact]
        public async Task Get_ShouldReturnOk()
        {
            // створюємо тестовий список команд
            var teams = new List<TeamDTO>
            {
                new TeamDTO
                {
                    Id = 1,
                    Name = "Barcelona",
                    Coach = "Coach 1"
                }
            };

            teamService.GetAll().Returns(teams);

            var result = await controller.Get();

            // перевіряємо HTTP 200 OK
            result.Result.Should().BeOfType<OkObjectResult>();

            var okResult = result.Result as OkObjectResult;

            okResult!.Value.Should().BeEquivalentTo(teams);
        }

        [Fact]
        public async Task Get_ShouldReturnOk_WhenTeamExists()
        {
            // створюємо знайдену команду
            var team = new TeamDTO
            {
                Id = 1,
                Name = "Barcelona",
                Coach = "Coach 1"
            };

            teamService.Get(1).Returns(team);

            var result = await controller.Get(1);

            // перевіряємо HTTP 200 OK
            result.Result.Should().BeOfType<OkObjectResult>();

            var okResult = result.Result as OkObjectResult;

            okResult!.Value.Should().BeEquivalentTo(team);
        }

        [Fact]
        public async Task Get_ShouldReturnNotFound_WhenTeamDoesNotExist()
        {
            // імітуємо відсутню команду
            teamService
                .Get(100)
                .Returns(Task.FromException<TeamDTO>(
                    new ValidationException("Немає такого клуба!")));

            var result = await controller.Get(100);

            // контролер повинен повернути HTTP 404
            result.Result.Should().BeOfType<NotFoundObjectResult>();

            var notFoundResult = result.Result as NotFoundObjectResult;

            notFoundResult!.Value.Should().Be("Немає такого клуба!");
        }

        [Fact]
        public async Task Create_ShouldReturnOk()
        {
            // створюємо валідну команду
            var team = new TeamDTO
            {
                Id = 1,
                Name = "Barcelona",
                Coach = "Coach 1"
            };

            var result = await controller.Create(team);

            // перевіряємо HTTP 200 OK
            result.Should().BeOfType<OkResult>();

            // перевіряємо виклик сервісу
            await teamService.Received(1).Create(team);
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // додаємо помилку в ModelState
            controller.ModelState.AddModelError(
                "Name",
                "поле має бути заповнене.");

            var team = new TeamDTO();

            var result = await controller.Create(team);

            // при невалідній моделі повертається HTTP 400
            result.Should().BeOfType<BadRequestObjectResult>();

            await teamService.DidNotReceive().Create(Arg.Any<TeamDTO>());
        }

        [Fact]
        public async Task Update_ShouldReturnNoContent()
        {
            // створюємо валідну команду
            var team = new TeamDTO
            {
                Id = 1,
                Name = "Real Madrid",
                Coach = "Coach 2"
            };

            var result = await controller.Update(1, team);

            // PUT повинен повернути HTTP 204
            result.Should().BeOfType<NoContentResult>();

            await teamService.Received(1).Update(team);
        }

        [Fact]
        public async Task Update_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // додаємо помилку в ModelState
            controller.ModelState.AddModelError(
                "Name",
                "поле має бути заповнене.");

            var team = new TeamDTO();

            var result = await controller.Update(1, team);

            // контролер повинен повернути HTTP 400
            result.Should().BeOfType<BadRequestObjectResult>();

            await teamService.DidNotReceive().Update(Arg.Any<TeamDTO>());
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent()
        {
            // видалення виконується успішно
            var result = await controller.Delete(1);

            // успішний DELETE повертає HTTP 204
            result.Should().BeOfType<NoContentResult>();

            await teamService.Received(1).Delete(1);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenTeamDoesNotExist()
        {
            // імітуємо відсутню команду
            teamService
                .Delete(100)
                .Returns(Task.FromException(
                    new ValidationException("Немає такого клуба!")));

            var result = await controller.Delete(100);

            // контролер перетворює виняток на HTTP 404
            result.Should().BeOfType<NotFoundObjectResult>();

            var notFoundResult = result as NotFoundObjectResult;

            notFoundResult!.Value.Should().Be("Немає такого клуба!");
        }
    }
}