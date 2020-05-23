using AutoMapper;
using DocumentManager.Api.Middleware;
using DocumentManager.Persistence.Configuration;
using DocumentManager.Persistence.Repositories;
using DocumentManager.Persistence.Storage;
using DocumentManager.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Fluent;
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
            {
                app.UseDeveloperExceptionPage();

                CreateCosmosDbContainerIfNotExistsAsync().GetAwaiter().GetResult();
                CreateAzureBlobContainerIfNotExistsAsync().GetAwaiter().GetResult();
            }
            else
                app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Manager"));

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private void RegisterTypes(IServiceCollection services)
        {
            services.AddSingleton<IDocumentsRepository, DocumentsRepository>();
            services.AddSingleton<IContentStorage, AzureBlobContentStorage>();
            services.AddScoped<IDocumentsService, DocumentsService>();
        }

        private async Task CreateCosmosDbContainerIfNotExistsAsync()
        {
            var dbSettings = _configuration.GetSection("CosmosDb").Get<DbSettings>();

            var clientBuilder = new CosmosClientBuilder(dbSettings.Endpoint, dbSettings.Key);
            using(var client = clientBuilder.WithConnectionModeDirect().Build())
            {
                var database = await client.CreateDatabaseIfNotExistsAsync(dbSettings.Database);
                await database.Database.CreateContainerIfNotExistsAsync(dbSettings.Container, "/partition");
            }
        }

        private async Task CreateAzureBlobContainerIfNotExistsAsync()
        {
            var azureBlobSettings = _configuration.GetSection("AzureBlobStorage").Get<StorageSettings>();
            var credentials = new StorageCredentials(azureBlobSettings.Name, azureBlobSettings.Key);
            var storageAccount = new CloudStorageAccount(credentials, true);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(azureBlobSettings.Container);
            await container.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Blob, new BlobRequestOptions(), new OperationContext());
        }
    }
}
