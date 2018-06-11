﻿using System;
using Ibento.DevelopmentHost.Bus;
using Ibento.DevelopmentHost.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Ibento.DevelopmentHost
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection collection)
        {
            var mainBus = new InMemoryBus("MainBus", true, TimeSpan.FromMilliseconds(100));
            var authenticationService = new AuthenticatedRequestMessageProcessor(mainBus);
            var incomingHttpRequestAuthenticationManager = new IncomingHttpRequestAuthenticationManager(mainBus);
            var logbookEntryWriter = new LogbookEntryWriter(mainBus);
            mainBus.Subscribe(authenticationService);
            mainBus.Subscribe(incomingHttpRequestAuthenticationManager);
            mainBus.Subscribe(logbookEntryWriter);
            collection.AddSingleton<IPublisher>(mainBus);
            collection.AddSingleton<ISubscriber>(mainBus);
            collection.AddSingleton(authenticationService);
            collection.AddSingleton(incomingHttpRequestAuthenticationManager);
            collection.AddLogging();
            collection.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
