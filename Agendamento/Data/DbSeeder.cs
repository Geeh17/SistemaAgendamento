using Agendamento.Models;
using Microsoft.EntityFrameworkCore;

namespace Agendamento.Data
{
    /// <summary>
    /// Garante que o banco exista e, se estiver vazio, insere alguns
    /// agendamentos de exemplo para facilitar a demonstração do sistema.
    /// </summary>
    public static class DbSeeder
    {
        public static void EnsureSeeded(AppDbContext context)
        {
            context.Database.Migrate();

            if (context.Tarefas.Any())
            {
                return;
            }

            var agora = DateTime.UtcNow;

            var exemplos = new List<Tarefa>
            {
                new()
                {
                    Titulo = "Consulta com cliente - Ana Souza",
                    Descricao = "Reunião inicial para levantamento de requisitos do projeto.",
                    Local = "Sala 2 - Escritório Central",
                    DataAgendamento = agora.AddDays(1).Date.AddHours(9),
                    Status = StatusTarefa.Confirmado
                },
                new()
                {
                    Titulo = "Manutenção preventiva - Equipamento 12",
                    Descricao = "Checklist completo de manutenção e troca de peças de desgaste.",
                    Local = "Galpão B",
                    DataAgendamento = agora.AddDays(2).Date.AddHours(14),
                    Status = StatusTarefa.Pendente
                },
                new()
                {
                    Titulo = "Apresentação de proposta comercial",
                    Descricao = "Apresentar proposta final e negociar condições de pagamento.",
                    Local = "Online - Google Meet",
                    DataAgendamento = agora.AddHours(3),
                    Status = StatusTarefa.EmAndamento
                },
                new()
                {
                    Titulo = "Retorno pós-atendimento - João Pedro",
                    Descricao = "Confirmar satisfação do cliente após o atendimento realizado.",
                    Local = "Ligação telefônica",
                    DataAgendamento = agora.AddDays(-1).Date.AddHours(11),
                    Status = StatusTarefa.Concluido
                },
                new()
                {
                    Titulo = "Treinamento da equipe de suporte",
                    Descricao = "Capacitação sobre o novo fluxo de atendimento ao cliente.",
                    Local = "Auditório",
                    DataAgendamento = agora.AddDays(5).Date.AddHours(10),
                    Status = StatusTarefa.Pendente
                },
                new()
                {
                    Titulo = "Visita técnica cancelada",
                    Descricao = "Cliente solicitou remarcação por indisponibilidade.",
                    Local = "Rua das Flores, 123",
                    DataAgendamento = agora.AddDays(-2).Date.AddHours(15),
                    Status = StatusTarefa.Cancelado
                }
            };

            context.Tarefas.AddRange(exemplos);
            context.SaveChanges();
        }
    }
}
