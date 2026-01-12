extern alias rt;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rider.Plugins.CodeGenJetbrains.Execution.Abstract;
using Rider.Plugins.CodeGenJetbrains.Execution.Common;

namespace Rider.Plugins.CodeGenJetbrains;

public static class DependencyInjection
{
    public static ServiceCollection ServiceCollection = [];
    public static IServiceProvider ServiceProvider;
    private static bool Initialized;

    [rt::System.Runtime.CompilerServices.ModuleInitializer]
    public static void Initialize()
    {
        if (Initialized)
            return;

        Initialize(ServiceCollection);
        ServiceCollection.AddScoped<SolutionTypeElementsAccessor>();

        ServiceProvider = ServiceCollection.BuildServiceProvider();

        Initialized = true;
    }

    public static void Initialize(IServiceCollection services)
    {
        var assembly = Assembly.GetAssembly(typeof(DependencyInjection));

        RegisterOpenGenericAsImplementedInterfaces(services, assembly, typeof(IExecutor<>));
        RegisterOpenGenericAsImplementedInterfaces(services, assembly, typeof(IDialogForm<>));
    }

    private static void RegisterOpenGenericAsImplementedInterfaces(
        IServiceCollection services,
        Assembly assembly,
        Type openGenericInterface)
    {
        var concreteTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false });

        foreach (var implType in concreteTypes)
        {
            var matchingInterfaces = implType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface);

            foreach (var serviceType in matchingInterfaces)
                services.AddScoped(serviceType, implType);
        }
    }
}
