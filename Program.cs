using Inmobiliaria10.Data;
using Inmobiliaria10.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Inmobiliaria10.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Database>();

builder.Services.AddScoped<IConceptoRepo, ConceptoRepo>();
builder.Services.AddScoped<IContratoRepo, ContratoRepo>();
builder.Services.AddScoped<IInquilinoRepo, InquilinoRepo>();
builder.Services.AddScoped<IInmuebleRepo, InmuebleRepo>();
builder.Services.AddScoped<IInmuebleTipoRepo, InmuebleTipoRepo>();
builder.Services.AddScoped<IInmuebleUsoRepo, InmuebleUsoRepo>();
builder.Services.AddScoped<IImagenRepo, ImagenRepo>();
builder.Services.AddScoped<IPagoRepo, PagoRepo>();
builder.Services.AddScoped<IPropietarioRepo, PropietarioRepo>();
builder.Services.AddScoped<IRolRepo, RolRepo>();
builder.Services.AddScoped<IUsuarioRepo, UsuarioRepo>();
builder.Services.AddScoped<IEmailService, EmailService>();



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
app.UseAuthentication();
//app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/Home/MostrarCodigo", "?code={0}");

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
