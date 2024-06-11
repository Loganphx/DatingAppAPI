using API.Data;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Middleware;
using API.Services;
using API.SignalR;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<DataContext>(opt =>
{
     opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddCors();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddScoped<ITokenService, TokenService>();
// builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<LogUserActivity>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));
builder.Services.AddSignalR();
builder.Services.AddSingleton<PresenceTracker>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
// Configure the HTTP request pipeline.
 if (app.Environment.IsDevelopment())
 {
     //app.UseDeveloperExceptionPage();
     app.UseSwagger();
     app.UseSwaggerUI();
 }
app.UseCors(builder => builder
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()
     .WithOrigins("https://localhost:4200"));
 
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");

using var scope    = app.Services.CreateScope();
var       services = scope.ServiceProvider;
try
{
     var logger = services.GetService<ILogger<Seed>>();
     var context = services.GetRequiredService<DataContext>();
     var userManager = services.GetRequiredService<UserManager<AppUser>>();
     var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
     await context.Database.MigrateAsync();
     await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]");
     await context.Database.ExecuteSqlRawAsync("DELETE FROM [Groups]");
     await Seed.SeedUsers(logger, userManager, roleManager);
}
catch (Exception e)
{
     var logger = services.GetService<ILogger<Program>>();
     logger.LogError(e, "An error occured during migrations");
}

app.Run();
