using MediatR;

namespace BuildingBlocks.CQRS;

public interface IqueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : notnull
{
}

