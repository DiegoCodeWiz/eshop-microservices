using Basket.API.Basket.StoreBasket;
using Basket.API.Data;
using Basket.API.Models;
using FluentValidation;

namespace Basket.API.Basket.DeleteBasket
{
    public record DeleteBasketCommand(String UserName) : ICommand<DeleteBasketResult>;
    public record DeleteBasketResult(bool IsSuccess);
    public class DeleteBasketCommandvalidator : AbstractValidator<DeleteBasketCommand>
    {
        public DeleteBasketCommandvalidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("El nombre de usuario es requerido.");
        }
    }

    public class DeleteBasketCommandHandler(IBasketRepository repository)
        : ICommandHandler<DeleteBasketCommand, DeleteBasketResult>
    {
        public async Task<DeleteBasketResult> Handle(DeleteBasketCommand command, CancellationToken cancellationToken)
        {
            await repository.DeleteBasket(command.UserName, cancellationToken);
            return new DeleteBasketResult(true);
        }
    }
}