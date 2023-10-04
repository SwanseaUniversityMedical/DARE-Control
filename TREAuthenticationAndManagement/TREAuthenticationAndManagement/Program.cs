using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.SqlServer;
using TRE_TESK.Models;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

var AuthenticationSettings = new AuthenticationSettings();
configuration.Bind(nameof(AuthenticationSettings), AuthenticationSettings);
builder.Services.AddSingleton(AuthenticationSettings);


builder.Services.AddDbContext<ApplicationDbContext>(options => options
    .UseLazyLoadingProxies(true)
    .UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")
));

builder.Services.AddDbContext<ApplicationDbContext>();
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHsts();
}



using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.MapRazorPages();

app.Run();
