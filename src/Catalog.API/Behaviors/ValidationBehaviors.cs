using FluentValidation;

namespace Catalog.API.Behaviors
{
    public class ValidationBehaviors<TRequest,TResponse>:IPipelineBehavior<TRequest,TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        public ValidationBehaviors(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }
        /*este metodo se encarga de manejar la peticion handler */
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any()) 
                return await next();
            var context = new ValidationContext<TRequest>(request);

            var validationResult = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResult
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();
            if (failures.Count != 0)
                throw new ValidationException(failures);
            return await next();
        }
    }
}
