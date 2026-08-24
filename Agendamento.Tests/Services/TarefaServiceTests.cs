using Agendamento.DTOs;
using Agendamento.Models;
using Agendamento.Repositories;
using Agendamento.Services;
using Agendamento.Services.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Agendamento.Tests.Services
{
    public class TarefaServiceTests
    {
        private readonly Mock<ITarefaRepository> _repositoryMock;
        private readonly TarefaService _service;

        public TarefaServiceTests()
        {
            _repositoryMock = new Mock<ITarefaRepository>();
            var loggerMock = new Mock<ILogger<TarefaService>>();
            _service = new TarefaService(_repositoryMock.Object, loggerMock.Object);
        }

        private static Tarefa CriarTarefaExemplo(int id = 1, StatusTarefa status = StatusTarefa.Pendente, DateTime? dataAgendamento = null)
        {
            return new Tarefa
            {
                Id = id,
                Titulo = "Consulta com cliente",
                Descricao = "Levantamento de requisitos",
                Local = "Sala 2",
                DataAgendamento = dataAgendamento ?? DateTime.UtcNow.AddDays(1),
                DataCriacao = DateTime.UtcNow,
                Status = status
            };
        }

        // ---------- ObterPorIdAsync ----------

        [Fact]
        public async Task ObterPorIdAsync_QuandoTarefaExiste_RetornaDtoCorrespondente()
        {
            // Arrange
            var tarefa = CriarTarefaExemplo(id: 42);
            _repositoryMock.Setup(r => r.ObterPorIdAsync(42)).ReturnsAsync(tarefa);

            // Act
            var resultado = await _service.ObterPorIdAsync(42);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(42);
            resultado.Titulo.Should().Be(tarefa.Titulo);
            resultado.StatusDescricao.Should().Be(tarefa.Status.ToString());
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoTarefaNaoExiste_LancaNotFoundException()
        {
            // Arrange
            _repositoryMock.Setup(r => r.ObterPorIdAsync(It.IsAny<int>())).ReturnsAsync((Tarefa?)null);

            // Act
            var acao = async () => await _service.ObterPorIdAsync(999);

            // Assert
            await acao.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*999*");
        }

        // ---------- CriarAsync ----------

        [Fact]
        public async Task CriarAsync_DeveApararEspacosEChamarRepositorio()
        {
            // Arrange
            var dto = new TarefaCreateDto
            {
                Titulo = "  Reunião de alinhamento  ",
                Descricao = "  Discutir cronograma  ",
                Local = "  Sala 3  ",
                DataAgendamento = DateTime.UtcNow.AddDays(2),
                Status = StatusTarefa.Confirmado
            };

            Tarefa? tarefaCapturada = null;
            _repositoryMock
                .Setup(r => r.AdicionarAsync(It.IsAny<Tarefa>()))
                .Callback<Tarefa>(t =>
                {
                    t.Id = 10;
                    tarefaCapturada = t;
                })
                .ReturnsAsync((Tarefa t) => t);

            // Act
            var resultado = await _service.CriarAsync(dto);

            // Assert
            tarefaCapturada.Should().NotBeNull();
            tarefaCapturada!.Titulo.Should().Be("Reunião de alinhamento");
            tarefaCapturada.Descricao.Should().Be("Discutir cronograma");
            tarefaCapturada.Local.Should().Be("Sala 3");
            tarefaCapturada.Status.Should().Be(StatusTarefa.Confirmado);

            resultado.Id.Should().Be(10);
            resultado.Titulo.Should().Be("Reunião de alinhamento");

            _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Tarefa>()), Times.Once);
            _repositoryMock.Verify(r => r.SalvarAsync(), Times.Once);
        }

        [Fact]
        public async Task CriarAsync_QuandoCamposOpcionaisNulos_MantemComoNulo()
        {
            // Arrange
            var dto = new TarefaCreateDto
            {
                Titulo = "Ligação de retorno",
                Descricao = null,
                Local = null,
                DataAgendamento = DateTime.UtcNow.AddHours(5),
                Status = StatusTarefa.Pendente
            };

            _repositoryMock
                .Setup(r => r.AdicionarAsync(It.IsAny<Tarefa>()))
                .ReturnsAsync((Tarefa t) => t);

            // Act
            var resultado = await _service.CriarAsync(dto);

            // Assert
            resultado.Descricao.Should().BeNull();
            resultado.Local.Should().BeNull();
        }

        // ---------- AtualizarAsync ----------

        [Fact]
        public async Task AtualizarAsync_QuandoTarefaExiste_AtualizaCamposESalva()
        {
            // Arrange
            var tarefaExistente = CriarTarefaExemplo(id: 5, status: StatusTarefa.Pendente);
            _repositoryMock.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(tarefaExistente);

            var dto = new TarefaUpdateDto
            {
                Titulo = "  Novo título  ",
                Descricao = "  Nova descrição  ",
                Local = "  Novo local  ",
                DataAgendamento = DateTime.UtcNow.AddDays(3),
                Status = StatusTarefa.Confirmado
            };

            // Act
            var resultado = await _service.AtualizarAsync(5, dto);

            // Assert
            resultado.Titulo.Should().Be("Novo título");
            resultado.Descricao.Should().Be("Nova descrição");
            resultado.Local.Should().Be("Novo local");
            resultado.Status.Should().Be(StatusTarefa.Confirmado);

            _repositoryMock.Verify(r => r.Atualizar(tarefaExistente), Times.Once);
            _repositoryMock.Verify(r => r.SalvarAsync(), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_QuandoTarefaNaoExiste_LancaNotFoundExceptionSemChamarSalvar()
        {
            // Arrange
            _repositoryMock.Setup(r => r.ObterPorIdAsync(It.IsAny<int>())).ReturnsAsync((Tarefa?)null);
            var dto = new TarefaUpdateDto
            {
                Titulo = "Título qualquer",
                DataAgendamento = DateTime.UtcNow,
                Status = StatusTarefa.Pendente
            };

            // Act
            var acao = async () => await _service.AtualizarAsync(123, dto);

            // Assert
            await acao.Should().ThrowAsync<NotFoundException>();
            _repositoryMock.Verify(r => r.SalvarAsync(), Times.Never);
        }

        // ---------- RemoverAsync ----------

        [Fact]
        public async Task RemoverAsync_QuandoTarefaExiste_RemoveESalva()
        {
            // Arrange
            var tarefa = CriarTarefaExemplo(id: 7);
            _repositoryMock.Setup(r => r.ObterPorIdAsync(7)).ReturnsAsync(tarefa);

            // Act
            await _service.RemoverAsync(7);

            // Assert
            _repositoryMock.Verify(r => r.Remover(tarefa), Times.Once);
            _repositoryMock.Verify(r => r.SalvarAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoverAsync_QuandoTarefaNaoExiste_LancaNotFoundExceptionSemChamarSalvar()
        {
            // Arrange
            _repositoryMock.Setup(r => r.ObterPorIdAsync(It.IsAny<int>())).ReturnsAsync((Tarefa?)null);

            // Act
            var acao = async () => await _service.RemoverAsync(404);

            // Assert
            await acao.Should().ThrowAsync<NotFoundException>();
            _repositoryMock.Verify(r => r.Remover(It.IsAny<Tarefa>()), Times.Never);
            _repositoryMock.Verify(r => r.SalvarAsync(), Times.Never);
        }

        // ---------- ListarAsync ----------

        [Fact]
        public async Task ListarAsync_DeveMapearItensERespeitarPaginacaoInformada()
        {
            // Arrange
            var itens = new List<Tarefa>
            {
                CriarTarefaExemplo(id: 1),
                CriarTarefaExemplo(id: 2)
            };
            var filtro = new TarefaFiltro { Pagina = 2, TamanhoPagina = 10 };

            _repositoryMock
                .Setup(r => r.ListarAsync(filtro))
                .ReturnsAsync((itens, 25));

            // Act
            var resultado = await _service.ListarAsync(filtro);

            // Assert
            resultado.Itens.Should().HaveCount(2);
            resultado.Itens.Select(i => i.Id).Should().BeEquivalentTo(new[] { 1, 2 });
            resultado.PaginaAtual.Should().Be(2);
            resultado.TamanhoPagina.Should().Be(10);
            resultado.TotalItens.Should().Be(25);
            resultado.TotalPaginas.Should().Be(3);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-5, 1)]
        [InlineData(3, 3)]
        public async Task ListarAsync_QuandoPaginaInvalida_NormalizaParaUm(int paginaInformada, int paginaEsperada)
        {
            // Arrange
            var filtro = new TarefaFiltro { Pagina = paginaInformada, TamanhoPagina = 20 };
            _repositoryMock
                .Setup(r => r.ListarAsync(filtro))
                .ReturnsAsync((new List<Tarefa>(), 0));

            // Act
            var resultado = await _service.ListarAsync(filtro);

            // Assert
            resultado.PaginaAtual.Should().Be(paginaEsperada);
        }

        [Theory]
        [InlineData(0, 20)]
        [InlineData(-1, 20)]
        [InlineData(500, 20)]
        [InlineData(50, 50)]
        public async Task ListarAsync_QuandoTamanhoPaginaForaDoIntervalo_NormalizaParaVinte(int tamanhoInformado, int tamanhoEsperado)
        {
            // Arrange
            var filtro = new TarefaFiltro { Pagina = 1, TamanhoPagina = tamanhoInformado };
            _repositoryMock
                .Setup(r => r.ListarAsync(filtro))
                .ReturnsAsync((new List<Tarefa>(), 0));

            // Act
            var resultado = await _service.ListarAsync(filtro);

            // Assert
            resultado.TamanhoPagina.Should().Be(tamanhoEsperado);
        }

        // ---------- ObterEstatisticasAsync ----------

        [Fact]
        public async Task ObterEstatisticasAsync_DeveContarCorretamentePorStatusEProximosSeteDias()
        {
            // Arrange
            var agora = DateTime.UtcNow;
            var todas = new List<Tarefa>
            {
                CriarTarefaExemplo(id: 1, status: StatusTarefa.Pendente, dataAgendamento: agora.AddDays(1)),
                CriarTarefaExemplo(id: 2, status: StatusTarefa.Confirmado, dataAgendamento: agora.AddDays(3)),
                CriarTarefaExemplo(id: 3, status: StatusTarefa.EmAndamento, dataAgendamento: agora.AddDays(6)),
                CriarTarefaExemplo(id: 4, status: StatusTarefa.Concluido, dataAgendamento: agora.AddDays(-2)),
                CriarTarefaExemplo(id: 5, status: StatusTarefa.Cancelado, dataAgendamento: agora.AddDays(2)),
                // Fora da janela de 7 dias, não deve ser contabilizada em ProximosSeteDias
                CriarTarefaExemplo(id: 6, status: StatusTarefa.Pendente, dataAgendamento: agora.AddDays(10)),
                // Dentro da janela, mas cancelada — não deve contar em ProximosSeteDias
                CriarTarefaExemplo(id: 7, status: StatusTarefa.Cancelado, dataAgendamento: agora.AddDays(4))
            };

            _repositoryMock.Setup(r => r.ObterTodosParaEstatisticasAsync()).ReturnsAsync(todas);

            // Act
            var estatisticas = await _service.ObterEstatisticasAsync();

            // Assert
            estatisticas.Total.Should().Be(7);
            estatisticas.Pendentes.Should().Be(2);
            estatisticas.Confirmados.Should().Be(1);
            estatisticas.EmAndamento.Should().Be(1);
            estatisticas.Concluidos.Should().Be(1);
            estatisticas.Cancelados.Should().Be(2);
            // Dentro dos próximos 7 dias e não cancelados: itens 1 (dia 1), 2 (dia 3) e 3 (dia 6).
            // Item 5 está no período mas é Cancelado; item 6 está fora do período; item 4 é no passado.
            estatisticas.ProximosSeteDias.Should().Be(3);
        }

        [Fact]
        public async Task ObterEstatisticasAsync_QuandoNaoHaTarefas_RetornaTudoZerado()
        {
            // Arrange
            _repositoryMock.Setup(r => r.ObterTodosParaEstatisticasAsync()).ReturnsAsync(new List<Tarefa>());

            // Act
            var estatisticas = await _service.ObterEstatisticasAsync();

            // Assert
            estatisticas.Total.Should().Be(0);
            estatisticas.Pendentes.Should().Be(0);
            estatisticas.Confirmados.Should().Be(0);
            estatisticas.EmAndamento.Should().Be(0);
            estatisticas.Concluidos.Should().Be(0);
            estatisticas.Cancelados.Should().Be(0);
            estatisticas.ProximosSeteDias.Should().Be(0);
        }
    }
}
