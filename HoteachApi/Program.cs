using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // Adding Application Insights
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Retrieve MongoDB connection string from environment variable
        var mongoConnectionString = Environment.GetEnvironmentVariable("MongoDBConnectionString");
        if (string.IsNullOrEmpty(mongoConnectionString))
        {
            throw new ArgumentNullException("MongoDBConnectionString", "MongoDB connection string is not set.");
        }

        // Add MongoDB client as singleton
        services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));

        // Add other services as necessary
        // Example: services.AddSingleton<YourCustomService>();

    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole(); // Adds console logging
        // Additional logging providers can be added here (e.g., Application Insights)
    })
    .Build();

host.Run();
