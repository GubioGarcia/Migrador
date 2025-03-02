using Migrador.Application.Services;
using Migrador.Application.Interfaces;
using Migrador.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Inje��o de depend�ncia
builder.Services.AddScoped<EtapaMigradorService>();
builder.Services.AddScoped<RespostaMigradorService>();
builder.Services.AddScoped<IManipularArquivoService, ManipularArquivoService>();

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
