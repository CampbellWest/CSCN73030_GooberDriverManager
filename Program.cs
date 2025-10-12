var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();   
builder.Services.AddSwaggerGen();            

var app = builder.Build();

// home page return 200
app.MapGet("/", () => Results.Text("DriverManagement API is running"));

// Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DriverManagement API v1");
    c.RoutePrefix = "swagger"; // browse /swagger
});

// Map attribute-routed controllers
app.MapControllers();

app.Run();