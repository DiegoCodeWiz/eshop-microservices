using Mapster;

namespace Catalog.API.Models.Products.GetProductById
{
    //public record GetProductByIdRequest();

    public record GetProductByIdResponse(Product Product);

    public class GetProductsByIdEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products/{id}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetProductByIdQuery(id));
                //mapeamos la respuesta de la consulta a get products por id
                var response = result.Adapt<GetProductByIdResponse>();
                return Results.Ok(response);
            })
            .WithName("GetProductById")
            .Produces<GetProductByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Obtenemos productos por id")
            .WithDescription("Obtenemos productos por id");
        }
    }
}