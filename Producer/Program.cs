using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Producer;
using DbContext = Shared.DbContext;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((builderContext, serviceCollection) =>
{
    var dbConnectionString = builderContext.Configuration.GetConnectionString("Database");
    serviceCollection.AddDbContext<DbContext>(optionsBuilder => optionsBuilder.UseSqlServer(dbConnectionString));
    
    // Setup MassTransit using GRPC and transactional outbox
    serviceCollection.AddMassTransit(registrationConfigurator =>
    {
        registrationConfigurator.AddBusObserver<BusObserver>();

        registrationConfigurator.AddEntityFrameworkOutbox<DbContext>(outboxConfigurator =>
        {
            outboxConfigurator.UseSqlServer();
            outboxConfigurator.UseBusOutbox();
        });

        registrationConfigurator.UsingGrpc((_, factoryConfigurator) =>
        {
            factoryConfigurator.Host(hostConfigurator =>
            {
                hostConfigurator.Host = "localhost";
                hostConfigurator.Port = 12345;
            });
        });
    });
});

var app = builder.Build();

// Create the database if necessary
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
dbContext.Database.EnsureCreated();

app.Run();
