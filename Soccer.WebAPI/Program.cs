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

// =====================================================================================================

// додано файл Directory.Build.targets, який автоматично запускає тести перед білдом Soccer.WebAPI

// =====================================================================================================

// додано файл appsettings.json в корінь solution, який містить конфігурацію Firebase

var builder = WebApplication.CreateBuilder(args);

// визначаємо шлях до кореня solution
string? solutionPath = builder.Environment.ContentRootPath;

while (solutionPath != null &&
       !File.Exists(Path.Combine(solutionPath, "Soccer.sln")))
{
    solutionPath = Directory.GetParent(solutionPath)?.FullName;
}

// перевіряємо, що Soccer.sln знайдено
if (solutionPath == null)
{
    throw new DirectoryNotFoundException(
        "Не знайдено корінь solution.");
}

// завантажуємо спільну конфігурацію з appsettings.json,
// який знаходиться в корені solution
string appsettingsPath =
    Path.Combine(solutionPath, "appsettings.json");

builder.Configuration.AddJsonFile(
    appsettingsPath,
    optional: false,
    reloadOnChange: false);

// отримуємо назву Firebase проєкту з конфігурації
string projectId =
    builder.Configuration["Firebase:ProjectId"]
    ?? throw new InvalidOperationException(
        $"Firebase:ProjectId не задано в {appsettingsPath}.");

// визначаємо шлях до Firebase service account
string firebasePath = Path.GetFullPath(
    Path.Combine(
        solutionPath,
        "Soccer.Infrastructure",
        "RealFirebase",
        "firebase.json"));

// створюємо Infrastructure
builder.Services.AddInfrastructure(
    firebasePath,
    projectId);

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
