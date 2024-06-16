using AutoMapper;
using FekraHubAPI.Constract;
using FekraHubAPI.ContractMaker;
using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.Extentions;
using FekraHubAPI.Repositories.Implementations;
using FekraHubAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json.Serialization;
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
builder.Services.AddScoped<ApplicationUsersServices>();

builder.Services.AddTransient<IContractMaker, ContractMaker>();
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromDays(7);
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:3000")//frontend url
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


using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var AllPermissions = Enum.GetValues(typeof(PermissionsEnum.AllPermissions));
    //var permissions = context.AspNetPermissions.ToList();

    builder.Services.AddAuthorization(options =>
    {
        foreach (var permission in AllPermissions)
        {
            options.AddPolicy(permission.ToString(),
            policy => policy.RequireClaim(permission.ToString(), permission.ToString()));
        }
    });
}
// Add services to the container.
builder.Services.AddTransient(typeof(IRepository<>), typeof(GenericRepository<>));
// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.MaxDepth = 64;
    });
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
