using DhanAlgoTrading.Api.Services;
using DhanAlgoTrading.Models.Configuration;
using DhanAlgoTrading.Services;


// using Microsoft.Extensions.Options; // Not directly needed in Program.cs for this setup
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200") // Your Angular app's origin
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                          // For development, you might allow any origin:
                          // policy.AllowAnyOrigin()
                          //       .AllowAnyHeader()
                          //       .AllowAnyMethod();
                      });
});


// Add services to the container.

// 1. Configure DhanApiSettings from appsettings.json
builder.Services.Configure<DhanApiSettings>(builder.Configuration.GetSection("DhanApiSettings"));

// 2. Register HttpClient for IDhanService, implemented by DhanService
// This makes HttpClient available via constructor injection in DhanService.
// The HttpClient's lifetime is managed by the factory.
builder.Services.AddHttpClient<IDhanService, DhanService>()
    .AddPolicyHandler(GetRetryPolicy()); // Add Polly retry policy
builder.Services.AddHostedService<LiveOrderUpdateService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage(); // Shows detailed errors in development
}

app.UseHttpsRedirection();

app.UseRouting(); // Make sure UseRouting is called before UseCors

app.UseCors(MyAllowSpecificOrigins); // Apply the CORS policy

app.UseAuthorization(); // Placeholder, actual auth setup would be more complex

app.MapControllers();

app.Run();

// Polly Retry Policy Definition
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // Handles HttpRequestException, 5XX results, 408 RequestTimeout
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Handle 429 (Rate Limit)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff: 2s, 4s, 8s
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                // Log retry attempts. Using Console.WriteLine for simplicity in a static method.
                // In a real app, you might pass an ILogger via context if needed.
                Console.WriteLine($"Retrying API call to {outcome.Result?.RequestMessage?.RequestUri} due to {outcome.Result?.StatusCode}, attempt {retryAttempt} after {timespan.TotalSeconds}s");
            });
}