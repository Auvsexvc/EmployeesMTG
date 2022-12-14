using MTGWebApi.Data;
using MTGWebApi.Interfaces;
using MTGWebApi.MiddleWare;
using MTGWebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddControllers();
builder.Services.AddHostedService<AppDbInitializer>();
builder.Services.AddSingleton<IAppDbFileHandler, AppDbFileHandler>();
builder.Services.AddScoped<IAppDbContext, AppDbContext>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();