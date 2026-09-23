using Soccer.Domain.Entities;
using Soccer.Domain.Interfaces;

/*
 * цей клас є тестовою реалізацією IRepository<Player>, яка повністю імітує роботу репозиторію
 * без підключення до реального Firestore: дані зберігаються в звичайному List<Player>,
 * а методи GetAll, Get, Create, Update та Delete реалізують ті самі операції CRUD,
 * що й реальний репозиторій; початкові гравці додаються безпосередньо до списку,
 * а новий Id генерується автоматично.
 * такий підхід дозволяє інтеграційним тестам перевіряти взаємодію WebAPI та Application
 * із репозиторієм через реальні HTTP-запити, але без залежності від Firebase та мережевого доступу.
 * 
 * використовуэться як альтернатива реальному FirestorePlayerRepository у тестах, щоб ізолювати тестування контролерів та сервісів від зовнішніх залежностей,
 * https://gist.github.com/sunmeat/ec49c340abd8b3d3a6040d903a2c0555
 */
namespace Soccer.IntegrationTests.Fakes
{
    public class InMemoryPlayerRepository : IRepository<Player>
    {
        private readonly List<Player> players = new()
        {
            new Player
            {
                Id = 1,
                Name = "Test Player",
                Age = 25,
                Position = "Forward",
                TeamId = null
            },
            new Player
            {
                Id = 2,
                Name = "Second Player",
                Age = 28,
                Position = "Goalkeeper",
                TeamId = null
            }
        };

        public Task<IEnumerable<Player>> GetAll()
        {
            return Task.FromResult(
                players.AsEnumerable());
        }

        public Task<Player?> Get(int id)
        {
            Player? player = players
                .FirstOrDefault(p => p.Id == id);

            return Task.FromResult(player);
        }

        public Task<Player?> Get(string name)
        {
            Player? player = players
                .FirstOrDefault(p => p.Name == name);

            return Task.FromResult(player);
        }

        public Task Create(Player player)
        {
            int newId = players.Count == 0
                ? 1
                : players.Max(p => p.Id) + 1;

            player.Id = newId;

            players.Add(player);

            return Task.CompletedTask;
        }

        public Task Update(Player player)
        {
            Player? existing = players
                .FirstOrDefault(p => p.Id == player.Id);

            if (existing != null)
            {
                existing.Name = player.Name;
                existing.Age = player.Age;
                existing.Position = player.Position;
                existing.TeamId = player.TeamId;
            }

            return Task.CompletedTask;
        }

        public Task Delete(int id)
        {
            Player? player = players
                .FirstOrDefault(p => p.Id == id);

            if (player != null)
                players.Remove(player);

            return Task.CompletedTask;
        }
    }
}