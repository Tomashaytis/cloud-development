using CourseManagement.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins("https://localhost:7282")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Ass course generator
builder.Services.AddSingleton<CourseGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseCors("AllowClient");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// API Endpoint
app.MapGet("/land-plot", (int? id, CourseGenerator generator, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("Request received with id: {Id}", id);
        var course = generator.GenerateOne(id);

        logger.LogInformation("The course was successfully generated: {Title}", course.Title);
        return Results.Ok(course);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during course generation");
        return Results.Problem("Internal server error", statusCode: 500);
    }
});

app.MapDefaultEndpoints();

app.Run();