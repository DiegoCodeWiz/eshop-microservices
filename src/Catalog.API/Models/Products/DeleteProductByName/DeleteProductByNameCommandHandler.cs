using Catalog.API.Exceptions;
using FluentValidation;

namespace Catalog.API.Models.Products.DeleteProductByName
{
    public record DeleteProductByNameCommand(string Name) : ICommand<DeleteProductByNameResult>;
    public record DeleteProductByNameResult(bool Success);

    // Mismo patrón que DeleteBasketCommandvalidator (Basket.API)
    public class DeleteProductByNameCommandValidator : AbstractValidator<DeleteProductByNameCommand>
    {
        public DeleteProductByNameCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre del producto es requerido.");
        }
    }

    internal class DeleteProductByNameCommandHandler(IDocumentSession documentSession, ILogger<DeleteProductByNameCommandHandler> logger)
        : ICommandHandler<DeleteProductByNameCommand, DeleteProductByNameResult>
    {
        public async Task<DeleteProductByNameResult> Handle(DeleteProductByNameCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("DeleteProductByNameCommandHandler.Handle llamado con {@Request}", request);

            var product = await documentSession.Query<Product>()
                .FirstOrDefaultAsync(p => p.Name.ToLower() == request.Name.ToLower(), cancellationToken);

            if (product is null)
                throw new ProductNotFoundException(request.Name);

            documentSession.Delete(product);
            await documentSession.SaveChangesAsync(cancellationToken);

            return new DeleteProductByNameResult(true);
        }
    }
}
