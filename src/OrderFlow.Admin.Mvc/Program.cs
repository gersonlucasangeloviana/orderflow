var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
var app = builder.Build();
app.UseExceptionHandler("/Home/Error"); app.UseStaticFiles(); app.UseRouting();
app.MapControllerRoute("default", "{controller=Orders}/{action=Index}/{id?}");
app.Run();
