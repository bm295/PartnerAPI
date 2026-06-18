namespace Shipping.Partner.Integration.Application.Cqrs;

public sealed record CommandResult<TResult>(TResult? Value, string? Error)
{
    public bool Succeeded => Error is null;

    public static CommandResult<TResult> Success(TResult value) => new(value, null);

    public static CommandResult<TResult> Failure(string error) => new(default, error);
}
