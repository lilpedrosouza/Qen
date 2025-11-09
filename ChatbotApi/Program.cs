using ChatbotApi.Configuration;
using ChatbotApi.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ==================== CONFIGURAÇÃO DA OPENAI ====================
builder.Services.Configure<OpenAISettings>(Options =>
{
    Options.ApiKey = builder.Configuration["OpenAI:ApiKey"]
    ?? throw new InvalidOperationException("OpenAI API key não configurada.");

    Options.Model = builder.Configuration["OpenAI:Model"] ?? "gpt-4o-mini";
});

// ==================== REGISTRO DE SERVIÇOS ====================
builder.Services.AddScoped<IChatService, ChatService>();

// ==================== CONFIGURAÇÃO DE CORS ====================
builder.Services.AddCors(Options =>
{
    Options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
    });
});

// ==================== CONFIGURAÇÃO DE CONTROLLERS ====================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.WriteIndented = true;
});

// ==================== SWAGGER/OPENAPI ====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(Options =>
{
    Options.SwaggerDoc("v1", new()
    {
        Title = "Chatbot API",
        Version = "v1",
        Description = "API para um chatbot utilizando OpenAI.",
        Contact = new()
        {
            Name = "Pedro Souza",
            Email = "pedrohenrique.nasci.souza2020@gmail.com"
        }
    });
});

// ==================== LOGGING ====================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Define o nível de log
if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
}

// ==================== BUILD DA APLICAÇÃO ====================
var app = builder.Build();

// ==================== PIPELINE DE MIDDLEWARE ====================

// Swagger apenas em desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Chatbot API v1");
        options.RoutePrefix = string.Empty; // Swagger na raiz (https://localhost:PORT/)
    });
    
    // Usa CORS menos restritivo em desenvolvimento
    app.UseCors("AllowAll");
}
else
{
    // Em produção, use CORS mais restritivo
    app.UseCors("AllowFrontend");
    
    // Tratamento de erros em produção
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Força HTTPS
app.UseHttpsRedirection();

// Habilita autorização (se implementar autenticação no futuro)
app.UseAuthorization();

// Mapeia os controllers
app.MapControllers();

// Endpoint de erro global
app.Map("/error", (HttpContext context) =>
{
    return Results.Problem(
        title: "Ocorreu um erro no servidor",
        statusCode: StatusCodes.Status500InternalServerError
    );
});

// ==================== LOG DE INICIALIZAÇÃO ====================
app.Logger.LogInformation("===========================================");
app.Logger.LogInformation("🚀 Chatbot API Iniciada!");
app.Logger.LogInformation("Ambiente: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("Modelo OpenAI: {Model}", 
    builder.Configuration["OpenAI:Model"] ?? "gpt-4o-mini");

if (app.Environment.IsDevelopment())
{
    app.Logger.LogInformation("📚 Swagger UI: https://localhost:{Port}", 
        builder.Configuration["Kestrel:Endpoints:Https:Url"] ?? "5001");
}

app.Logger.LogInformation("===========================================");

// ==================== INICIA A APLICAÇÃO ====================
app.Run();