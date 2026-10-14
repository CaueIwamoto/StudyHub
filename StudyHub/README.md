# StudyHub — Backend completo (Usuário, Pedido e Gerenciamento de Propostas)

Plataforma educacional colaborativa (FEI — Tópicos Avançados de Engenharia de Software, 2º semestre de 2026).
Escopo: **Usuário**, **Pedido** (Etapa 1) e **Gerenciamento de Propostas** (Etapa 2). Sem frontend.

Stack: C# / ASP.NET Core Web API (.NET 8), EF Core 8 + Npgsql (PostgreSQL), JWT Bearer, BCrypt, Swagger, xUnit.

> **Aviso de transparência (leia antes de rodar):** o ambiente em que a Etapa 2 foi implementada **não tinha .NET SDK e
> bloqueava o download** (dot.net / NuGet). Portanto `dotnet restore`, `dotnet build` e `dotnet test` **NÃO foram executados**
> — nem para a Etapa 1 nem para a Etapa 2 — e **nenhum teste xUnit foi executado**. O código foi revisado manualmente e o SQL da
> migration (tabela, constraints, índices únicos parciais e bloqueio `FOR UPDATE`) foi validado num PostgreSQL 16 real via `psql`,
> mas isso **não substitui** compilar e testar. Rode `dotnet build && dotnet test` primeiro; veja `CONTINUACAO.md`.

## Estrutura

```
StudyHub.sln
├── src/StudyHub.Domain          Entidades, enums, interfaces de repositório/UoW, DomainException
├── src/StudyHub.Application     DTOs, serviços (casos de uso), validações, exceções de aplicação
├── src/StudyHub.Infrastructure  DbContext, Fluent API, repositórios, migrations, JWT, BCrypt
├── src/StudyHub.Api             Controllers, Program.cs, JWT Bearer, Swagger, middleware de erros
└── tests/StudyHub.Tests         Testes xUnit (EF Core InMemory)
```

## 1. Instalar dependências

Pré-requisitos: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) e Docker (com Docker Compose).

```bash
dotnet restore
dotnet tool restore     # instala o dotnet-ef 8.0.4 (manifesto em .config/dotnet-tools.json)
```

## 2. Configurar variáveis de ambiente

```bash
cp .env.example .env      # Windows PowerShell: Copy-Item .env.example .env
```

Edite o `.env` e defina **pelo menos**:

| Variável | Descrição |
|---|---|
| `POSTGRES_PASSWORD` | senha do PostgreSQL (obrigatória) |
| `Jwt__SecretKey` | chave de assinatura do JWT, **mínimo 32 caracteres** (obrigatória) |

As demais têm padrão (`POSTGRES_DB=studyhub`, `POSTGRES_USER=studyhub`, `POSTGRES_HOST=localhost`, `POSTGRES_PORT=5432`,
`Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpirationMinutes=60`).
A API lê o `.env` automaticamente (procura a partir do diretório atual, subindo as pastas); variáveis já
definidas no ambiente têm prioridade. Alternativamente, defina `ConnectionStrings__DefaultConnection`.

## 3. Inicializar o PostgreSQL

```bash
docker compose up -d
docker compose ps        # aguarde o status "healthy"
```

O `docker-compose.yml` usa as variáveis `POSTGRES_*` do `.env` e persiste os dados no volume `studyhub_pgdata`.

## 4. Executar as migrations

```bash
dotnet ef database update --project src/StudyHub.Infrastructure --startup-project src/StudyHub.Api
```

Em ambiente `Development` a API também aplica as migrations ao iniciar (`Database:MigrateOnStartup=true`),
então este passo é opcional ao rodar via `dotnet run`.

> As migrations `InitialCreate` e `AdicionarProposta` e o `ModelSnapshot` foram escritos manualmente (sem `dotnet ef`). Se preferir gerar pelas
> ferramentas do EF: apague a pasta `src/StudyHub.Infrastructure/Persistence/Migrations` e rode
> `dotnet ef migrations add InitialCreate --project src/StudyHub.Infrastructure --startup-project src/StudyHub.Api --output-dir Persistence/Migrations`.

## 5. Iniciar a API

```bash
dotnet run --project src/StudyHub.Api
```

