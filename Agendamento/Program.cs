using Microsoft.EntityFrameworkCore;
using Agendamento.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços ao contêiner
builder.Services.AddControllers();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Adiciona Swagger
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura o middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agendamento API v1"));
}

app.UseRouting();
app.MapControllers();
app.MapGet("/", () => "Bem-vindo ao Sistema de Agendamento!");

app.Run();
