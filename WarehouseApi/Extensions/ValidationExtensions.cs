using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WarehouseApi.Extensions;

public static class ValidationExtensions
{
    public static bool TryValidate<T>(T model, out ValidationProblemDetails? problem)
    {
        var context = new ValidationContext(model!);
        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(model!, context, results, validateAllProperties: true);

        if (valid)
        {
            // Дополнительно валидируем элементы коллекций (позиции поставки и т.п.).
            if (model is IEnumerable<object> sequence)
            {
                foreach (var item in sequence)
                {
                    if (item is null) continue;
                    var itemContext = new ValidationContext(item);
                    var itemResults = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(item, itemContext, itemResults, true))
                    {
                        results.AddRange(itemResults);
                        valid = false;
                    }
                }
            }
        }

        // Рекурсивно валидируем свойства-коллекции у DTO.
        if (valid)
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.PropertyType != typeof(string) &&
                    typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType))
                {
                    if (prop.GetValue(model) is System.Collections.IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            if (item is null) continue;
                            var itemContext = new ValidationContext(item);
                            var itemResults = new List<ValidationResult>();
                            if (!Validator.TryValidateObject(item, itemContext, itemResults, true))
                            {
                                results.AddRange(itemResults);
                                valid = false;
                            }
                        }
                    }
                }
            }
        }

        if (valid)
        {
            problem = null;
            return true;
        }

        var errors = results
            .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => r.ErrorMessage ?? "Ошибка валидации").ToArray());

        problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Ошибка валидации",
            Detail = "Один или несколько входных параметров некорректны."
        };
        return false;
    }
}
