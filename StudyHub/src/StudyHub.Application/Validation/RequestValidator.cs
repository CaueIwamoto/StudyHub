using System.ComponentModel.DataAnnotations;
using StudyHub.Application.Exceptions;

namespace StudyHub.Application.Validation;

public static class RequestValidator
{
    /// <summary>Valida os atributos DataAnnotations do DTO e lança RequestValidationException (HTTP 400).</summary>
    public static void ValidateObject(object? request)
    {
        if (request is null)
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["request"] = new[] { "O corpo da requisição é obrigatório." }
            });

        var results = new List<ValidationResult>();
        var context = new ValidationContext(request);
        if (Validator.TryValidateObject(request, context, results, validateAllProperties: true))
            return;

        var errors = results
            .SelectMany(r => (r.MemberNames.Any() ? r.MemberNames : new[] { string.Empty })
                .Select(m => (Campo: m, Mensagem: r.ErrorMessage ?? "Valor inválido.")))
            .GroupBy(x => x.Campo)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Mensagem).Distinct().ToArray());

        throw new RequestValidationException(errors);
    }

    internal static List<string> NormalizarAreas(IEnumerable<string>? areas, string campo, ValidationErrors erros)
    {
        var lista = (areas ?? Enumerable.Empty<string>())
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (lista.Count > 20)
            erros.Add(campo, "Informe no máximo 20 áreas de conhecimento.");
        if (lista.Any(a => a.Length > 60))
            erros.Add(campo, "Cada área de conhecimento deve ter no máximo 60 caracteres.");

        return lista;
    }
}
