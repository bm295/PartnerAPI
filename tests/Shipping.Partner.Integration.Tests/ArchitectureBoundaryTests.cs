using Shipping.Partner.Integration.Api.DependencyInjection;
using Shipping.Partner.Integration.Application.Abstractions;
using Xunit;

namespace Shipping.Partner.Integration.Tests;

public class ArchitectureBoundaryTests
{
    [Fact]
    public void PersistencePorts_ShouldNotExposeHttpRequestContracts()
    {
        Type[] persistencePorts =
        [
            typeof(IShippingPartnerRepository),
            typeof(IShippingOrderRepository),
            typeof(IShipmentEventStore)
        ];

        var exposedTypes = persistencePorts
            .SelectMany(port => port.GetMethods())
            .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType)
                .Append(method.ReturnType))
            .SelectMany(Flatten);

        Assert.DoesNotContain(exposedTypes, type =>
            type.Namespace == "Shipping.Partner.Integration.Api.Contracts");
    }

    [Fact]
    public void CompositionRoot_ShouldBelongToApiBoundary()
    {
        Assert.Equal(
            "Shipping.Partner.Integration.Api.DependencyInjection",
            typeof(ShippingPartnerIntegrationServiceCollectionExtensions).Namespace);
    }

    private static IEnumerable<Type> Flatten(Type type)
    {
        yield return type;

        foreach (var argument in type.GetGenericArguments())
        {
            foreach (var nestedType in Flatten(argument))
            {
                yield return nestedType;
            }
        }
    }
}
