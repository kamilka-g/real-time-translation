using EchoTravel.Api.Adapters;
using EchoTravel.Api.Hubs;
using EchoTravel.Api.Ports;
using EchoTravel.Api.Services;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Configure the translator client and settings
builder.Services.AddHttpClient<Translator>();
builder.Services.AddSingleton<Translator>();

// Configure the STT client adapter; URL comes from configuration
builder.Services.AddHttpClient<FastApiSttClient>(c =>
{
    var baseUrl = builder.Configuration["SttService:BaseUrl"] ?? "http://localhost:8001";
    c.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddSingleton<ISttClient>(sp => sp.GetRequiredService<FastApiSttClient>());

// Core service that orchestrates transcription, translation and broadcasting
builder.Services.AddSingleton<AnnouncementsService>();

// Enable CORS for development; restrict origins in production as needed
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true)));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapHub<AnnouncementsHub>("/hubs/announcements");
app.MapGet("/", () => "EchoTravel API running");

app.Run();
