namespace Catalog.API.Exceptions
{
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException()
            : base("Producto no encontrado")
        {
        }

        public ProductNotFoundException(string name)
            : base($"Producto con nombre '{name}' no encontrado")
        {
        }
    }
}
