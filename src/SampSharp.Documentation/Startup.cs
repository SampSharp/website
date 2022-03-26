// SampSharp.Documentation
// Copyright 2019 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using SampSharp.Documentation.Configuration;
using SampSharp.Documentation.NewModels;
using SampSharp.Documentation.Repositories;
using SampSharp.Documentation.Services;

namespace SampSharp.Documentation
{
	public class Startup
	{
		private readonly IWebHostEnvironment _webHostEnvironment;

		public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
		{
			_webHostEnvironment = webHostEnvironment;
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			IMvcBuilder builder = services.AddRazorPages();

#if DEBUG
			if (_webHostEnvironment.IsDevelopment())
			{
				builder.AddRazorRuntimeCompilation();
			}
#endif

			// TODO: Move connection string to config
			services.AddDbContextPool<DocsDbContext>(options => options
				.UseMySql("foobar", mySqlOptions => mySqlOptions
					.ServerVersion(new ServerVersion(new Version(10, 4, 8), ServerType.MariaDb))
				));

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

			services.Configure<RepositoryOptions>(Configuration.GetSection("Repository"));
			services.Configure<StorageOptions>(Configuration.GetSection("Storage"));
			services.Configure<ImportOptions>(Configuration.GetSection("Import"));

			services.AddTransient<IDocsVersionBuilder, DocsVersionBuilder>();
			services.AddTransient<IDocumentationDataRepository, DocumentationDataRepository>();
			services.AddTransient<IViewRenderService, ViewRenderService>();
			services.AddTransient<IGithubDataRepository, GithubDataRepository>();
			
			//services.AddTransient<IDocsImportService, DocsImportService>();
			services.AddTransient<ApiImportService>();
			services.AddTransient<NewDocsImportService>();

			services.AddTransient<IStorageService, StorageService>();
			services.AddTransient<IFileSystemService, FileSystemService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, NewDocsImportService docsImportService)
		{
			try
			{
				docsImportService.ImportAllBranches().Wait();
				//apiImportService.ImportAllFromNuGet().Wait();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			else
				app.UseExceptionHandler("/__internal/error/server_error");

			app.UseHsts();

			app.Use(async (ctx, next) =>
			{
				await next();

				if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
				{
					//Re-execute the request so the user gets the error page
					string originalPath = ctx.Request.Path.Value;
					ctx.Items["originalPath"] = originalPath;
					ctx.Request.Path = "/__internal/error/not_found";
					await next();
				}
			});

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute("home2", "/index2", new
				{
					controller = "Home",
					action = "Index2"
				});

				endpoints.MapControllerRoute("home", "/", new
				{
					controller = "Home",
					action = "Index"
				});
				
				endpoints.MapControllerRoute(
					"server_error",
					"/__internal/error/server_error",
					new
					{
						controller = "Error",
						action = "ServerError"
					});

				endpoints.MapControllerRoute(
					"not_found",
					"/__internal/error/not_found",
					new
					{
						controller = "Error",
						action = "PageNotFound"
					});

				endpoints.MapControllerRoute(
					"webhook_manual_all_branches",
					"/webhook/all",
					new
					{
						controller = "WebHook",
						action = "ImportAllBranches"
					});

				endpoints.MapControllerRoute(
					"webhook_github",
					"/webhook/github",
					new
					{
						controller = "WebHook",
						action = "GitHub"
					});
				
				endpoints.MapControllerRoute(
					"docs",
					"docs/{version?}/{*page}",
					new
					{
						controller = "Documentation",
						action = "Index"
					});

				endpoints.MapControllerRoute(
					"api",
					"api/{assembly}/{version}",
					new
					{
						controller = "Api",
						action = "Assembly"
					});

				endpoints.MapControllerRoute(
					"api",
					"api/{assembly}/{version}/{type}",
					new
					{
						controller = "Api",
						action = "Type"
					});

			});
		}
	}
}
