using SistemaMVC.Data;

var builder = WebApplication.CreateBuilder(args);

// MVC + sesión (la sesión guarda la reserva / requerimiento en construcción)
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{
    opciones.IdleTimeout = TimeSpan.FromMinutes(60);
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
});

// Capa de acceso a datos (ADO.NET + SQL Server)
builder.Services.AddSingleton<Conexion>();
builder.Services.AddScoped<UsuarioDAO>();
builder.Services.AddScoped<ClienteDAO>();
builder.Services.AddScoped<HabitacionDAO>();
builder.Services.AddScoped<ReservaDAO>();
builder.Services.AddScoped<MaterialDAO>();
builder.Services.AddScoped<RequerimientoDAO>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
