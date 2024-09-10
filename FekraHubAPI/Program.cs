using AutoMapper;
using DinkToPdf;
using DinkToPdf.Contracts;
using FekraHubAPI.CleanTables;
using FekraHubAPI.Constract;
using FekraHubAPI.ContractMaker;
using FekraHubAPI.Data;
using FekraHubAPI.Data.Models;
using FekraHubAPI.EmailSender;
using FekraHubAPI.ExportReports;
using FekraHubAPI.Extentions;
using FekraHubAPI.Filters;
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
using Serilog;
using System;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text.Json.Serialization;
using IEmailSender = FekraHubAPI.EmailSender.IEmailSender;


var builder = WebApplication.CreateBuilder(args);


// Add Connection DataBase.


builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext;

    var connectionString = builder.Configuration.GetConnectionString("ProdConn");

    if (httpContext != null && httpContext.User.Identity.IsAuthenticated)
    {
        var userEmail = httpContext.User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        if (IsDeveloperEmail(userEmail))
        {
            connectionString = builder.Configuration.GetConnectionString("develpConn");
        }
    }

    options.UseSqlServer(connectionString);
});
builder.Services.AddHttpContextAccessor();


builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<ApplicationUsersServices>();
builder.Services.AddTransient<IExportPDF, ExportPDF>();
builder.Services.AddTransient<IContractMaker, ContractMaker>();
builder.Services.AddSingleton (typeof(IConverter),new SynchronizedConverter(new PdfTools() ));
string libPath;
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    libPath = Path.Combine(AppContext.BaseDirectory, "libs", "windows", "libwkhtmltox.dll");
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    libPath = Path.Combine(AppContext.BaseDirectory, "libs", "linux", "libwkhtmltox.so");
}
else
{
    throw new PlatformNotSupportedException("Unsupported OS platform.");
}
if (!File.Exists(libPath))
{
    throw new FileNotFoundException("Library not found.", libPath);
}
NativeLibrary.Load(libPath);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<BrowserOnlyFilter>();
});
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromDays(7);
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:3000", "https://fekrahub.app")//frontend url
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});
builder.Services.AddHostedService<CleaneUsersTable>();
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80); // HTTP
    options.ListenAnyIP(443);
});
//Adding Authentication 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
});

builder.Host.UseSerilog((context, configuration) =>
      configuration
      .ReadFrom.Configuration(context.Configuration)
      );

using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var AllPermissions = Enum.GetValues(typeof(PermissionsEnum.AllPermissions));
    //var permissions = context.AspNetPermissions.ToList();

    builder.Services.AddAuthorization(options =>
    {
        foreach (var permission in AllPermissions)
        {
            options.AddPolicy(permission.ToString(), policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permissions" &&
                                               c.Value.Contains(permission.ToString()))
                )
            );
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


app.Use(async (context, next) =>
{
    var userEmail = context.User?.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

    if (!string.IsNullOrEmpty(userEmail) && IsDeveloperEmail(userEmail))
    {
        context.Items["ConnectionString"] = builder.Configuration.GetConnectionString("develpConn");
    }
    else
    {
        context.Items["ConnectionString"] = builder.Configuration.GetConnectionString("ProdConn");
    }

    await next.Invoke();
});
bool IsDeveloperEmail(string email)
{
    var developerEmails = new List<string> {
            "mlolo5041@gmail.com",
            "htarbouch7@gmail.com",
            "remon4445@gmail.com",
            "hlovellcharles@gmail.com",
            "abog9022@gmail.com",
            "abog546a1@gmail.com",
            "yousefeldada@gmail.com",
            "abog5464@gmail.com",
            "abog5461@gmail.com",
            "remoanff@gmail.com",
            "hatha.ana.com.net.sy@gmail.com",
            "francisdani935@gmail.com",
            "caryer@gmail.com",
            "abog5463@gmail.com",
            "halanabeel76@gmail.com",
            "info@netwitcher.com"};
    return developerEmails.Contains(email);
}




app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run("http://0.0.0.0:80");
