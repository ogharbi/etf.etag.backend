
using Asp.Versioning;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Identity.Web;
using System.IO.Compression;
using System.Net;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using  VC.AG.DAO.UnitOfWork;
using  VC.AG.Models;
using  VC.AG.Models.Helpers;
using  VC.AG.ServiceLayer.Contracts;
using  VC.AG.ServiceLayer.Services;
using Wkhtmltopdf.NetCore;
namespace VC.AG.WebAPI.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddRequirements(this IServiceCollection services, WebApplicationBuilder builder, IConfiguration config)
        {
            IWebHostEnvironment env = builder.Environment;
            var keyVaultUri = config.GetValue<string>(AppConstants.AppSettingsKeys.keyVault_Uri);
            if (!string.IsNullOrEmpty(keyVaultUri))
            {
                builder.Configuration.AddAzureKeyVault(
                    new Uri($"{keyVaultUri}"),
                    new DefaultAzureCredential());
            }
            var aad = config.GetSection("AzureAd");
            aad.GetSection("Instance").Value = config.GetValue<string>(AppConstants.AppSettingsKeys.Instance);
            aad.GetSection("Domain").Value = config.GetValue<string>(AppConstants.AppSettingsKeys.Domain);
            aad.GetSection("TenantId").Value = config.GetValue<string>(AppConstants.AppSettingsKeys.TenantId);
            aad.GetSection("ClientId").Value = config.GetValue<string>(AppConstants.AppSettingsKeys.ClientId);
            aad.GetSection("ClientSecret").Value = config.GetValue<string>(AppConstants.AppSettingsKeys.ClientSecret);
            aad.GetSection("Audiance").Value = config.GetValue<string>(AppConstants.AppSettingsKeys.Authority);
            aad.GetSection("Resource").Value = config.GetValue<string>(AppConstants.AppSettingsKeys.Resource);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(aad);
            services.AddAuthorization();
            services.AddControllers().AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.PropertyNamingPolicy = null;
                opts.JsonSerializerOptions.MaxDepth = 64;
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                opts.JsonSerializerOptions.Converters.Add(new DoubleInfinityConverter());
                opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                opts.JsonSerializerOptions.WriteIndented = true;

            });
            services.Configure<FormOptions>(options =>
            {
                options.ValueCountLimit = int.MaxValue;
            });

            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.ReportApiVersions = true;

            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            services.AddMvc(options =>
            {
                options.MaxModelBindingCollectionSize = int.MaxValue;
            });
            var appUrl = $"{config.GetValue<string>(AppConstants.AppSettingsKeys.AppUrl)}";
            services.AddCors(op =>
            {
                op.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(appUrl).AllowAnyHeader().AllowAnyMethod();
                });
            });  
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.SmallestSize;
            });

            if (env.IsDevelopment())
            {
                services.AddSwaggerGen();
            }
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserContract, UserService>();
            services.AddScoped<ISiteContract, SiteService>();
            services.AddScoped<IAppContract, AppService>();
            services.AddScoped<IFormContract, FormService>();
            services.AddScoped<INotifContract, NotifService>();
            services.AddScoped<IFileService, FileService>();
            services.AddWkhtmltopdf();
        }
    }
}
