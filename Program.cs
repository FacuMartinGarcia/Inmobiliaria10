using Inmobiliaria10.Data;
using Inmobiliaria10.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IPropietarioRepo, PropietarioRepo>();

// (Opcional) si querés inyectar Database en otras clases
builder.Services.AddSingleton<Database>();

var app = builder.Build();

// --- Pipeline HTTP ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();          // en lugar de MapStaticAssets
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// --- Test de conexión a MySQL (opcional) ---
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<Database>(); // ctor Database(IConfiguration)
    using var conn = db.GetConnection();
    conn.Open();
    Console.WriteLine("Conexión abierta correctamente a MySQL.");
}
catch (Exception ex)
{
    Console.WriteLine("Error al conectar a MySQL: " + ex.Message);
}

app.Run();
