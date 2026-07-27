using Catalog.API.Exceptions;
using Catalog.API.Models.Products;

namespace Catalog.API.Products.UpdateProducts
{
    public record UpdateProductCommand(Guid Id, string Name, List<string> Category,
        string Description, string ImageFile, decimal Price) :
        ICommand<UpdateProductResult>;

    public record UpdateProductResult(bool IsSuccess);

    /// <summary>
    /// Esta clase se encargara de establecer el orden de actualizacion del producto.
    /// </summary>
    internal class UpdateProductCommandHandler(IDocumentSession session,
        ILogger<UpdateProductCommandHandler> logger)
        : ICommandHandler<UpdateProductCommand, UpdateProductResult>
    {
        public async Task<UpdateProductResult> Handle(UpdateProductCommand command,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("UpdateProductHandler.Handle llamado con {@Command}", command);
            var product = await session.LoadAsync<Product>(command.Id, cancellationToken);
            if (product is null)
            {
                throw new ProductNotFoundException();
            }

            product.Name = command.Name;
            product.Category = command.Category;
            product.Description = command.Description;
            product.ImageFile = command.ImageFile;
            product.Price = command.Price;

            session.Update(product);
            await session.SaveChangesAsync(cancellationToken);

            return new UpdateProductResult(true);
        }
    }
}