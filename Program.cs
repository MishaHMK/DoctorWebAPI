using DoctorWebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using DoctorWebApi.Interfaces;
using DoctorWebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using DoctorWebApi.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using DoctorWebApi.Provider;

var builder = WebApplication.CreateBuilder();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddSession();

builder.Services.AddAuthentication().AddJwtBearer();

builder.Services.AddTransient<IAppointmentService, AppointmentService>();

builder.Services.AddTransient<IJwtService, JwtService>();

builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddControllers();

builder.Services.AddMvc().AddSessionStateTempDataProvider();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.Configure<HostUrlOptions>(builder.Configuration.GetSection("URLs"));

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<User, IdentityRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    opt.SignIn.RequireConfirmedEmail = true;
    opt.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddTokenProvider<EmailConfirmationTokenProvider<User>>("emailconfirmation");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseCors("AllowOrigin");

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseAuthentication();

app.MapControllers();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
