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

var builder = WebApplication.CreateBuilder();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

})
    .AddJwtBearer(o =>
    {
        o.SaveToken = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Df6YcHO4ZiHEMWM4IN0cnWwbM"))
        };
        o.Events = new JwtBearerEvents()
        {
            OnAuthenticationFailed = async context =>
            {
                var ex = context.Exception;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSession();

builder.Services.AddAutoMapper(typeof(Program));

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AllMappersProfile());
});

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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.Configure<HostUrlOptions>(builder.Configuration.GetSection("URLs"));

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

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

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(x => x
           .AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());

app.UseDefaultFiles();

app.UseStaticFiles();

//app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

//app.MapControllers();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
