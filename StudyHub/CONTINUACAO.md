Hu# CONTINUACAO.md — Estado após a Etapa 2

## 1. Resultado real da compilação e dos testes (leia primeiro)

**Nada foi compilado nem testado** (nem na Etapa 1, nem na Etapa 2). O ambiente da Etapa 2 não tinha o .NET SDK e a rede
bloqueava dot.net, NuGet, apt e os registries de pacotes necessários.

- `dotnet restore`: **NÃO executado**
- `dotnet build`: **NÃO executado**
- `dotnet test`: **NÃO executado** (nenhum teste xUnit rodou; nenhum deve ser considerado aprovado)
- `dotnet ef`: **NÃO executado**; a migration `20261014010000_AdicionarProposta` e o `ModelSnapshot` foram escritos à mão.

O que foi de fato verificado:
- Revisão manual do código C# (usings, assinaturas, injeção de dependência, tipos dos lambdas de transação).
- No PostgreSQL 16 local, via `psql`: o DDL equivalente à migration (tabela, CHECKs, FKs, índices únicos parciais) executa sem erro;
  uma segunda proposta `Pendente` do mesmo prestador/pedido é barrada pelo índice; duas propostas `Aceita` no mesmo pedido são
  barradas pelo índice; `SELECT ... FOR UPDATE` na linha do pedido faz a segunda sessão esperar a primeira terminar.

Primeiros passos na sua máquina:

```bash
dotnet restore && dotnet build && dotnet test
```

Pontos de maior risco: erros de compilação pequenos (não detectáveis sem compilador), a migration/snapshot manuais (se necessário,
apague `Persistence/Migrations` e regere: `dotnet ef migrations add InitialCreate ...` e depois `AdicionarProposta`), a consulta
`FromSqlInterpolated(... FOR UPDATE)` do `PedidoRepository` e os testes de integração PostgreSQL (`STUDYHUB_TEST_POSTGRES`).

## 2. O que a Etapa 2 acrescentou

- **Domain**: `Proposta`, `TipoProposta`, `StatusProposta`, `IPropostaRepository`; `Pedido.RegistrarProposta/Fechar/Reabrir/EstaDisponivel`;
  `IPedidoRepository.GetByIdParaAtualizacaoAsync` (bloqueio de linha); `PedidoFiltro.ApenasDisponiveis`.
- **Application**: `CriarPropostaRequest`, `PropostaResponse`, `PropostaDetalhadaResponse`, `IPropostaService`/`PropostaService`;
  `PedidoService` agora edita/cancela em transação com bloqueio e encerra propostas pendentes ao cancelar; `ListarDisponiveisAsync`.
- **Infrastructure**: `PropostaConfiguration` (Fluent API), `PropostaRepository`, migration incremental `AdicionarProposta`,
  `UnitOfWork` traduz violações dos índices únicos parciais em 409 com mensagem adequada.
- **Api**: `PropostasController` (6 endpoints) e `GET /api/pedidos/disponiveis`.
- **Tests**: `PropostaTests` (cenários 1–4 e regras), `PostgresIntegrationTests` (concorrência real, *Skipped* sem
  `STUDYHUB_TEST_POSTGRES`), contratos de autorização em `ApiContractTests`; `TestEnvironment` ignora o aviso de transação do InMemory.

## 3. Decisões/suposições

- Etapa 1 (mantidas): `Categoria` opcional ("Geral"); filtros de valor usam `ValorInicial`; `GET /api/usuarios/{id}` anônimo.
- `PedidoService.CancelarAsync` agora também marca propostas pendentes como `NaoSelecionada` (pendência apontada na Etapa 1).
- `AceiteValorInicial` exige `valorProposto` igual ao `ValorInicial` (regra 7), ou seja, o campo continua obrigatório no corpo.
- Valores são arredondados a 2 casas (AwayFromZero) antes das regras 6–8, como já era feito em `ValorInicial`.
- Acesso a proposta de terceiros retorna `403` (e não `404`), seguindo o padrão da Etapa 1.
- Concorrência: bloqueio pessimista da linha do pedido + transação; índices únicos parciais como garantia final. Não foi usado
  `xmin` como token de concorrência para evitar migration/snapshot manuais mais frágeis.
- Docker Compose continua provisionando apenas o PostgreSQL; a API roda via `dotnet run`.
- Fora do escopo (não implementado): frontend, Pagamento, Contrato, Avaliação, Agenda, Chat, Gerenciamento de Serviço.
