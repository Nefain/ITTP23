using ITTP23.Models;
using ITTP23.Servise;
using ITTP23.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ITTP23
{
    public class Startup
    {
        IConfigurationRoot configurationRoot;

        public Startup(Microsoft.AspNetCore.Hosting.IHostingEnvironment configuration)
        {
            configurationRoot = new ConfigurationBuilder().SetBasePath(configuration.ContentRootPath).AddJsonFile("appsettings.json").Build();


        }
        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(option => option.EnableEndpointRouting = false);
            services.AddEntityFrameworkSqlite().AddDbContext<AutoDataContext>(options => options.UseSqlite(configurationRoot.GetConnectionString("DefaultConnection")));
            services.AddTransient<IUserService, UserService>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "User API", Version = "v1" });
                c.EnableAnnotations();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Авторизация(Введите свой токен)'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });
            DatabaseMigrator.MigrateDatabase(services.BuildServiceProvider());
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "User API V1");
                c.RoutePrefix = string.Empty;

                c.DocumentTitle = "User API";
                c.InjectStylesheet("/path/to/custom-styles.css"); // Optional: add custom CSS styles
                c.EnableFilter();
                c.ShowExtensions();
                c.EnableValidator();
            });
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            var serviceProvider = new ServiceCollection()
            .AddDbContext<AutoDataContext>(options => options.UseSqlite(configurationRoot.GetConnectionString("DefaultConnection")))
            .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AutoDataContext>();

                // Check if the admin user already exists
                if (!dbContext.Users.Any(u => u.Login == "Admin"))
                {
                    // Create the admin user
                    var adminUser = new User
                    {
                        Id = Guid.NewGuid(),
                        Login = "Admin",
                        Password = "Admin",
                        Name = "admin",
                        Admin = true,
                        ModifiedBy = "",
                        ModifiedOn = DateTime.UtcNow,
                        CreatedBy = "Admin",
                        CreatedOn = DateTime.UtcNow,
                        RevokedBy = "",
                        RevokedOn = null,
                        Birthday = null,
                        Gender = 2,
                        Token = null,

                    };

                    dbContext.Users.Add(adminUser);
                    dbContext.SaveChanges();
                }
            }
        }
    }
}
