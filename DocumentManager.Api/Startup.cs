using AutoMapper;
using DocumentManager.Api.Middleware;
using DocumentManager.Api.Options;
using DocumentManager.Api.Settings;
using DocumentManager.Persistence.Repositories;
using DocumentManager.Persistence.Storage;
using DocumentManager.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Swashbuckle.AspNetCore.Swagger;
using System.Threading.Tasks;

namespace DocumentManager.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));
            RegisterTypes(services);

            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new Info()));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Manager"));

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private void RegisterTypes(IServiceCollection services)
        {
            services.AddSingleton<IDocumentsRepository>(serviceProvider => 
            {
                var dbOptions = _configuration.GetSection("CosmosDb").Get<DbSettings>();
                var repositoryLogger = serviceProvider.GetService<ILogger<DocumentsRepository>>();

                return InitializeCosmosDbServiceInstanceAsync(dbOptions, repositoryLogger).GetAwaiter().GetResult();
            });
            
            services.AddSingleton<IContentStorage>(serviceProvider =>
            {
                var azureBlobOptions = _configuration.GetSection("AzureBlobStorage").Get<StorageSettings>();
                var azureBlobStorageLogger = serviceProvider.GetService<ILogger<AzureBlobContentStorage>>();

                return InitializeAzureBlobServiceInstanceAsync(azureBlobOptions, azureBlobStorageLogger).GetAwaiter().GetResult();
            });

            services.AddScoped<IDocumentsService, DocumentsService>();
        }

        private static async Task<DocumentsRepository> InitializeCosmosDbServiceInstanceAsync(DbSettings dbSettings, ILogger<DocumentsRepository> logger)
        {
            var clientBuilder = new Microsoft.Azure.Cosmos.Fluent.CosmosClientBuilder(dbSettings.Endpoint, dbSettings.Key);
            var client = clientBuilder.WithConnectionModeDirect().Build();
            var database = await client.CreateDatabaseIfNotExistsAsync(dbSettings.Database);
            var response = await database.Database.CreateContainerIfNotExistsAsync(dbSettings.Container, "/partition");

            return new DocumentsRepository(response.Container, dbSettings.PartitionKey, logger);
        }

        private static async Task<AzureBlobContentStorage> InitializeAzureBlobServiceInstanceAsync(StorageSettings storageSettings, ILogger<AzureBlobContentStorage> logger)
        {
            var credentials = new StorageCredentials(storageSettings.Name, storageSettings.Key);
            var storageAccount = new CloudStorageAccount(credentials, true);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(storageSettings.Container);
            await container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Blob, new BlobRequestOptions(), new OperationContext());

            return new AzureBlobContentStorage(container, logger);
        }
    }
}
