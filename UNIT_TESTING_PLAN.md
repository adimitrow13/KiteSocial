# Учебен План: Правила и Стъпки за Писане на Unit Тестове (KiteSocial)

Този документ е структуриран така, че:
- **Основният текст (маркиран с ✍️)** е кратък, ясен и точен – за **записване в тетрадките/бележките на учениците** („черно на бяло“).
- **Текстът в скоби *(🗣️ Обяснение за учениците: ...)* ** е за **преподавателя** – какво да им разкажеш с примери на достъпен език по всяка точка.

---

## Част 1: Какво е Unit Тест и Златните Правила

### 1. Дефиниция
✍️ **Unit тест (Единичен тест):** Автоматизиран тест, който проверява **една най-малка изолирана част от кода** (обикновено един метод в клас), за да потвърди, че при дадени входни данни връща очаквания резултат.

*(🗣️ Обяснение за учениците: Представете си, че сглобявате кайт или автомобил. Преди да пуснете колата на пътя, проверявате отделно дали свещта дава искра и дали спирачката стиска диска. Не палите целия двигател само за да проверите крушката на фара. В програмирането „крушката“ е един метод – напр. методът `CreateToken`, `ToggleLikeAsync` или `GetUserStatsAsync` в нашия проект KiteSocial.)*

---

