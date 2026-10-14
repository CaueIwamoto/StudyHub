using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StudyHub.Application.Exceptions;
using StudyHub.Domain.Exceptions;

namespace StudyHub.Api.Middleware;

/// <summary>
/// Tratamento global de exceções. Converte exceções conhecidas em respostas ProblemDetails com o status HTTP adequado
/// e NUNCA expõe detalhes internos (stack trace, mensagens de banco) em erros inesperados.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Cliente cancelou a requisição; nada a responder.
            if (!context.Response.HasStarted)
                context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogError(ex, "Exceção após o início da resposta.");
            return;
        }

        ProblemDetails problem;

        switch (ex)
        {
            case RequestValidationException v:
                problem = new ValidationProblemDetails(v.Errors.ToDictionary(k => k.Key, k => k.Value))
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Erro de validação.",
                    Detail = v.Message
                };
                break;
            case BadHttpRequestException:
            case JsonException:
                problem = Create(StatusCodes.Status400BadRequest, "Requisição inválida.", "O corpo da requisição é inválido.");
                break;
            case UnauthorizedException u:
                problem = Create(StatusCodes.Status401Unauthorized, "Não autenticado.", u.Message);
                break;
            case ForbiddenException f:
                problem = Create(StatusCodes.Status403Forbidden, "Acesso negado.", f.Message);
                break;
            case NotFoundException n:
                problem = Create(StatusCodes.Status404NotFound, "Recurso não encontrado.", n.Message);
                break;
            case ConflictException c:
                problem = Create(StatusCodes.Status409Conflict, "Conflito.", c.Message);
                break;
            case DomainException d:
                problem = Create(StatusCodes.Status409Conflict, "Operação não permitida.", d.Message);
                break;
            default:
                _logger.LogError(ex, "Erro não tratado em {Method} {Path}", context.Request.Method, context.Request.Path);
                problem = Create(StatusCodes.Status500InternalServerError, "Erro interno do servidor.",
                    "Ocorreu um erro inesperado. Tente novamente mais tarde.");
                break;
        }

        problem.Instance = context.Request.Path;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.Clear();
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(
            problem, problem.GetType(), (JsonSerializerOptions?)null, "application/problem+json", context.RequestAborted);
    }

    private static ProblemDetails Create(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };
}
