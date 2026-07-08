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
├── Controllers/        Endpoints HTTP (TarefasController)
├── Services/            Regras de negócio + exceções de domínio
├── Repositories/        Acesso a dados via EF Core
├── Data/                 DbContext + seed de dados de exemplo
├── Models/               Entidade Tarefa e enum StatusTarefa
├── DTOs/                 Contratos de entrada/saída da API
├── Middleware/           Tratamento global de exceções
├── Extensions/           Registro de serviços (DI, Swagger, CORS)
└── wwwroot/              Front-end (HTML/CSS/JS) do painel
```

## Próximos passos sugeridos

- Autenticação/autorização (ex.: JWT) se o sistema for exposto publicamente.
- Testes automatizados (xUnit) para `TarefaService`.
- Deploy como container Docker (o SQLite facilita: um único arquivo de dados).
