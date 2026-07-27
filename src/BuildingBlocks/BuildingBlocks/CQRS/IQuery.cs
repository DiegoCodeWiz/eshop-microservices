using MediatR;

namespace BuildingBlocks.CQRS;

//esta interface devuelve un resultado de consulta not null
public interface IQuery<out TResponse> : IRequest<TResponse>
    where TResponse : notnull
{
}
