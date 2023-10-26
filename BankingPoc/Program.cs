using BankingPoc;
using BankingPoc.Data;
using BankingPoc.Data.Interfaces;
using BankingPoc.Data.Options;
using BankingPoc.Services;
using BankingPoc.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<AccountConfigOptions>(builder.Configuration.GetSection("AccountConfig"));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddDbContext<BankingContext>(
    options => options.UseInMemoryDatabase("name=InMemory"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCustomExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();