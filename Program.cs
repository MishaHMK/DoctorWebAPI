using DoctorWebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using DoctorWebApi.Interfaces;
using DoctorWebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder();

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddSession();

builder.Services.AddAuthentication().AddJwtBearer();

builder.Services.AddTransient<IAppointmentService, AppointmentService>();

builder.Services.AddTransient<IJwtService, JwtService>();

builder.Services.AddControllers();

builder.Services.AddMvc().AddSessionStateTempDataProvider();

builder.Services.AddSession();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

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

app.UseSession();

app.UseAuthorization();

app.UseAuthentication();

app.MapControllers();

app.UseSession();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
