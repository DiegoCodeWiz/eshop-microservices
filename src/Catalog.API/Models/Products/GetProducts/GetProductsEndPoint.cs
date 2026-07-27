using Catalog.API.Common.Pagination;

namespace Catalog.API.Models.Products.GetProducts
{
    public class GetProductsEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products", async (ISender sender, string? name, int pageIndex = 1, int pageSize = 10) =>
            {
                var result = await sender.Send(new GetProductsQuery(name, pageIndex, pageSize));
                return Results.Ok(result.Products);
            })
                .WithName("GetProducts")
                .Produces<PaginatedResult<Product>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Consultar productos (busqueda por nombre + paginado)")
                .WithDescription("Devuelve productos filtrados opcionalmente por nombre (?name=), con paginado (?pageIndex=&pageSize=).");
        }
    }
}
