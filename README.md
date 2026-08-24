# Sistema de Agendamento

Aplicação completa (back-end + front-end) para gestão de agendamentos, compromissos e tarefas.

## O que mudou nesta versão

O projeto original era um único controller falando diretamente com o `DbContext`, sem
front-end, sem tratamento de erros e com uma connection string presa a uma máquina
específica. Esta versão reorganiza o back-end em camadas e entrega um painel web completo.

**Back-end**
- Arquitetura em camadas: `Controllers` → `Services` → `Repositories` → `Data`, com DTOs
  próprios de entrada/saída (a API nunca expõe a entidade do banco diretamente).
- Regras de negócio centralizadas em `TarefaService`, com exceções de domínio
  (`NotFoundException`, `BusinessRuleException`) tratadas por um middleware global que
  devolve respostas padronizadas (`ProblemDetails`), sem vazar stack trace.
- Mantido o **SQL Server** (mesma connection string do projeto original, apontando para
  a instância local `SQLEXPRESS`). O banco e a tabela `Tarefas` são criados
  automaticamente na primeira execução (`EnsureCreated`), já com alguns agendamentos de
  exemplo — não é necessário rodar migrations.
- Endpoints RESTful em `/api/Tarefas` (listar com filtros e paginação, obter por id,
  criar, atualizar, excluir, estatísticas para o painel).
- Swagger configurado com descrição da API, disponível em `/swagger`.
- CORS habilitado para permitir consumo por um front-end separado, se desejar.
- `Startup.cs` (arquivo antigo, não referenciado por nada e incompatível com o
  `Program.cs` atual) e as migrations antigas (que já não batiam com o modelo) foram
  removidos. O banco agora é criado automaticamente a partir do modelo atual.

**Front-end**
- Painel web em `wwwroot/`, servido pelo próprio ASP.NET Core (sem precisar de Node,
  build step ou servidor separado): liste, busque, filtre por status e período, crie,
  edite e exclua agendamentos, com estatísticas em tempo real.

## Como rodar

Pré-requisito: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
cd Agendamento
dotnet restore
dotnet run
```

O terminal vai mostrar a URL (algo como `http://localhost:5245`). Abra essa URL no
navegador para ver o painel. A documentação interativa da API fica em `/swagger`.

O projeto usa a instância `DESKTOP-IE1FUDG\SQLEXPRESS` configurada em
`appsettings.json`. Se você rodar em outra máquina, ajuste a `DefaultConnection` para
apontar para o seu SQL Server/SQLEXPRESS local. O banco `Agendamento` e a tabela
`Tarefas` são criados automaticamente na primeira execução.

### Rodando pelo Visual Studio

Abra `SistemaAgendamento.sln`, defina `Agendamento` como projeto de inicialização e
pressione F5. O navegador abre automaticamente no painel.

## Estrutura do projeto

```
Agendamento/
├── Controllers/        Endpoints HTTP (TarefasController, AuthController)
├── Services/            Regras de negócio + exceções de domínio + autenticação (Services/Auth)
├── Repositories/        Acesso a dados via EF Core
├── Data/                 DbContext + seed de dados de exemplo e do usuário admin
├── Models/               Entidades (Tarefa, Usuario) e enum StatusTarefa
├── DTOs/                 Contratos de entrada/saída da API (incl. DTOs/Auth)
├── Middleware/           Tratamento global de exceções
├── Extensions/           Registro de serviços (DI, Swagger, CORS, JWT)
├── Migrations/           Migrations do EF Core (Tarefas, Usuarios)
└── wwwroot/              Front-end (HTML/CSS/JS) do painel, com tela de login

Agendamento.Tests/
└── Services/             Testes xUnit para TarefaService
```

## Próximos passos sugeridos

- ~~Autenticação/autorização (ex.: JWT) se o sistema for exposto publicamente.~~ ✅ Implementado — ver seção [Autenticação](#autenticação-jwt) abaixo.
- ~~Testes automatizados (xUnit) para `TarefaService`.~~ ✅ Implementado — ver seção [Testes automatizados](#testes-automatizados) abaixo.
- Deploy como container Docker (o SQLite facilita: um único arquivo de dados).

## Autenticação (JWT)

A API agora exige um token JWT em todos os endpoints de `/api/Tarefas`. O painel web
(`wwwroot/`) já foi adaptado: ele mostra uma tela de login antes de exibir a agenda e
anexa o token automaticamente em cada chamada.

**Login**

```
POST /api/Auth/login
Content-Type: application/json

{
  "nomeUsuario": "admin",
  "senha": "Admin@123"
}
```

Resposta:

```json
{
  "token": "eyJhbGciOi...",
  "expiraEm": "2026-08-24T13:00:00Z",
  "nomeUsuario": "admin",
  "papel": "Admin"
}
```

Use o token nas chamadas subsequentes: `Authorization: Bearer {token}`. No Swagger
(`/swagger`), clique em **Authorize** e cole `Bearer {token}`.

**Usuário administrador inicial**

Na primeira execução, um usuário administrador é criado automaticamente com base nas
chaves `AdminSeed:NomeUsuario` / `AdminSeed:SenhaInicial` do `appsettings.json`
(por padrão, `admin` / `Admin@123`). A senha é sempre armazenada com hash
(`PasswordHasher<Usuario>`) — nunca em texto puro.

**Configuração (`appsettings.json` → seção `Jwt`)**

| Chave                | Descrição                                                        |
|-----------------------|-------------------------------------------------------------------|
| `Jwt:Key`              | Chave de assinatura (mín. 32 caracteres). **Troque em produção** e prefira uma variável de ambiente / cofre de segredos em vez de deixar fixa no arquivo. |
| `Jwt:Issuer`           | Emissor do token.                                                  |
| `Jwt:Audience`         | Audiência esperada do token.                                       |
| `Jwt:ExpiracaoMinutos` | Tempo de vida do token, em minutos (padrão: 60).                   |

> ⚠️ O valor incluído em `appsettings.json` é apenas para desenvolvimento local. Antes de
> expor a API publicamente, gere uma chave própria (ex.: `openssl rand -base64 48`) e
> troque a senha inicial do usuário `admin`.

Como a tabela `Usuarios` é criada via migration (`AddUsuarios`) e o `DbSeeder` roda
`context.Database.Migrate()` na inicialização, basta rodar `dotnet run` normalmente —
nenhum passo manual adicional é necessário em bancos novos. Bancos já existentes de uma
versão anterior deste projeto também recebem a nova tabela automaticamente na próxima
inicialização, graças à migration.

## Testes automatizados

O projeto `Agendamento.Tests` (xUnit + Moq + FluentAssertions) cobre as regras de
negócio de `TarefaService`: busca por id (encontrado/não encontrado), criação
(normalização de campos), atualização, remoção, listagem/paginação (incluindo os
limites de página e tamanho de página) e o cálculo de estatísticas do painel.

```bash
cd Agendamento.Tests
dotnet test
```

