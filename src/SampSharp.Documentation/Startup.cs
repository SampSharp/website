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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampSharp.Documentation.Configuration;
using SampSharp.Documentation.Repositories;

namespace SampSharp.Documentation
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
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.Configure<RepositoryOptions>(Configuration.GetSection("Repository"));
			services.Configure<StorageOptions>(Configuration.GetSection("Storage"));

			services.AddTransient<IDataImportService, DataImportService>();
			services.AddTransient<IVersionBuilder, VersionBuilder>();
			services.AddTransient<DataImportService>();
			services.AddTransient<IDataRepository, DataRepository>();
			services.AddTransient<IGithubDataRepository, GithubDataRepository>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			else
			{
				app.UseExceptionHandler("/__internal/error/server_error");

				app.UseHsts();
			}

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

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					"home",
					"/",
					new
					{
						controller = "Home",
						action = "Index"
					});

				routes.MapRoute(
					"server_error",
					"/__internal/error/server_error",
					new
					{
						controller = "Error",
						action = "ServerError"
					});

				routes.MapRoute(
					"not_found",
					"/__internal/error/not_found",
					new
					{
						controller = "Error",
						action = "PageNotFound"
					});

				routes.MapRoute(
					"webhook_manual_all_branches",
					"/webhook/all",
					new
					{
						controller = "WebHook",
						action = "ImportAllBranches"
					});

				routes.MapRoute(
					"webhook_github",
					"/webhook/github",
					new
					{
						controller = "WebHook",
						action = "GitHub"
					});

				routes.MapRoute(
					"documentation",
					"{versionOrPage?}/{*page}",
					new
					{
						controller = "Documentation",
						action = "Index"
					});
			});
		}
	}
}