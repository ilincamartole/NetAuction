//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.EntityFrameworkCore;
//using WebApplication1.Data;
//using WebApplication1.Models;
//using WebApplication1.Services;
//using WebApplication1.Hubs;

//var builder = WebApplication.CreateBuilder(args);

//// 1. Servicii de bază
//builder.Services.AddRazorPages();
//builder.Services.AddControllersWithViews();

//// 2. Configurare Bază de Date
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//// 3. Configurare Identity
//// Am setat RequireConfirmedAccount = false pentru a evita eroarea "Invalid attempt" la login
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
//    options.SignIn.RequireConfirmedAccount = true;
//    options.Password.RequiredLength = 8;
//    options.User.RequireUniqueEmail = true;
//})
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddDefaultTokenProviders()
//    .AddDefaultUI();

//// 4. Serviciul de Email
//// Înregistrare Servicii
//builder.Services.AddSignalR();
//builder.Services.AddTransient<IEmailSender, EmailSender>();

//// 5. Configurări de Securitate (Lockout & Parole)
//builder.Services.Configure<IdentityOptions>(options =>
//{
//    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
//    options.Lockout.MaxFailedAccessAttempts = 5;
//    options.Lockout.AllowedForNewUsers = true;

//    options.Password.RequireDigit = true;
//    options.Password.RequireLowercase = true;
//    options.Password.RequireNonAlphanumeric = true;
//    options.Password.RequireUppercase = true;
//});

//// 6. Securizarea Cookie-urilor
//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.Cookie.HttpOnly = true;
//    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
//    options.LoginPath = "/Identity/Account/Login";
//    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
//    options.SlidingExpiration = true;
//    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//});

//builder.Services.AddHostedService<AuctionWorker>();

//builder.Services.AddHttpClient();

//var app = builder.Build();

//// 7. Pipeline-ul de cereri (Middleware)
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//// Authentication trebuie să fie ÎNAINTE de Authorization
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.MapRazorPages();

//// --- SEEDING LOGIC (Runtime) ---
//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

//    // A. Creare Rol Admin
//    if (!await roleManager.RoleExistsAsync("Admin"))
//    {
//        await roleManager.CreateAsync(new IdentityRole("Admin"));
//    }

//    // B. Listă Admini de creat
//    var adminData = new List<(string Email, string Nume)>
//    {
//        ("vlad@test.com", "Vlad"),
//        ("simon@test.com", "Simon"),
//        ("ilinca@test.com", "Ilinca"),
//        ("maria@test.com", "Maria"),
//        ("ilincamartole@gmail.com", "Ilinca Admin")
//    };

//    foreach (var data in adminData)
//    {
//        var user = await userManager.FindByEmailAsync(data.Email);

//        if (user == null)
//        {
//            user = new ApplicationUser
//            {
//                UserName = data.Email,
//                Email = data.Email,
//                EmailConfirmed = true,
//                nume = data.Nume,     
//                prenume = "Admin",
//                adresa = "Adresa Generică Test",
//                balance = 0,
//                data_inregistrarii = DateTime.Now
//            };

//            var result = await userManager.CreateAsync(user, "ParolaTest123!");

//            if (result.Succeeded)
//            {
//                await userManager.AddToRoleAsync(user, "Admin");
//            }
//        }
//        else
//        {
//            // Dacă user-ul există deja (din încercări anterioare), ne asigurăm doar că are rolul
//            if (!await userManager.IsInRoleAsync(user, "Admin"))
//            {
//                await userManager.AddToRoleAsync(user, "Admin");
//            }
//        }
//    }
//}
//app.MapHub<AuctionHub>("/auctionHub");
//app.MapControllers();
//app.Run();

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Servicii de bază
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// 2. Configurare Bază de Date (MODIFICAT PENTRU RENDER / POSTGRESQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Dacă rulează pe Render (link-ul începe cu postgres://)
    if (connectionString != null && connectionString.StartsWith("postgres://"))
    {
        var databaseUri = new Uri(connectionString);
        var userInfo = databaseUri.UserInfo.Split(':');
        var npgsqlConnectionString = $"Host={databaseUri.Host};Port={databaseUri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={databaseUri.LocalPath.TrimStart('/')};Pooling=true;SSL Mode=Require;Trust Server Certificate=true;";

        options.UseNpgsql(npgsqlConnectionString);
    }
    else
    {
        // Altfel, folosește SQL Server (pentru rularea locală)
        options.UseSqlServer(connectionString);
    }
});

// 3. Configurare Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// 4. Serviciul de Email & SignalR
builder.Services.AddSignalR();
builder.Services.AddTransient<IEmailSender, EmailSender>();

// 5. Configurări de Securitate (Lockout & Parole)
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
});

// 6. Securizarea Cookie-urilor
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddHostedService<AuctionWorker>();
builder.Services.AddHttpClient();

var app = builder.Build();

// 7. Pipeline-ul de cereri (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication trebuie să fie ÎNAINTE de Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// --- SEEDING LOGIC & MIGRĂRI AUTOMATE (Runtime) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // --- NOU: Aplică migrările automat la pornire (necesar pentru Render) ---
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "A apărut o eroare la aplicarea migrărilor bazei de date.");
    }

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // A. Creare Rol Admin
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // B. Listă Admini de creat
    var adminData = new List<(string Email, string Nume)>
    {
        ("vlad@test.com", "Vlad"),
        ("simon@test.com", "Simon"),
        ("ilinca@test.com", "Ilinca"),
        ("maria@test.com", "Maria"),
        ("ilincamartole@gmail.com", "Ilinca Admin")
    };

    foreach (var data in adminData)
    {
        var user = await userManager.FindByEmailAsync(data.Email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = data.Email,
                Email = data.Email,
                EmailConfirmed = true,
                nume = data.Nume,
                prenume = "Admin",
                adresa = "Adresa Generică Test",
                balance = 0,
                data_inregistrarii = DateTime.Now
            };

            var result = await userManager.CreateAsync(user, "ParolaTest123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
        else
        {
            // Dacă user-ul există deja, ne asigurăm doar că are rolul
            if (!await userManager.IsInRoleAsync(user, "Admin"))
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}

app.MapHub<AuctionHub>("/auctionHub");
app.MapControllers();
app.Run();