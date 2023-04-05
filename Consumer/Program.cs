using Consumer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DbContext = Shared.DbContext;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((builderContext, serviceCollection) =>
{
    var dbConnectionString = builderContext.Configuration.GetConnectionString("Database");
    serviceCollection.AddDbContext<DbContext>(optionsBuilder => optionsBuilder.UseSqlServer(dbConnectionString));
    
    // Setup MassTransit using GRPC and transactional outbox
    serviceCollection.AddMassTransit(registrationConfigurator =>
    {
        registrationConfigurator.AddConsumer<TestMessageConsumer>();

        registrationConfigurator.AddEntityFrameworkOutbox<DbContext>(outboxConfigurator =>
        {
            outboxConfigurator.UseSqlServer();
            outboxConfigurator.UseBusOutbox();
        });

        registrationConfigurator.UsingGrpc((registrationContext, factoryConfigurator) =>
        {
            factoryConfigurator.Host(hostConfigurator =>
            {
                hostConfigurator.Host = "localhost";
                hostConfigurator.Port = 12346;
                
                hostConfigurator.AddServer(new Uri("http://localhost:12345"));
            });
            
            factoryConfigurator.ConfigureEndpoints(registrationContext);
        });
    });
});

var app = builder.Build();

// Create the database if necessary
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
dbContext.Database.EnsureCreated();

app.Run();
