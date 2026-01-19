using Gplant.API.ApiResponse;
using Gplant.API.Handlers;
using Gplant.Application.Abstracts;
using Gplant.Application.Security;
using Gplant.Application.Services;
using Gplant.Domain.Constants;
using Gplant.Domain.Entities;
using Gplant.Infrastructure;
using Gplant.Infrastructure.Options;
using Gplant.Infrastructure.Processors;
using Gplant.Infrastructure.Repositories;
using Gplant.Infrastructure.Seed;
using Gplant.Infrastructure.Seeds;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();


builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.JwtOptionsKey));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.EmailOptionsKey));

builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnectionString")));

builder.Services.AddScoped<IAuthTokenProcessor, AuthTokenProcessor>();
builder.Services.AddScoped<IEmailProcessor, EmailProcessor>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IOTPService, OTPService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<IOTPGenerator, OTPGenerator>();
builder.Services.AddScoped<IOTPRepository, OTPRepository>();
builder.Services.AddScoped<IActionTokenRepository, ActionTokenRepository>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme   = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme      = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme         = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddGoogle(options =>
    {
        var clientId = builder.Configuration["Authentication:Google:ClientId"];

        if (string.IsNullOrEmpty(clientId)) throw new ArgumentNullException(null, nameof(clientId));

        var clientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

        if (string.IsNullOrEmpty(clientSecret)) throw new ArgumentNullException(null, nameof(clientSecret));

        options.ClientId        = clientId;
        options.ClientSecret    = clientSecret;
        options.SignInScheme    = CookieAuthenticationDefaults.AuthenticationScheme;

        options.ClaimActions.MapJsonKey("image", "picture");

        options.Events.OnRemoteFailure = context =>
        {
            context.HandleResponse();

            context.Response.Redirect("http://localhost:3000/sign-in");

            return Task.CompletedTask;
        };
    })
    .AddFacebook(options =>
    {
        var clientId = builder.Configuration["Authentication:Facebook:ClientId"];

        if (string.IsNullOrEmpty(clientId)) throw new ArgumentNullException(null, nameof(clientId));

        var clientSecret = builder.Configuration["Authentication:Facebook:ClientSecret"];

        if (string.IsNullOrEmpty(clientSecret)) throw new ArgumentNullException(null, nameof(clientSecret));

        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.Events.OnRemoteFailure = context =>
        {
            context.HandleResponse();

            context.Response.Redirect("http://localhost:3000/sign-in");

            return Task.CompletedTask;
        };
    })
    .AddMicrosoftAccount(options =>
    {
        var clientId = builder.Configuration["Authentication:Microsoft:ClientId"];

        if (string.IsNullOrEmpty(clientId)) throw new ArgumentNullException(null, nameof(clientId));

        var clientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];

        if (string.IsNullOrEmpty(clientSecret)) throw new ArgumentNullException(null, nameof(clientSecret));

        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.Events.OnRemoteFailure = context =>
        {
            context.HandleResponse();

            context.Response.Redirect("http://localhost:3000/sign-in");

            return Task.CompletedTask;
        };
    })
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection(JwtOptions.JwtOptionsKey)
                                              .Get<JwtOptions>() ?? throw new ArgumentException(nameof(JwtOptions));

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["ACCESS_TOKEN"];
                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                context.HandleResponse();

                context.Response.StatusCode     = StatusCodes.Status401Unauthorized;
                context.Response.ContentType    = "application/json";

                return context.Response.WriteAsJsonAsync(new ErrorResponse(
                    StatusCode: 401,
                    Error:      "Unauthorized",
                    Message:    "Authentication required. Please log in to continue.",
                    Timestamp:  DateTime.UtcNow
                ));
            },

            OnForbidden = context =>
            {
                context.Response.StatusCode     = StatusCodes.Status403Forbidden;
                context.Response.ContentType    = "application/json";

                return context.Response.WriteAsJsonAsync(new ErrorResponse(
                    StatusCode: 403,
                    Error:      "Forbidden",
                    Message:    "You do not have permission to access this resource.",
                    Timestamp:  DateTime.UtcNow
                ));
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.Admin))
    .AddPolicy("AdminOrManager", policy => policy.RequireRole(Roles.Admin, Roles.Manager));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    await RoleSeeder.SeedAsync(roleManager);
    await UserSeeder.SeedAdminAsync(userManager);
}

app.UseExceptionHandler(_ => { });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
