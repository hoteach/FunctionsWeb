using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
//using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var mongoConnectionString = Environment.GetEnvironmentVariable("MongoDBConnectionString");
        if (string.IsNullOrEmpty(mongoConnectionString))
        {
            throw new ArgumentNullException("MongoDBConnectionString", "MongoDB connection string is not set.");
        }

        services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));
    })
/*    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
    })*/
    .Build();

host.Run();
