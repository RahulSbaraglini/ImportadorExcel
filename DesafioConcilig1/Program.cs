var builder = WebApplication.CreateBuilder(args);

// Adiciona servi�os ao container
builder.Services.AddControllersWithViews();
builder.Services.AddSession(); // Habilita sess�o

var app = builder.Build();

// Configura��o do pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Middleware de sess�o deve vir antes do UseAuthorization
app.UseAuthorization();

// Define a rota padr�o para a tela de login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
