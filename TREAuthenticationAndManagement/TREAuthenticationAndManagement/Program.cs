using Microsoft.Extensions.Configuration;
using TRE_TESK.Models;
using TRE_TESK.Services;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

var AuthenticationSettings = new AuthenticationSettings();
configuration.Bind(nameof(AuthenticationSettings), AuthenticationSettings);
builder.Services.AddSingleton(AuthenticationSettings);

var HasuraSettings = new HasuraSettings();
configuration.Bind(nameof(HasuraSettings), HasuraSettings);
builder.Services.AddSingleton(HasuraSettings);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddScoped<IHasuraService, HasuraService>();
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHsts();
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
