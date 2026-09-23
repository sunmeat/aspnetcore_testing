using Microsoft.AspNetCore.Mvc.Testing; // dotnet add package Microsoft.AspNetCore.Mvc.Testing - бібліотека для інтеграційного тестування веб-додатків ASP.NET Core

namespace Soccer.IntegrationTests.Fixtures // Fixtures - це спеціальні класи, які надають спільні ресурси для тестів, наприклад, налаштування тестового середовища, створення тестових даних або конфігурацію сервісів
{
    public class CustomWebApplicationFactory
        : WebApplicationFactory<Program> // клас Program з Soccer.WebAPI, який містить метод Main і конфігурацію веб-додатку
    {

        // цей клас дозволяє створювати тестовий веб-сервер для інтеграційного тестування API,
        // використовуючи реальні HTTP-запити та відповіді,
        // що дозволяє перевіряти поведінку контролерів і сервісів у реальному середовищі
        
        protected override void Dispose(bool disposing)
        {
            // звільняємо ресурси, які використовує тестовий сервер
            base.Dispose(disposing);
        }
    }
}