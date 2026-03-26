using System.Runtime.InteropServices;
using DotNetEnv;
using Npgsql;
using PROXY_TODB.DBInterfaces;
using PROXY_TODB.DBmethods;
using PROXY_TODB.InitService;
using PROXY_TODB.Models;

var builder = WebApplication.CreateBuilder(args);

Env.Load("settings.env");

FullProgramModel.MasterConnectionString = Environment.GetEnvironmentVariable("masterconn");
FullProgramModel.Host = Environment.GetEnvironmentVariable("host");
FullProgramModel.Port = Environment.GetEnvironmentVariable("port");
FullProgramModel.ManagementDBName = Environment.GetEnvironmentVariable("managementdb");

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IcreateDB, CreateDBaction>();
builder.Services.AddScoped<IremoveDB, RemoveDBaction>();
builder.Services.AddHostedService<InitManagamentDB>();
builder.Services.AddScoped<IexecScript, ExecScript>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();

app.Run();