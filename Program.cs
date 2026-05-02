using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Servicii de baz?
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// 2. Configurare Baz? de Date
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Configurare Identity cu suport pentru 2FA (AddDefaultTokenProviders este cheia)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// 4. Serviciul de Email (Necesar pentru trimiterea codurilor 2FA prin mail)
builder.Services.AddTransient<IEmailSender, EmailSender>();

// 5. Configur?ri de Securitate Avansate (Lockout & Parole)
builder.Services.Configure<IdentityOptions>(options =>
{
    // Set?ri Lockout (Brute Force Protection)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Set?ri extra pentru securitatea parolei
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
});

// 6. Securizarea Cookie-urilor (Previne atacurile de tip Session Hijacking)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true; // JavaScript-ul nu poate fura cookie-ul
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // For?eaz? HTTPS
});

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

// Importante pentru Identity: Authentication trebuie s? fie ÎNAINTE de Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // 1. Creeaz? rolul dac? nu exist?
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // 2. Pune-?i email-ul t?u aici pentru a deveni Admin
    var adminUser = await userManager.FindByEmailAsync("ilincamartole@gmail.com");
    if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

app.Run();