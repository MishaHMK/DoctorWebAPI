using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DoctorWebApi.Interfaces;
using DoctorWebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity.UI.Services;
using IdentityServer4.AccessTokenValidation;
using AutoMapper;
using Doctor.DataAcsess.Interfaces;
using Doctor.DataAcsess.Entities;
using Doctor.DataAcsess;
using DoctorWebApi.EmailProvider;
using DoctorWebApi.Mapper;
using Microsoft.OpenApi.Models;
using DoctorWebApi.Helper;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddAutoMapper(typeof(Program));

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AllMappersProfile());
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        {
            p.WithOrigins(
                "http://localhost:3000"
            ); 
            p.AllowAnyHeader();
            p.AllowAnyMethod();
            p.AllowCredentials();

            p.SetPreflightMaxAge(TimeSpan.FromDays(1));
        }
));

builder.Services.AddTransient<IAppointmentService, AppointmentService>();

builder.Services.AddTransient<IAccountService, AccountService>();

builder.Services.AddTransient<IJwtService, JwtService>();

builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddScoped<IMessageRepository, MessageRepository>();

builder.Services.AddControllers();

builder.Services.AddMvc().AddSessionStateTempDataProvider();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<HostUrlOptions>(builder.Configuration.GetSection("URLs"));

builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);

builder.Services.AddIdentity<User, IdentityRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    opt.SignIn.RequireConfirmedEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.\nExample: 'Bearer {your token}'"
    });
});


//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Events.OnRedirectToLogin = (context) =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });

builder.Services.ConfigureOptions<JwtOptionsSetup>();
builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseHsts();
app.UseCors();

app.MapControllers();

app.Run();
