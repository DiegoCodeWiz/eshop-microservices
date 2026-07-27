namespace Catalog.API.Models.Products.DeleteProductByName
{
    public class DeleteProductByNameEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/products/by-name/{name}", async (string name, ISender sender) =>
            {
                var command = new DeleteProductByNameCommand(name);
                var response = await sender.Send(command);
                return Results.Ok(response);
            })
                .WithName("EliminarProductoPorNombre")
                .Produces<DeleteProductByNameResult>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithSummary("Elimina un producto por nombre")
                .WithDescription("Este endpoint elimina el producto cuyo nombre coincide (sin distinguir mayusculas/minusculas) con el valor recibido en la ruta.");
        }
    }
}
