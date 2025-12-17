using StudentSuccessPredictor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".StudentSuccessPredictor.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<StudentJournalStore>();
builder.Services.AddSingleton<MlStudentModelService>();

var app = builder.Build();

app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

// простий “захист” для Journal/Predict: перевірка сесії
app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path.Value ?? "";
    var needsAuth = path.StartsWith("/Journal", StringComparison.OrdinalIgnoreCase)
                 || path.StartsWith("/Predict", StringComparison.OrdinalIgnoreCase);

    if (needsAuth && string.IsNullOrWhiteSpace(ctx.Session.GetString("TEACHER")))
    {
        ctx.Response.Redirect("/Account/Login");
        return;
    }

    await next();
});

// тренуємо/завантажуємо модель при старті
using (var scope = app.Services.CreateScope())
{
    var ml = scope.ServiceProvider.GetRequiredService<MlStudentModelService>();
    ml.EnsureModelReady();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
