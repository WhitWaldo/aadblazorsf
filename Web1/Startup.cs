using System.Fabric;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace Web1
{
	public class Startup
	{
		private readonly StatelessServiceContext _context;

		public Startup(StatelessServiceContext context)
		{
			_context = context;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
				.AddMicrosoftIdentityWebApp(opt =>
				{
					opt.Domain = _context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings
						.Sections["AzureAD"].Parameters["Domain"].Value;
					opt.TenantId = _context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings
						.Sections["AzureAD"].Parameters["TenantId"].Value;
					opt.Instance = _context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings
						.Sections["AzureAD"].Parameters["Instance"].Value;
					opt.ClientId = _context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings
						.Sections["AzureAD"].Parameters["ClientId"].Value;;
					opt.CallbackPath = _context.CodePackageActivationContext.GetConfigurationPackageObject("Config").Settings
						.Sections["AzureAD"].Parameters["CallbackPath"].Value;;
				});
			services.AddControllersWithViews()
				.AddMicrosoftIdentityUI();

			services.AddAuthorization(options =>
			{
				// By default, all incoming requests will be authorized according to the default policy
				options.FallbackPolicy = options.DefaultPolicy;
			});

			services.AddRazorPages();
			services.AddServerSideBlazor()
				.AddMicrosoftIdentityConsentHandler();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.Use((context, next) =>
			{
				context.Request.Host = new HostString("localhost", 9013);
				return next();
			});

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseCookiePolicy(new CookiePolicyOptions
			{
				MinimumSameSitePolicy = SameSiteMode.Lax
			});

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});
		}
	}
}
