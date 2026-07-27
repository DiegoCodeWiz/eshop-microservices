namespace Catalog.API.Models.Products.GetProductByCategory
{
    record GetProductByCategoryQuery(string Category) : IQuery<GetProductByCategoryResult>;

    record GetProductByCategoryResult(IEnumerable<Product> Products);

    internal class GetProductByCategoryQueryHandler(IDocumentSession session, ILogger<GetProductByCategoryQueryHandler> logger) : IqueryHandler<GetProductByCategoryQuery, GetProductByCategoryResult>
    {
        public async Task<GetProductByCategoryResult> Handle(GetProductByCategoryQuery query, CancellationToken cancellationToken)
        {
            logger.LogInformation("GetProductByCategoryQueryHandler.Handle se llama al metodo {@Query}", query);
            var products = await session.Query<Product>()
                .Where(p => p.Category.Contains(query.Category))
                .ToListAsync();
            return new GetProductByCategoryResult(products);
        }
    }
}