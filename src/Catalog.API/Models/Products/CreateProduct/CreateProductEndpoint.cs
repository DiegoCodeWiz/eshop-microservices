using BuildingBlocks.CQRS;
using Carter;
using MediatR;

namespace Catalog.API.Models.Products.CreateProduct
{
    public record CreateProductRequest(string Name, string Description,
        List<string> Category, string ImageFile, decimal Price);
    public record CreateProducResponse(Guid Id);
    public class CreateProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/products", async (CreateProductRequest request, ISender sender) =>
            {
                var command = new CreateProductCommand(request.Name, request.Description, request.Category, request.ImageFile, request.Price);
                var response = await sender.Send(command);
                return Results.Created($"/products/{response.Id}", response);
            })
                .WithName("CrearProducto")
                .Produces<CreateProducResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Crea un nuevo producto")
                .WithDescription("Este endpoint permite crear un nuevo producto en el catálogo. " +
                "Se requiere proporcionar el nombre, descripción, categoría, imágenes y precio del producto. " +
                "y se retorna el identificador de la entidad creada");
        }
    }
}
