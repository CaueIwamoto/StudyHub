namespace StudyHub.Domain.Exceptions;

/// <summary>Violação de uma regra de negócio / transição de estado inválida.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
