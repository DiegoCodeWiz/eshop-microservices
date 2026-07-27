using Basket.API.Data;
using Basket.API.Models;
using FluentValidation;

namespace Basket.API.Basket.StoreBasket
{
    public record StoreBasketCommand(ShoppingCart Cart) : ICommand<StoreBasketResult>;
    public record StoreBasketResult(string UserName);
    public class StoreBasketCommandvalidator : AbstractValidator<StoreBasketCommand>
    {
        public StoreBasketCommandvalidator()
        {
            RuleFor(x => x.Cart).NotNull().WithMessage("El Carrito no puede ser nulo.");
            RuleFor(x => x.Cart.UserName).NotEmpty().WithMessage("El nombre del usuario es requerido.");
        }
    }

    public class StoreBasketCommandHandler(IBasketRepository repository)
        : ICommandHandler<StoreBasketCommand, StoreBasketResult>
    {
        public async Task<StoreBasketResult> Handle(StoreBasketCommand command, CancellationToken cancellationToken)
        {
            ShoppingCart cart = command.Cart;
            await repository.StoreBasket(cart, cancellationToken);
            return new StoreBasketResult(cart.UserName);
        }
    }
}
