using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Attendance_Performance_Control.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Attendance_Performance_Control.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace Attendance_Performance_Control
{
    public class Startup
    {
        //Declare secret variables from "appsettings.json"
        private string _adminEmail = null;
        private string _adminPass = null;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //call secret variables from "secret.json"
            _adminEmail = Configuration["Attendence-Control:AdminEmail"];
            _adminPass = Configuration["Attendence-Control:AdminPass"];

            // Add default portuguese culture
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("pt", "pt");
                //By default the below will be set HTTP Request Header to whatever the server culture is.
                // Force to have default culture everywhere
                options.SupportedCultures = new List<CultureInfo> {new CultureInfo("pt")};
                // Remove all Culture Providers from pipeline, to cleaner and lighter code,
                // we don't need them in current project
                options.RequestCultureProviders = new List<IRequestCultureProvider>();
            });

            services.AddMvc();

            //services.AddMvc(options =>
            //{
            //    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(x => $"O valor '{x}' n�o � v�lido.");
            //    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(x =>
            //        $"O valor {x} deve ser o n�mero.");
            //    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(x =>
            //        $"O valor para '{x}' deve ser fornecido");
            //    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((x, y) =>
            //        $"O valor '{x}' n�o � v�lido para {y}.");
            //    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => "O valor � obrigat�rio.");
            //    options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(x =>
            //        $"O valor fornecido n�o � v�lido para {x}.");
            //    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(x => $"O valor '{x}' n�o � v�lido.");
            //    options.ModelBindingMessageProvider.SetMissingRequestBodyRequiredValueAccessor(() =>
            //        "O valor n�o pode ser nulo");
            //    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(x =>
            //        $"O valor '{x}' n�o � v�lido.");
            //    options.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(() =>
            //        "O valor fornecido n�o � v�lido.");
            //    options.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(() =>
            //        "O valor deve ser o n�mero.");
            //});


            //Application DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));


            services.AddDefaultIdentity<ApplicationUser>(options =>
                    options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddErrorDescriber<CustomIdentityError>();

            //services for SendGrid email sender
            services.AddTransient<IEmailSender, EmailSender>();
            //configure a class for secret keys:  SendGrid API and UserName
            services.Configure<AuthMessageSenderOptions>(Configuration);

            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
            //    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            //    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            //});

            services.AddControllersWithViews();

            services.AddRazorPages();


            // Add default options Identity configuration
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(24);
                options.Lockout.MaxFailedAccessAttempts = 7;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });

            //Session expiry for MVC is provided via cookie
            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                //A flag that says the cookie is only available to servers.
                //The browser only sends the cookie but cannot access it through JavaScript.
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(24);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            //Add this service for automatic redirect to login page and after to Index
            services.AddAuthorization(options =>
            {
                // This says, that all pages need AUTHORIZATION. But when a controller, 
                // for example the login controller in Login.cshtml.cs, is tagged with
                // [AllowAnonymous] then it is not in need of AUTHORIZATION.
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Call to set default culture - PT
            //app.UseRequestLocalization();
            app.UseRequestLocalization(new RequestLocalizationOptions()
            {
            DefaultRequestCulture = new RequestCulture("pt", "pt"),
            //By default the below will be set HTTP Request Header to whatever the server culture is.
            // Force to have default culture everywhere
            SupportedCultures = new List<CultureInfo> { new CultureInfo("pt") },
            // Remove all Culture Providers from pipeline, to cleaner and lighter code,
            // we don't need them in current project
            RequestCultureProviders = new List<IRequestCultureProvider>()
        });

            app.UseHttpsRedirection();

            //for using files .css .js in wwwroot folder
            app.UseStaticFiles(); 

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            // Function that create user and add it to Admin Role
            CreateRoles(serviceProvider);
        }

        private void CreateRoles(IServiceProvider serviceProvider)
        {

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            //use secret variables from "secret.json"
            string email = _adminEmail;

            //Check if Administrator role exist
            var isAdminRoleExist = roleManager.RoleExistsAsync("Admin").Result;

            //if Admin role do not exist
            if (!isAdminRoleExist)
            {
                //Create Admin Role
                Task<IdentityResult>  roleResult = roleManager.CreateAsync(new IdentityRole("Admin"));
                roleResult.Wait();

                //Create User
                ApplicationUser administrator = new ApplicationUser
                {
                    FirstName = "�ngelo",
                    LastName = "Marum",
                    Email = email,
                    OccupationId = 6, // Occupation-Cargo - Diretor Geral (Hidden for other users)
                    UserName = email,
                    EmailConfirmed = true
                };

                Task<IdentityResult> newUser = userManager.CreateAsync(administrator, _adminPass);
                newUser.Wait();
                

                //Add User to Admin role
                if (newUser.Result.Succeeded)
                {
                    Task<IdentityResult> newUserRole = userManager.AddToRoleAsync(administrator, "Admin");
                    newUserRole.Wait();
                }
            }
        }
    }
}
