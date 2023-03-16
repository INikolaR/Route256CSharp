using System.Runtime.Serialization;

namespace Route256.PriceCalculator.Infrastructure.Exceptions;

public class EntityNotFoundException : InfrastructureException
{
    public EntityNotFoundException()
    {
    }

    protected EntityNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public EntityNotFoundException(string? message) : base(message)
    {
    }

    public EntityNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}