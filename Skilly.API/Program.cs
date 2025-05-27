using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Skilly.Application.Abstract;
using Skilly.Application.Implementation;
using Skilly.Application.Middlewares;
using Skilly.Core.Entities;
using Skilly.Infrastructure.Abstract;
using Skilly.Infrastructure.Implementation;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using Skilly.Persistence.Implementation;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
using Skilly.API.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Skilly.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("cs")));



            builder.Services.Configure<PaymobSettings>(builder.Configuration.GetSection("Paymob"));

            builder.Services.AddIdentity<User, IdentityRole>()
                           .AddEntityFrameworkStores<ApplicationDbContext>()
                           .AddDefaultTokenProviders();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });
            builder.Services.AddDistributedMemoryCache();



            var firebaseServiceAccountPath = builder.Configuration["FirebaseConfig:ServiceAccountFile"];

            var firebasePath = Path.Combine(Directory.GetCurrentDirectory(), firebaseServiceAccountPath);

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(firebasePath)
                });
            }


            var smtpSettings = builder.Configuration.GetSection("SMTP");
            builder.Services.AddSingleton<IEmailService>(new EmailService(
                smtpSettings["Server"],
                int.Parse(smtpSettings["Port"]),
                smtpSettings["User"],
                smtpSettings["Password"]
            ));

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IGenericRepository<User>, UserRepository>();
            builder.Services.AddScoped<IUserProfileRepository,UserProfileRepository>();
            builder.Services.AddScoped<IServiceProviderRepository,ServiceProviderRepository>();
            builder.Services.AddScoped<IServicegalleryRepository,servicegalleryRepository>();
            builder.Services.AddScoped<IProviderServicesRepository,ProviderServiceRepository>();
            builder.Services.AddScoped<IReviewRepository,ReviewRepository>();
            builder.Services.AddScoped<ICategoryRepository,CategoryRepository>();
            builder.Services.AddScoped<IRequestserviceRepository,RequestserviceRepository>();
            builder.Services.AddScoped<IOfferSalaryRepository, OfferSalaryRepository>();


            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IClaimsService,ClaimsService>();
            builder.Services.AddScoped<IImageService,ImageService>();
            builder.Services.AddScoped<IAuthService,AuthService>();
            builder.Services.AddScoped<IChatService,ChatService>(); 
            builder.Services.AddScoped<IBannerService,BannerService>();
            builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

            builder.Services.AddScoped<FirebaseV1Service>();
            builder.Services.AddScoped<PaymobService>();

            builder.Services.AddAutoMapper(typeof(MappingProfile));
            // builder.Services.AddTransient<ExceptionMiddleware>();

            // Add services to the container.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", builder =>
                {
                    builder
                        .SetIsOriginAllowed(_ => true) // يسمح بأي Origin
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials(); // مهم جداً في حالة WebSocket
                });
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = long.MaxValue;
            });
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = null; 
                options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
            });
            builder.Services.AddLogging();


            builder.Services.AddControllers();

            builder.Services.AddSignalR().AddHubOptions<ChatHub>(options =>
            {
                options.EnableDetailedErrors = true;

            });


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
               
                c.CustomSchemaIds(type => type.FullName);
            });


            /*-----------------------------Swagger Part-----------------------------*/
            #region Swagger REgion

            // إضافة خدمات Swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Skilly API", Version = "v1" });

                // Add support for bearer token authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter 'Bearer' followed by a space and the token.",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
                { new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
            new string[] {}
        }
                });
            });

            #endregion

            builder.Services.AddLogging();
            builder.Logging.AddConsole();
            var app = builder.Build();

            //app.UseMiddleware<LocalizationMiddleware>();    
            app.UseRequestLocalization(options =>
            {
                options.SetDefaultCulture("ar-EG")
                       .AddSupportedCultures("ar-EG", "en-US")
                       .AddSupportedUICultures("ar-EG", "en-US");
            });

            
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseSession();
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            app.Use(async (context, next) =>
            {
                Console.WriteLine($"Incoming Request: {context.Request.Path}");
                await next();
                Console.WriteLine($"Response Status: {context.Response.StatusCode}");
            });

            app.UseRouting();
            app.UseCors("MyPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHub<ChatHub>("/chatHub");
            app.MapControllers();

            app.Run();
        }
    }
}