A API sobe em `http://localhost:5080` (perfil `http` em `launchSettings.json`).

## 6. Abrir o Swagger

`http://localhost:5080/swagger` — use o botão **Authorize** e cole apenas o token JWT retornado no login.
Healthcheck: `GET http://localhost:5080/health`.

## 7. Testar cadastro, login e pedidos

Pelo Swagger ou com curl:

```bash
# 1) Cadastrar um cliente
curl -s -X POST http://localhost:5080/api/auth/register -H "Content-Type: application/json" -d '{
  "nome": "Ana Cliente", "email": "ana@exemplo.com", "senha": "Senha@1234", "tipo": "Cliente" }'

# 2) Cadastrar um prestador
curl -s -X POST http://localhost:5080/api/auth/register -H "Content-Type: application/json" -d '{
  "nome": "Paulo Professor", "email": "paulo@exemplo.com", "senha": "Senha@1234", "tipo": "Prestador",
  "descricaoPerfil": "Professor de português", "areasConhecimento": ["Redação", "Gramática"] }'

# 3) Login (copie o campo "token" da resposta)
curl -s -X POST http://localhost:5080/api/auth/login -H "Content-Type: application/json" -d '{
  "email": "ana@exemplo.com", "senha": "Senha@1234" }'

TOKEN="cole_o_token_aqui"

# 4) Meu perfil
curl -s http://localhost:5080/api/usuarios/me -H "Authorization: Bearer $TOKEN"

# 5) Criar pedido (somente Cliente)
curl -s -X POST http://localhost:5080/api/pedidos -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{
  "titulo": "Redação de português ENEM",
  "descricao": "Preciso de orientação e revisão de uma redação sobre o tema Discriminação Ideológica.",
  "categoria": "Redação", "valorInicial": 100.00 }'

# 6) Listar com filtros e paginação
curl -s "http://localhost:5080/api/pedidos?titulo=reda&categoria=Redação&status=Aberto&valorMinimo=50&valorMaximo=200&pagina=1&tamanhoPagina=10" \
  -H "Authorization: Bearer $TOKEN"

# 7) Meus pedidos / editar / cancelar (use o "id" retornado na criação)
curl -s http://localhost:5080/api/pedidos/meus -H "Authorization: Bearer $TOKEN"
curl -s -X PUT http://localhost:5080/api/pedidos/ID -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{
  "titulo": "Redação ENEM (revisada)", "descricao": "Nova descrição", "categoria": "Redação", "valorInicial": 120.00 }'
curl -s -i -X PATCH http://localhost:5080/api/pedidos/ID/cancelar -H "Authorization: Bearer $TOKEN"   # 204 No Content
```

Com o token de um **Prestador**, `POST /api/pedidos` retorna `403 Forbidden`.

## Executar os testes

```bash
dotnet build
dotnet test
```

Os testes de negócio não precisam de PostgreSQL: usam EF Core InMemory com os serviços e repositórios reais.

**Testes de integração com PostgreSQL real** (concorrência de aceites simultâneos e índices únicos parciais) ficam como
*Skipped* até que você informe um banco de testes (as migrations são aplicadas nele automaticamente):

```bash
docker compose up -d
docker compose exec postgres createdb -U studyhub studyhub_tests
# Linux/macOS
export STUDYHUB_TEST_POSTGRES="Host=localhost;Port=5432;Database=studyhub_tests;Username=studyhub;Password=SUA_SENHA"
# PowerShell: $env:STUDYHUB_TEST_POSTGRES = "Host=localhost;Port=5432;Database=studyhub_tests;Username=studyhub;Password=SUA_SENHA"
dotnet test --filter PostgresIntegrationTests
```

## 8. Gerenciamento de Propostas

| Método | Rota | Acesso | Sucesso |
|---|---|---|---|
| GET | `/api/pedidos/disponiveis` | Prestador | 200 (pedidos `Aberto`/`EmNegociacao`, paginado, mesmos filtros) |
| GET | `/api/pedidos/{id}` | JWT | 200 (detalhes do pedido) |
| POST | `/api/pedidos/{pedidoId}/propostas` | Prestador | 201 |
| GET | `/api/pedidos/{pedidoId}/propostas` | Cliente dono do pedido | 200 (`PropostaDetalhadaResponse[]`, `?status=`) |
| GET | `/api/propostas/minhas` | Prestador | 200 (`?status=`) |
| GET | `/api/propostas/{id}` | Prestador autor ou Cliente dono | 200 |
| PATCH | `/api/propostas/{id}/aceitar` | Cliente dono | 200 |
| PATCH | `/api/propostas/{id}/recusar` | Cliente dono | 200 |

