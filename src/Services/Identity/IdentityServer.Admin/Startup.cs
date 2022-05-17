﻿// Copyright (c) Jan Škoruba. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using IdentityServer.Admin.Configuration.Database;
using IdentityServer.Admin.EntityFramework.Shared.DbContexts;
using IdentityServer.Admin.EntityFramework.Shared.Entities.Identity;
using IdentityServer.Admin.Helpers;
using IdentityServer.Shared.Dtos;
using IdentityServer.Shared.Dtos.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Skoruba.AuditLogging.EntityFramework.Entities;
using Skoruba.Duende.IdentityServer.Admin.UI.Helpers.ApplicationBuilder;
using Skoruba.Duende.IdentityServer.Admin.UI.Helpers.DependencyInjection;
using Skoruba.Duende.IdentityServer.Shared.Configuration.Helpers;
using System.IdentityModel.Tokens.Jwt;

namespace IdentityServer.Admin
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            HostingEnvironment = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Adds the Duende IdentityServer Admin UI with custom options.
            services.AddIdentityServerAdminUI<AdminIdentityDbContext, IdentityServerConfigurationDbContext, IdentityServerPersistedGrantDbContext,
            AdminLogDbContext, AdminAuditLogDbContext, AuditLog, IdentityServerDataProtectionDbContext,
                UserIdentity, UserIdentityRole, UserIdentityUserClaim, UserIdentityUserRole,
                UserIdentityUserLogin, UserIdentityRoleClaim, UserIdentityUserToken, string,
                IdentityUserDto, IdentityRoleDto, IdentityUsersDto, IdentityRolesDto, IdentityUserRolesDto,
                IdentityUserClaimsDto, IdentityUserProviderDto, IdentityUserProvidersDto, IdentityUserChangePasswordDto,
                IdentityRoleClaimsDto, IdentityUserClaimDto, IdentityRoleClaimDto>(ConfigureUIOptions);

            // Monitor changes in Admin UI views
            services.AddAdminUIRazorRuntimeCompilation(HostingEnvironment);

            // Add email senders which is currently setup for SendGrid and SMTP
            services.AddEmailSenders(Configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseRouting();

            app.UseIdentityServerAdminUI();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapIdentityServerAdminUI();
                endpoint.MapIdentityServerAdminUIHealthChecks();
            });
        }

        public virtual void ConfigureUIOptions(IdentityServerAdminUIOptions options)
        {
            // Applies configuration from appsettings.
            options.BindConfiguration(Configuration);
            if (HostingEnvironment.IsDevelopment())
            {
                options.Security.UseDeveloperExceptionPage = true;
            }
            else
            {
                options.Security.UseHsts = true;
            }

            // Set migration assembly for application of db migrations
            var migrationsAssembly = MigrationAssemblyConfiguration.GetMigrationAssemblyByProvider(options.DatabaseProvider);
            options.DatabaseMigrations.SetMigrationsAssemblies(migrationsAssembly);

            // Use production DbContexts and auth services.
            options.Testing.IsStaging = false;
        }
    }
}