### 2. Структура на всеки Unit Тест – Правилото „AAA“ (Triple A)
✍️ Всеки unit тест задължително се пише в **3 стъпки (AAA)**:
1. **Arrange (Подготовка):** Създаваме нужните обекти, тестови данни и фалшиви зависимости (Mocks / In-Memory DB).
2. **Act (Действие):** Извикваме **само един** метод – този, който тестваме, и записваме резултата в променлива.
3. **Assert (Проверка):** Сравняваме получения резултат с очаквания чрез `Assert` (в C#) или `expect` (в Dart/Flutter).

*(🗣️ Обяснение за учениците: Запомнете го като готварска рецепта или химичен опит: **Arrange** е да си нарежете продуктите на плота; **Act** е да ги сложите в тигана; **Assert** е да опитате с лъжицата дали е солено. Ако в един тест няма `Assert`, този тест не проверява нищо – той винаги ще свети в зелено, дори кодът да е грешен!)*

---

### 3. Петте Златни Правила за Unit Тестове (F.I.R.S.T.)
✍️ **Правила, които всеки тест трябва да спазва:**
1. **Fast (Бързи):** Един unit тест трябва да се изпълнява за милисекунди. Хиляда теста трябва да минават за секунди.
2. **Isolated / Independent (Изолирани и независими):**
   - Тестовете **не зависят един от друг** (редът им на пускане няма значение).
   - Тестовете **не ползват реална база данни, файлове или интернет**. Ползват се *In-Memory DB* или *Mock обекти*.
3. **Repeatable (Повторяеми):** Пуснат 100 пъти на който и да е компютър, тестът винаги дава един и същ резултат.
4. **Self-Validating (Самопроверяващи се):** Резултатът е само **PASS (Зелено)** или **FAIL (Червено)** – без човек да чете конзолата.
5. **Thorough (Изчерпателни – Happy Path + Edge Cases):** Тестваме не само когато всичко е правилно (*Happy Path*), но и гранични случаи и грешки (*Edge Cases* – празен списък, `null`, чужд потребител, грешна парола).

*(🗣️ Обяснение за учениците: Защо не тестваме директно върху истинската база `kitesocial.db`? Защото ако първият тест изтрие потребител, вторият тест ще гръмне, че потребителят го няма. Затова всеки тест си прави собствена „пясъчна кутия“ в оперативната памет – прави си данните, проверява си метода и след края на теста кутията изчезва.)*

---

### 4. Правило за именуване на тестовете
✍️ **Формула за име на тестов метод:**
`[ИмеНаМетод]_[КаквоТрябваДаСтане]_[ПриКаквоУсловие]`
- **Пример:** `DeleteSessionAsync_ShouldReturnForbidden_WhenUserIsNotOwner`
- **Превод:** Методът `DeleteSessionAsync` трябва да върне `Forbidden`, когато потребителят не е собственик на сесията.

*(🗣️ Обяснение за учениците: Когато имате 200 теста в проекта и един светне в червено, само от името му трябва да разберете какво точно се е счупило, без дори да отваряте кода на теста. Имена като `Test1` или `SpotTest` са забранени в реалната практика!)*

---

## Част 2: План за Изпълнение в Backend-а (.NET 10 / C# с xUnit)

### Защо изнесохме бизнес логиката в Services и направихме „тънки контролери“ (Thin Controllers)?
✍️ **Разделение при тестването след рефакторирането:**
1. **Сървисите (`SpotService`, `SessionService`, `PostService`)** съдържат цялата бизнес логика и работят с `KiteSocialDbContext` $\rightarrow$ **тестват се с `InMemory` база данни**.
2. **Контролерите (`SpotsController`, `SessionsController`, `PostsController`)** са „тънки“ – те не знаят за базата данни, а само викат интерфейс (`ISpotService`, `ISessionService`, `IPostService`) и връщат HTTP статус код $\rightarrow$ **тестват се с `Moq` без никаква база данни!**

*(🗣️ Обяснение за учениците: Ето защо рефакторирахме дебелите контролери! Когато `SessionsController` и `PostsController` бъркаха директно в базата данни, беше много трудно да тестваме само контролера отделно от таблиците. Сега, понеже контролерът приема интерфейс `ISessionService` или `IPostService`, можем да му подадем фалшив сървис с `Moq` на 3 реда код и да проверим само дали връща правилния HTTP статус: `200 OK`, `404 NotFound` или `403 Forbid`!)*

---

### Стъпка 1: Създаване и настройка на тестовия проект
✍️ **Команди за създаване на тестов проект в .NET:**
1. В [`KiteSocial.API.csproj`](file:///Users/aleks/KiteSocial/KiteSocial.API.csproj) се изключва папката с тестовете (`<Compile Remove="KiteSocial.Tests/**" />`).
2. Създава се проект с **xUnit**:
   ```bash
   dotnet new xunit -n KiteSocial.Tests -o KiteSocial.Tests
   dotnet add KiteSocial.Tests/KiteSocial.Tests.csproj reference KiteSocial.API.csproj
   ```
3. Добавят се 2 основни пакета за тестване:
   - `Microsoft.EntityFrameworkCore.InMemory` – за тестване на **Services** слоя.
   - `Moq` – за тестване на **Controllers** слоя чрез фалшиви интерфейси.
   ```bash
   dotnet add KiteSocial.Tests/KiteSocial.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 10.0.11
   dotnet add KiteSocial.Tests/KiteSocial.Tests.csproj package Moq
   ```

*(🗣️ Обяснение за учениците: Тук им покажи важния трик с `.csproj` файла! Понеже `KiteSocial.API.csproj` е в главната папка, .NET автоматично компилира всички `.cs` файлове във всички подпапки. Ако не му кажем `<Compile Remove="KiteSocial.Tests/**" />`, основният проект ще се опита да компилира тестовете и ще даде грешка.)*

---

### Стъпка 2: Какво точно тестваме по слоеве в Backend-а

#### А) Слой „Services“ – с `InMemory` база данни
✍️ **Правило за In-Memory DbContext:** Всеки тест създава контекст с уникално име чрез `Guid.NewGuid().ToString()`, за да не се смесват данните между отделните тестове:

```csharp
private static KiteSocialDbContext CreateInMemoryContext()
{
    var options = new DbContextOptionsBuilder<KiteSocialDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    return new KiteSocialDbContext(options);
}
```

✍️ **Какво тестваме във всеки от 4-те сървиса:**
1. **[`TokenService`](file:///Users/aleks/KiteSocial/Services/TokenService.cs):** Генерира ли валиден JWT токен с потребителските данни и хвърля ли грешка при липсващ `Jwt:Key`.
2. **[`SpotService`](file:///Users/aleks/KiteSocial/Services/SpotService.cs):** Филтрира ли спотовете по `search`, `country` и `waterType`; връща ли `null` при несъществуващ спот.
3. **[`SessionService`](file:///Users/aleks/KiteSocial/Services/SessionService.cs):**
   - `GetUserStatsAsync` – смята ли вярно общо минути, рекорд за скок и любим спот (и връща ли 0 при празен списък).
   - `DeleteSessionAsync` – връща ли `NotFound`, `Forbidden` (при чужда сесия) и `Success` (при собствена сесия).
4. **[`PostService`](file:///Users/aleks/KiteSocial/Services/PostService.cs):**
   - `CreatePostAsync` – връща ли грешка `"Referenced spot does not exist."`, ако подадем несъществуващ спот.
   - `ToggleLikeAsync` – добавя ли харесване при първо извикване (`IsLiked = true, LikesCount = 1`) и маха ли го при второ извикване (`IsLiked = false, LikesCount = 0`).
   - `DeletePostAsync` / `DeleteCommentAsync` – връща ли `PostDeleteResult.Forbidden`, когато друг потребител опита да изтрие поста/коментара.

---

#### Б) Слой „Controllers“ (Тънките контролери) – с `Moq` и `ClaimsPrincipal`
✍️ **Какво се тества в тънките контролери ([`SpotsController`](file:///Users/aleks/KiteSocial/Controllers/SpotsController.cs), [`SessionsController`](file:///Users/aleks/KiteSocial/Controllers/SessionsController.cs), [`PostsController`](file:///Users/aleks/KiteSocial/Controllers/PostsController.cs)):**
- Подаваме им `Mock<ISpotService>`, `Mock<ISessionService>` или `Mock<IPostService>`.
- Проверяваме дали преобразуват резултата от сървиса в правилния **HTTP статус код**:
  - Сървисът връща данни $\rightarrow$ контролерът връща `200 OK` (`OkObjectResult`) или `201 Created` (`CreatedAtActionResult`).
  - Сървисът връща `null` или `NotFound` $\rightarrow$ контролерът връща `404 Not Found` (`NotFoundObjectResult`).
  - Сървисът връща `Forbidden` $\rightarrow$ контролерът връща `403 Forbidden` (`ForbidResult`).
  - Сървисът връща `Success` при изтриване $\rightarrow$ контролерът връща `204 NoContent` (`NoContentResult`).

*(🗣️ Обяснение за учениците: Вижте колко лесно се тества [`SessionsController.DeleteSession`](file:///Users/aleks/KiteSocial/Controllers/SessionsController.cs#L80-L94) или [`PostsController.DeletePost`](file:///Users/aleks/KiteSocial/Controllers/PostsController.cs#L64-L78) след рефакторирането! В контролера има `switch` конструкция: `NotFound => NotFound()`, `Forbidden => Forbid()`, `Success => NoContent()`. С `Moq` просто казваме на фалшивия сървис: „Върни `Forbidden`“ и проверяваме дали контролерът връща `ForbidResult`!)*

---

## Част 3: План за Изпълнение във Frontend-а (Flutter / Dart)

### Стъпка 1: Трите вида тестове във Flutter
✍️ Във Flutter (`flutter_test`) пишем 3 групи тестове в папката `kite_social_app/test/`:
1. **Model Unit Tests (`test(...)`):** Проверяват дали JSON данните от сървъра се превръщат правилно в Dart обекти (`fromJson`) и дали изчисляемите свойства (като `waterTypeDisplay`) връщат точния текст.
2. **Service Unit Tests (`test(...)` с `MockClient`):** Проверяват [`ApiService`](file:///Users/aleks/KiteSocial/kite_social_app/lib/services/api_service.dart) чрез симулиран HTTP клиент (`MockClient`), който връща фалшив JSON със статус `200`, `400`, `423` или `429`.
3. **Widget Tests (`testWidgets(...)`):** Проверяват екраните ([`LoginScreen`](file:///Users/aleks/KiteSocial/kite_social_app/lib/screens/login_screen.dart), [`HomeScreen`](file:///Users/aleks/KiteSocial/kite_social_app/lib/screens/home_screen.dart)) – дали бутоните, текстовите полета и съобщенията за грешка се показват на екрана при натискане (`tester.tap`, `tester.enterText`, `tester.pump`).

---

## Част 4: Обобщен Чеклист за Изпълнение Навсякъде

✍️ **Какво трябва да направим последователно, за да минат успешно всички тестове:**

1. **Backend (.NET 10):**
   - [ ] Добавяме `<Compile Remove="KiteSocial.Tests/**" />` в [`KiteSocial.API.csproj`](file:///Users/aleks/KiteSocial/KiteSocial.API.csproj).
   - [ ] Създаваме тестовия проект `KiteSocial.Tests` и инсталираме `Microsoft.EntityFrameworkCore.InMemory` и `Moq`.
   - [ ] Пишем тестовете за сървисите (`TokenService`, `SpotService`, `SessionService`, `PostService`) с In-Memory DB.
   - [ ] Пишем тестовете за тънките контролери (`SpotsController`, `SessionsController`, `PostsController`) с `Moq`.
   - [ ] Изпълняваме:
     ```bash
     dotnet test KiteSocial.Tests/KiteSocial.Tests.csproj
     ```

2. **Frontend (Flutter):**
   - [ ] Поправяме `test/widget_test.dart` и добавяме тестове за моделите и екраните.
   - [ ] Изпълняваме:
     ```bash
     cd kite_social_app
     flutter test
     ```

---

## Част 5: Сценарий за Жива Демонстрация пред Класа (След Рефакторирането на Контролерите)

Този сценарий е специално адаптиран след рефакторирането на контролерите. С него ще покажеш на учениците **и двете техники** на живо за 10–15 минути:
1. **Тестване на Service с `InMemory` база данни** ([`PostService`](file:///Users/aleks/KiteSocial/Services/PostService.cs) или [`SessionService`](file:///Users/aleks/KiteSocial/Services/SessionService.cs)).
2. **Тестване на рефакториран Тънък Контролер с `Moq`** ([`SessionsController`](file:///Users/aleks/KiteSocial/Controllers/SessionsController.cs) или [`PostsController`](file:///Users/aleks/KiteSocial/Controllers/PostsController.cs)).

---

### Стъпка 1: Покажи им разликата между Service и Тънък Контролер (2 мин.)
Отвори един до друг два файла на проектора:
1. [`Services/SessionService.cs`](file:///Users/aleks/KiteSocial/Services/SessionService.cs#L145-L161) – покажи им метода `DeleteSessionAsync`:
   ```csharp
   if (session.UserId != userId)
   {
       return SessionDeleteResult.Forbidden;
   }
   ```
2. [`Controllers/SessionsController.cs`](file:///Users/aleks/KiteSocial/Controllers/SessionsController.cs#L80-L94) – покажи им рефакторирания метод `DeleteSession`:
   ```csharp
   var result = await _sessionService.DeleteSessionAsync(id, userId.Value);
   return result switch
   {
       SessionDeleteResult.NotFound => NotFound(new { Message = "Session not found." }),
       SessionDeleteResult.Forbidden => Forbid(),
       SessionDeleteResult.Success => NoContent(),
       _ => BadRequest()
   };
   ```

*(🗣️ Какво да им кажеш: „Вижте колко чисто е разделена отговорността след като рефакторирахме дебелите контролери! `SessionService` проверява в базата дали сесията принадлежи на потребителя и връща `SessionDeleteResult.Forbidden`. А тънкият `SessionsController` изобщо няма база данни – той само превежда `Forbidden` в HTTP статус `403 Forbid()`. Сега ще напишем по един тест и за двете!“)*

---

### Стъпка 2: Създаване на тестовия проект в Терминала (2 мин.)

1. Добави в [`KiteSocial.API.csproj`](file:///Users/aleks/KiteSocial/KiteSocial.API.csproj) преди `</Project>`:
   ```xml
   <ItemGroup>
     <Compile Remove="KiteSocial.Tests/**" />
     <Content Remove="KiteSocial.Tests/**" />
     <EmbeddedResource Remove="KiteSocial.Tests/**" />
     <None Remove="KiteSocial.Tests/**" />
   </ItemGroup>
   ```
2. Изпълни в терминала:
   ```bash
   dotnet new xunit -n KiteSocial.Tests -o KiteSocial.Tests
   dotnet add KiteSocial.Tests/KiteSocial.Tests.csproj reference KiteSocial.API.csproj
   dotnet add KiteSocial.Tests/KiteSocial.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 10.0.11
   dotnet add KiteSocial.Tests/KiteSocial.Tests.csproj package Moq
   ```

---

### Стъпка 3: Напишете заедно тестовете в `KiteSocial.Tests/UnitTest1.cs` (6 мин.)

В този един файл им показваш:
- **Тест 1 & 2:** Тестване на **Service слоя** (`SessionService` и `PostService`) с `InMemory` база данни.
- **Тест 3:** Тестване на **рефакторирания Контролер** (`SessionsController`) с `Moq` и логнат потребител (`ClaimsPrincipal`).

```csharp
using System.Security.Claims;
using KiteSocial.API.Controllers;
using KiteSocial.API.Data;
using KiteSocial.API.Models.Entities;
using KiteSocial.API.Services;
using KiteSocial.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace KiteSocial.Tests;

public class KiteSocialLiveDemoTests
{
    // Помощен метод за изолирана база данни в RAM паметта (за Service тестовете)
    private static KiteSocialDbContext GetInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<KiteSocialDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new KiteSocialDbContext(options);
    }

    // Помощен метод за симулиране на логнат потребител с JWT клейм (за Controller тестовете)
    private static void SetLoggedUser(ControllerBase controller, Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestJwtAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    // =========================================================================
    // ЧАСТ 1: Тестване на SERVICE слоя (Бизнес логика с In-Memory База Данни)
    // =========================================================================

    [Fact]
    public async Task SessionService_GetUserStatsAsync_ShouldCalculateTotalsAndMaxJump()
    {
        // 1. ARRANGE (Подготовка: 2 сесии в паметта - 60 мин/7.5м и 90 мин/12.0м)
        using var db = GetInMemoryDb();
        var userId = Guid.NewGuid();
        var spot = new Spot { Id = Guid.NewGuid(), Name = "Поморие", Country = "България" };
        db.Spots.Add(spot);

        db.KiteSessions.AddRange(
            new KiteSession { UserId = userId, SpotId = spot.Id, Spot = spot, DurationMinutes = 60, MaxJumpHeightMeters = 7.5 },
            new KiteSession { UserId = userId, SpotId = spot.Id, Spot = spot, DurationMinutes = 90, MaxJumpHeightMeters = 12.0 }
        );
        await db.SaveChangesAsync();

        var service = new SessionService(db);

        // 2. ACT (Действие)
        var stats = await service.GetUserStatsAsync(userId);

        // 3. ASSERT (Проверка: 2 сесии, 150 минути, рекорд 12.0м)
        Assert.Equal(2, stats.TotalSessions);
        Assert.Equal(150, stats.TotalMinutesOnWater);
        Assert.Equal(12.0, stats.MaxJumpHeightRecord);
        Assert.Equal("Поморие", stats.FavoriteSpotName);
    }

    [Fact]
    public async Task PostService_ToggleLikeAsync_ShouldAddAndRemoveLike_WhenCalledTwice()
    {
        // 1. ARRANGE (Подготовка: създаваме 1 пост в In-Memory базата)
        using var db = GetInMemoryDb();
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        db.Posts.Add(new Post { Id = postId, UserId = userId, Content = "Супер вятър на Лозенец!" });
        await db.SaveChangesAsync();

        var postService = new PostService(db);

        // 2. ACT & ASSERT 1 – Първо натискане на Like -> добавя харесване
        var firstClick = await postService.ToggleLikeAsync(postId, userId);
        Assert.NotNull(firstClick);
        Assert.True(firstClick.IsLiked);
        Assert.Equal(1, firstClick.LikesCount);

        // 3. ACT & ASSERT 2 – Второ натискане на Like -> премахва харесването
        var secondClick = await postService.ToggleLikeAsync(postId, userId);
        Assert.NotNull(secondClick);
        Assert.False(secondClick.IsLiked);
        Assert.Equal(0, secondClick.LikesCount);
    }

    // =========================================================================
    // ЧАСТ 2: Тестване на ТЪНЪК КОНТРОЛЕР (с Moq, без база данни!)
    // =========================================================================

    [Fact]
    public async Task SessionsController_DeleteSession_ShouldReturnForbid_WhenServiceReturnsForbidden()
    {
        // 1. ARRANGE (Подготовка: правим фалшив ISessionService с Moq)
        var sessionId = Guid.NewGuid();
        var loggedUserId = Guid.NewGuid();

        var mockSessionService = new Mock<ISessionService>();
        mockSessionService
            .Setup(s => s.DeleteSessionAsync(sessionId, loggedUserId))
            .ReturnsAsync(SessionDeleteResult.Forbidden); // Казваме на мока да върне Forbidden

        var controller = new SessionsController(mockSessionService.Object);
        SetLoggedUser(controller, loggedUserId); // Симулираме логнат потребител

        // 2. ACT (Действие: извикваме метода на контролера)
        var actionResult = await controller.DeleteSession(sessionId);

        // 3. ASSERT (Проверка: контролерът трябва да върне HTTP 403 ForbidResult)
        Assert.IsType<ForbidResult>(actionResult);
    }
}
```

---

### Стъпка 4: Стартиране и демонстрация на „Зелено -> Червено“ (2 мин.)

1. Пусни в терминала:
   ```bash
   dotnet test KiteSocial.Tests/KiteSocial.Tests.csproj
   ```
   *(🗣️ Покажи им: „Виждате ли – `Passed: 3, Failed: 0` за по-малко от секунда! И сървисите с In-Memory базата, и тънкият контролер с Moq работят безупречно.“)*

2. **Демонстрирай как тестът на тънкия контролер хваща грешка:**
   Отвори [`Controllers/SessionsController.cs`](file:///Users/aleks/KiteSocial/Controllers/SessionsController.cs#L87-L93) на ред 90 и промени:
   ```csharp
   SessionDeleteResult.Forbidden => NoContent(), // Умишлена грешка: връщаме 204 вместо 403 Forbid!
   ```
3. Пусни отново `dotnet test KiteSocial.Tests/KiteSocial.Tests.csproj`.
   - Тестът `SessionsController_DeleteSession_ShouldReturnForbid_WhenServiceReturnsForbidden` веднага светва в **червено**:
     `Assert.IsType() Failure: Expected: Microsoft.AspNetCore.Mvc.ForbidResult, Actual: Microsoft.AspNetCore.Mvc.NoContentResult`
   *(🗣️ Какво да им кажеш: „Ето защо тестваме и сървиса, и тънкия контролер поотделно! Дори сървисът да си върши работата, ако някой в контролера обърка HTTP статус кода, тестът с `Moq` веднага го улавя!“)*
4. Върни обратно `SessionDeleteResult.Forbidden => Forbid(),` и пусни `dotnet test`, за да завършите на зелено.
