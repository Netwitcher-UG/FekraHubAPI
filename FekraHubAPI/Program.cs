using AutoMapper;
using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.Extentions;
using FekraHubAPI.Repositories.Implementations;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
using IEmailSender = FekraHubAPI.EmailSender.IEmailSender;


var builder = WebApplication.CreateBuilder(args);


// Add Connection DataBase.
#if DEBUG
builder.Services.AddDbContext<ApplicationDbContext>(op =>
      op.UseSqlServer(builder.Configuration.GetConnectionString("develpConn")));
#else
builder.Services.AddDbContext<ApplicationDbContext>(op =>
      op.UseSqlServer(builder.Configuration.GetConnectionString("myConn")));
#endif

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromDays(7);
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:5142")//frontend url
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});
//Adding Authentication 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("value", policy =>
        policy.RequireClaim("type", "value"));
});
// Add services to the container.
builder.Services.AddTransient(typeof(IRepository<>), typeof(GenericRepository<>));
// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGenJwtAuth();

builder.Services.AddCustomJwtAuth(builder.Configuration);


builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 209715200; // Limit upload size to 200 MB
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
