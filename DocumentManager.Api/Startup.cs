using DocumentManager.Api.Helpers;
using DocumentManager.Api.Options;
using DocumentManager.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Swashbuckle.AspNetCore.Swagger;
using System.Threading.Tasks;

namespace DocumentManager.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            RegisterTypes(services, Configuration);

            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new Info()));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Manager"));

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private void RegisterTypes(IServiceCollection services, IConfiguration configuration)
        {
            var dbOptions = configuration.GetSection("CosmosDb").Get<DbOptions>();
            services.AddSingleton<IDbService>(InitializeCosmosDbServiceInstanceAsync(dbOptions).GetAwaiter().GetResult());

            var azureBlobOptions = configuration.GetSection("AzureBlobStorage").Get<AzureBlobOptions>();
            services.AddSingleton<IContentService>(InitializeAzureBlobServiceInstanceAsync(azureBlobOptions).GetAwaiter().GetResult());

            services.AddSingleton<IFileValidator, PdfFileValidator>();
        }

        private static async Task<CosmosDbService> InitializeCosmosDbServiceInstanceAsync(DbOptions dbOptions)
        {
            var clientBuilder = new Microsoft.Azure.Cosmos.Fluent.CosmosClientBuilder(dbOptions.Endpoint, dbOptions.Key);
            var client = clientBuilder.WithConnectionModeDirect().Build();
            var database = await client.CreateDatabaseIfNotExistsAsync(dbOptions.Database);
            var response = await database.Database.CreateContainerIfNotExistsAsync(dbOptions.Container, "/id");

            return new CosmosDbService(response.Container);
        }

        private static async Task<AzureBlobContentService> InitializeAzureBlobServiceInstanceAsync(AzureBlobOptions options)
        {
            var credentials = new StorageCredentials(options.Name, options.Key);
            var storageAccount = new CloudStorageAccount(credentials, true);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(options.Container);
            await container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Blob, new BlobRequestOptions(), new OperationContext());

            return new AzureBlobContentService(container);
        }
    }
}
