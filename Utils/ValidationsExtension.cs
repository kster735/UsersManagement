using System.ComponentModel.DataAnnotations;


namespace UsersManagement.Utils;

public static class ValidationExtensions
{
    public static List<string> Validate<T>(this T model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(model, context, results, true);

        return results.Select(r => r.ErrorMessage ?? "Validation error").ToList();
    }
}
