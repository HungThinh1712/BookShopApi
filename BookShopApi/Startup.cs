using BookShopApi.DatabaseSettings;
using BookShopApi.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.FileProviders;
using System.IO;
using BookShopApi.Hubs;

namespace BookShopApi
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOriginsPolicy",
                builder =>
                {
                    builder.SetIsOriginAllowed(origin => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });


            services.Configure<BookShopDatabaseSettings>(
                     Configuration.GetSection(nameof(BookShopDatabaseSettings)));
            services.AddSingleton<IBookShopDatabaseSettings>(provider =>
                provider.GetRequiredService<IOptions<BookShopDatabaseSettings>>().Value);


        
            services.AddSingleton<BookService>();
            services.AddSingleton<TypeService>();
            services.AddSingleton<CommentService>();
            services.AddSingleton<PublishingHouseService>();
            services.AddSingleton<AuthorService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<ShoppingCartService>();
            services.AddSingleton<ProvinceService>();
            services.AddSingleton<OrderService>();
            services.AddSingleton<DistrictService>();
            services.AddSingleton<WardService>();
            services.AddSingleton<NotificationService>();
            services.AddAuthentication();
            services.AddMvc().AddSessionStateTempDataProvider();
            services.AddSession();

            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

            services.AddControllers().AddNewtonsoftJson().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>()); ;

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BookShop API", Version = "v1" });
            });
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookShop V1");
            });
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("AllowAllOriginsPolicy");
            app.UseAuthorization();
            app.UseSession();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "Images")),
                RequestPath = "/Images"
            });
          
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("api/hubs/notification");
            });
        }
    }
}
