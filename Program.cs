using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DoctorWebApi.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using AutoMapper;
using Doctor.DataAcsess.Interfaces;
using Doctor.DataAcsess.Entities;
using Doctor.DataAcsess;
using DoctorWebApi.EmailProvider;
using DoctorWebApi.Mapper;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Doctor.BLL.Interface;
using Doctor.BLL.Services;
using Doctor.DataAcsess.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Services.AddIdentityCore<User>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    opt.SignIn.RequireConfirmedEmail = true;
})
            .AddRoles<IdentityRole>()
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddSignInManager<SignInManager<User>>()
            .AddRoleValidator<RoleValidator<IdentityRole>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
        {
            var Key = Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]);
            o.SaveToken = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidIssuer = builder.Configuration["JWT:Issuer"],
                ValidAudience = builder.Configuration["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Key)
            };
});

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddAutoMapper(typeof(Program));

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

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

builder.Services.AddTransient<IAccountService, AccountService>();

builder.Services.AddTransient<IJWTService, JWTService>();

builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddTransient<IMessageService, MessageService>();

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


var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AllMappersProfile());
});
IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseHsts();
//app.UseCors();

app.MapControllers();

app.Run();
