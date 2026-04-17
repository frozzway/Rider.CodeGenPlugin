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
    public static readonly ServiceCollection ServiceCollection = [];
    public static IServiceProvider ServiceProvider = null!;
    private static bool _initialized;

    [rt::System.Runtime.CompilerServices.ModuleInitializer]
    public static void Initialize()
    {
        if (_initialized)
            return;

        Initialize(ServiceCollection);
        ServiceCollection.AddScoped<SolutionTypeElementsAccessor>();
        ServiceCollection.AddScoped<IContextAccessor, ContextAccessor>();

        ServiceProvider = ServiceCollection.BuildServiceProvider();

        _initialized = true;
    }

    public static void Initialize(IServiceCollection services)
    {
        var assembly = Assembly.GetAssembly(typeof(DependencyInjection));

        RegisterOpenGenericAsImplementedInterfaces(services, assembly, typeof(IExecutor<>));
        RegisterOpenGenericAsImplementedInterfaces(services, assembly, typeof(IDialogForm<>));
        RegisterSubClassesAsSelf(services, assembly, typeof(MapperGenerationService));
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

    private static void RegisterSubClassesAsSelf(
        IServiceCollection services,
        Assembly assembly,
        Type baseType)
    {
        var concreteTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(baseType));

        foreach (var implType in concreteTypes)
            services.AddScoped(implType);
    }
}
