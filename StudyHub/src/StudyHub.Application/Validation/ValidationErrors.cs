using StudyHub.Application.Exceptions;

namespace StudyHub.Application.Validation;

internal sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool Any => _errors.Count > 0;

    public void Add(string campo, string mensagem)
    {
        if (!_errors.TryGetValue(campo, out var lista))
        {
            lista = new List<string>();
            _errors[campo] = lista;
        }
        lista.Add(mensagem);
    }

    public void ThrowIfAny()
    {
        if (_errors.Count == 0) return;
        throw new RequestValidationException(_errors.ToDictionary(k => k.Key, v => v.Value.ToArray()));
    }
}
