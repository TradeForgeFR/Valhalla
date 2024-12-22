using System.Reflection;
using NetMQ;
using NetMQ.Sockets;
using Valhalla.Core.PluginBase;

namespace Valhalla.Core;

public class Worker : BackgroundService
{
    private readonly ILogger _logger;
    private const string XPublisherConnectionString = "tcp://127.0.0.1:1234";
    private const string XSubscriberConnectionString = "tcp://127.0.0.1:5678";


    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Thread proxy = new Thread(RunProxy);
        proxy.IsBackground = true;
        proxy.Start();

        // ManualResetEvent resetEvent = new ManualResetEvent(false);
        // List<Thread> threads = new List<Thread>();
        //
        // threads.Add(proxy);
        // proxy.Start();

        var pluginsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                          @"/Valhalla/Core/Plugins";
        
        if(!Directory.Exists(pluginsPath))
        {
            Directory.CreateDirectory(pluginsPath);
        }
        
        var pluginPaths = Directory.GetFiles(pluginsPath, "*.dll")
            .ToList();
        
        IEnumerable<IProvider> commands = pluginPaths.SelectMany(pluginPath =>
        {
            Assembly pluginAssembly = LoadPlugin(pluginPath);
            return CreateCommands(pluginAssembly);
        }).ToList();
        
        foreach (var command in commands)
        {
            command.Execute(XSubscriberConnectionString, XPublisherConnectionString);
        }
        
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    /// <summary>
    /// Run XPub-XSub Proxy
    /// </summary>
    private static void RunProxy()
    {
        using (var xpubSocket = new XPublisherSocket(XPublisherConnectionString))
        using (var xsubSocket = new XSubscriberSocket(XSubscriberConnectionString))
        {
            Console.WriteLine("Intermediary started, and waiting for messages");
            // proxy messages between frontend / backend
            var proxy = new Proxy(xsubSocket, xpubSocket);
            // blocks indefinitely
            proxy.Start();
        }
    }

    /// <summary>
    /// Load Plugins
    /// </summary>
    static Assembly LoadPlugin(string relativePath)
    {
        // Navigate up to the solution root
        string root = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(typeof(Program).Assembly.Location)))))));

        string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
        Console.WriteLine($"Loading commands from: {pluginLocation}");
        PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
        return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
    }

    static IEnumerable<IProvider> CreateCommands(Assembly assembly)
    {
        int count = 0;

        foreach (Type type in assembly.GetTypes())
        {
            var typeOf = typeof(IProvider);
            var isIProvider = typeOf.IsAssignableFrom(type);
            if (typeof(IProvider).IsAssignableFrom(type))
            {
                IProvider result = Activator.CreateInstance(type) as IProvider;
                if (result != null)
                {
                    count++;
                    yield return result;
                }
            }
        }

        // if (count == 0)
        // {
        //     string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
        //     throw new ApplicationException(
        //         $"Can't find any type which implements IProvider in {assembly} from {assembly.Location}.\n" +
        //         $"Available types: {availableTypes}");
        // }
    }
}