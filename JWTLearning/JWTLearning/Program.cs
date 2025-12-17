
using JWTLearning.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

//  [ Mapping Data ] JWT config binding
builder.Services.Configure<JWT>(
    builder.Configuration.GetSection("JWT")
);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
