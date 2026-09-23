using Soccer.Application.DependencyInjection;
using Soccer.Infrastructure.DependencyInjection;
using Soccer.Infrastructure.Persistence;

// у Firebase Console:
// Settings > Service accounts > Generate new private key > Download the JSON file
// файл кладемо в Soccer.Infrastructure\RealFirebase\firebase.json (з внесенням в .gitignore!)

// =====================================================================================================

// додано проєкт Soccer.UnitTests, який містить юніт-тести для Soccer.Application та Soccer.WebAPI.
// для юніт-тестів використовується xUnit, FluentAssertions та NSubstitute (для моків).
// xUnit є більш сучасним фреймворком, створеним тими ж авторами, що й NUnit 2.x,
// але з урахуванням накопиченого досвіду та виправленням застарілих патернів.
// у тестах перевіряється логіка застосунку без реального доступу до Firestore.

// =====================================================================================================

// додано проєкт Soccer.IntegrationTests, який містить інтеграційні тести для Soccer.Infrastructure та Soccer.WebAPI.
// для інтеграційних тестів використовується xUnit, FluentAssertions та Microsoft.AspNetCore.Mvc.Testing.
// інтеграційні тести перевіряють взаємодію між компонентами застосунку та Firestore.

var builder = WebApplication.CreateBuilder(args);

string firebasePath = Path.GetFullPath(
    Path.Combine(
        builder.Environment.ContentRootPath,
        "..",
        "Soccer.Infrastructure",
        "RealFirebase",
        "firebase.json"));

builder.Services.AddInfrastructure(firebasePath); // !!!
builder.Services.AddApplication();

builder.Services.AddControllers();

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    FirestoreSeeder seeder =
        scope.ServiceProvider.GetRequiredService<FirestoreSeeder>();

    await seeder.SeedAsync();
}

app.MapControllers();

app.Run();