using Inmobiliaria10.Data;
using Inmobiliaria10.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Inmobiliaria10.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Database>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuario/Login";       // redirigir si no está logueado
        options.LogoutPath = "/Usuario/Logout";     // logout
        options.AccessDeniedPath = "/Home/Restringido"; // acceso denegado
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy =>
        policy.RequireRole("Administrador"));
});
builder.Services.Configure<Microsoft.AspNetCore.Identity.IdentityOptions>(options =>
{
    options.ClaimsIdentity.RoleClaimType = System.Security.Claims.ClaimTypes.Role;
});
builder.Services.AddScoped<IConceptoRepo, ConceptoRepo>();
builder.Services.AddScoped<IContratoRepo, ContratoRepo>();
builder.Services.AddScoped<IInquilinoRepo, InquilinoRepo>();
builder.Services.AddScoped<IInmuebleRepo, InmuebleRepo>();
builder.Services.AddScoped<IInmuebleTipoRepo, InmuebleTipoRepo>();
builder.Services.AddScoped<IInmuebleUsoRepo, InmuebleUsoRepo>();
builder.Services.AddScoped<IImagenRepo, ImagenRepo>();
builder.Services.AddScoped<IPagoRepo, PagoRepo>();
builder.Services.AddScoped<IMesRepo, MesRepo>();
builder.Services.AddScoped<IPropietarioRepo, PropietarioRepo>();
builder.Services.AddScoped<IRolRepo, RolRepo>();
builder.Services.AddScoped<IUsuarioRepo, UsuarioRepo>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IImageStorageService, LocalImageStorageService>();
builder.Configuration.AddEnvironmentVariables(); 

var app = builder.Build();

// --- Pipeline HTTP ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();        
app.UseRouting();

//app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/Home/MostrarCodigo", "?code={0}");

app.UseAuthentication(); // habilitar autenticación
app.UseAuthorization();  // habilitar autorización
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.MapControllerRoute(
    name: "contratos-alias",
    pattern: "Contratos/{action=Index}/{id?}",
    defaults: new { controller = "Contrato" }
);


try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<Database>(); 
    using var conn = db.GetConnection();
    conn.Open();
    Console.WriteLine("Conexión abierta correctamente a MySQL.");
}
catch (Exception ex)
{
    Console.WriteLine("Error al conectar a MySQL: " + ex.Message);
}

app.Run();