Corpo do POST (o `PrestadorId` **nunca** vem do corpo; é a claim `sub` do JWT):

```json
{ "tipo": "Contraproposta", "valorProposto": 150.00, "mensagem": "Tenho experiência com redações do ENEM." }
```

`tipo`: `AceiteValorInicial` (valor deve ser igual ao `valorInicial` do pedido) ou `Contraproposta` (valor diferente).
Enums são serializados como texto (`Pendente`, `Aceita`, `Recusada`, `NaoSelecionada`, `Aberto`, `EmNegociacao`, ...).
`PropostaDetalhadaResponse` traz a proposta, o resumo do pedido (status e `valorAcordado` = resultado da negociação) e o
**perfil público do prestador** (sem e-mail); `GET /api/usuarios/{id}` também continua disponível.

### Fluxo completo no Swagger (`http://localhost:5080/swagger`)

1. `POST /api/auth/register` (Cliente) e `POST /api/auth/register` (Prestador); depois `POST /api/auth/login` como **cliente**; **Authorize** com o token.
2. `POST /api/pedidos` com `valorInicial: 100` → anote o `id`.
3. `login` como **prestador** → **Authorize** com o novo token.
4. `POST /api/pedidos/{pedidoId}/propostas` (`AceiteValorInicial` com 100, ou `Contraproposta` com 150) → proposta `Pendente`; pedido vira `EmNegociacao`.
5. `login` como **cliente** → `GET /api/pedidos/{pedidoId}/propostas` (valor proposto + perfil do prestador).
6. `PATCH /api/propostas/{id}/aceitar` (pedido `Fechado`, `valorAcordado` = valor da proposta, demais pendentes `NaoSelecionada`)
   **ou** `PATCH /api/propostas/{id}/recusar` (proposta `Recusada`; sem outras pendentes o pedido volta a `Aberto`).
7. Status final: `GET /api/pedidos/{id}` e `GET /api/propostas/{id}` (o prestador acompanha em `GET /api/propostas/minhas`).

### Regras, transações e concorrência

- Regras 1–20 do enunciado estão em `PropostaService` e nas entidades `Proposta`/`Pedido` (transições de estado). Violações de regra de
  estado retornam `409`; permissão, `403`; valores inválidos, `400`; recurso inexistente, `404`.
- **Aceitar, recusar, enviar proposta, editar e cancelar pedido** rodam em transação (`IUnitOfWork.ExecuteInTransactionAsync`) e
  **bloqueiam a linha do pedido** (`SELECT ... FOR UPDATE`, `IPedidoRepository.GetByIdParaAtualizacaoAsync`). Dois aceites simultâneos
  são serializados: o segundo enxerga o pedido já `Fechado` e recebe `409`.
- Garantias finais no banco (migration `AdicionarProposta`): índice único parcial `(PedidoId, PrestadorId) WHERE Status = 1` (uma proposta
  `Pendente` por prestador/pedido), índice único parcial `(PedidoId) WHERE Status = 2` (uma `Aceita` por pedido), `ValorProposto > 0`,
  `numeric(18,2)`, FKs com `Restrict` (histórico preservado), índices por `PedidoId` e `PrestadorId`.
- Cancelar um pedido `EmNegociacao` marca as propostas pendentes como `NaoSelecionada`. Editar pedido `EmNegociacao` retorna `409`.

## Códigos HTTP

`200 OK`, `201 Created` (cadastro, criação de pedido), `204 No Content` (cancelar pedido), `400` (validação),
`401` (sem token/credenciais inválidas), `403` (papel/propriedade), `404`, `409` (e-mail duplicado ou transição de status não permitida).
Erros seguem o formato `application/problem+json`; erros inesperados retornam `500` genérico, sem detalhes internos.
