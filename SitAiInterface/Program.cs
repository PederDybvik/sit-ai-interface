using Microsoft.OpenApi.Models;
using SitAiInterface.Services;
using System.Text.Json.Serialization;

// Railway (and similar platforms) expose the port via the PORT env variable.
// ASP.NET Core uses ASPNETCORE_URLS, so we bridge them here.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://+:{port}");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SIT AI Chatbot API",
        Version = "v1",
        Description = "AI-powered search assistant for SIT (Studentsamskipnaden i Gjøvik, Ålesund og Trondheim). " +
                      "Answers questions about student housing, health, fitness, associations, and more."
    });
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
        $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml"), includeControllerXmlComments: true);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<KnowledgeBaseService>();
builder.Services.AddScoped<ChatService>();

var app = builder.Build();

// Eagerly initialise the knowledge base at startup so the first request isn't slow
app.Services.GetRequiredService<KnowledgeBaseService>();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIT AI Chatbot API v1"));

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
