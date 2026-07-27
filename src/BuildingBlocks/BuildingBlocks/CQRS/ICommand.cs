using MediatR;

namespace BuildingBlocks.CQRS;
public interface ICommand : ICommand<Unit>
{

}
//Retorna una respuesta Generica con peticion generica
public interface ICommand<out TResponse> : IRequest<TResponse>
{

}

