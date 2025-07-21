using Microsoft.Extensions.Configuration;
using Tre_Camunda.Models;
using Tre_Camunda.Services;
using Zeebe.Client;


var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var zeebeSettings = new ZeebeSettings();
configuration.Bind(nameof(ZeebeSettings), zeebeSettings);
builder.Services.AddSingleton(zeebeSettings);


var zeebeClient = ZeebeClient.Builder()
    .UseGatewayAddress(zeebeSettings.Address)
    .UsePlainText()
    .Build();

builder.Services.AddSingleton<IZeebeClient>(zeebeClient);


builder.Services.AddScoped<IZeebeDmnService, ZeebeDmnService>();
builder.Services.AddHostedService<DmnStartupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
