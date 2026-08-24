using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PawTrack.Api.Data;
using PawTrack.Api.Services;
using System.Text;
using System.Text.Json.Serialization;

namespace PawTrack.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.ParameterLocation.Header,
                    Description = "Paste just the token here — Swagger adds 'Bearer ' automatically."
                });

                options.AddSecurityRequirement(document => new()
                {
                    [new("Bearer", document)] = new List<string>()
                });
            });
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

            

            builder.Services.AddDbContext<PawTrackDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));




            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });
            builder.Services.AddAuthorization();
            builder.Services.AddScoped<IAnimalService, AnimalService>();
            builder.Services.AddScoped<IMedicalService, MedicalService>();
            builder.Services.AddScoped<IBehaviorService, BehaviorService>();
            builder.Services.AddScoped<IVisitService, VisitService>();
            builder.Services.AddScoped<IAdoptionService, AdoptionService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IAiDescriptionService, AiDescriptionService>();
            builder.Services.AddScoped<IChatbotService, ChatbotService>();
            builder.Services.AddScoped<IReportService, ReportService>();

            // CORS — allows the PawTrack.Web Razor Pages frontend to call this API
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("PawTrackWeb", policy =>
                {
                    policy.WithOrigins(
                            builder.Configuration["Cors:WebOrigin"] ?? "https://localhost:7100",
                            "http://localhost:5100")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {

                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("PawTrackWeb");

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
