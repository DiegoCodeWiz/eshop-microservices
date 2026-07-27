using Catalog.API.Exceptions;

namespace Catalog.API.Models.Products.GetProductById
{
    public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdResult>;

    public record GetProductByIdResult(Product Product);

    internal class GetProductByIdHandler(IDocumentSession session, ILogger<GetProductByIdHandler> logger)
        : IqueryHandler<GetProductByIdQuery, GetProductByIdResult>
    {
        public async Task<GetProductByIdResult> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
        {
            logger.LogInformation("GetProductByIdQueryHandler.Handle se llamo con {@Query}", query);
            var product = await session.LoadAsync<Product>(query.Id, cancellationToken);
            if (product is null)
            {
                throw new ProductNotFoundException();
            }
            return new GetProductByIdResult(product);
        }
    }
}