using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using UVVConsultas.Data;

var builder = WebApplication.CreateBuilder(args);

// ----- Injeção de Dependência -----

// MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

// DbContext (EF Core + SQL Server) registrado no contêiner de DI
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Autenticação baseada em Cookies (login manual contra a tabela Usuarios)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Conta/Login";
        options.AccessDeniedPath = "/Conta/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Autorização
builder.Services.AddAuthorization();

var app = builder.Build();

// ----- Pipeline de Middleware -----

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// IMPORTANTE: UseAuthentication() sempre antes de UseAuthorization()
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
