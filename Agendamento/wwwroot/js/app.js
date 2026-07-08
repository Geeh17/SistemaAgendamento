(() => {
  "use strict";

  const API_BASE = "/api/Tarefas";

  const ICONS = {
    calendar: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="5" width="18" height="16" rx="2"/><path d="M8 3v4M16 3v4M3 10h18"/></svg>',
    api: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M9 4H6a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-3"/><path d="M20 4l-9 9M20 4h-5M20 4v5"/></svg>',
    plus: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 5v14M5 12h14"/></svg>',
    search: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="7"/><path d="m21 21-4.3-4.3"/></svg>',
    edit: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M12 20h9"/><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4Z"/></svg>',
    trash: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M3 6h18"/><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/></svg>'
  };

  function paintIcons(root = document) {
    root.querySelectorAll("[data-icon]").forEach((el) => {
      const name = el.getAttribute("data-icon");
      if (ICONS[name] && !el.dataset.painted) {
        el.innerHTML = ICONS[name];
        el.dataset.painted = "1";
      }
    });
  }

  const STATUS_LABEL = {
    Pendente: "Pendente",
    Confirmado: "Confirmado",
    EmAndamento: "Em andamento",
    Concluido: "Concluído",
    Cancelado: "Cancelado"
  };

  const state = {
    busca: "",
    status: "",
    periodo: "todos",
    tarefas: []
  };

  const els = {};

  document.addEventListener("DOMContentLoaded", () => {
    cacheEls();
    paintIcons();
    bindEvents();
    carregarEstatisticas();
    carregarTarefas();
  });

  function cacheEls() {
    els.ledgerBody = document.getElementById("ledger-body");
    els.ledgerLoading = document.getElementById("ledger-loading");
    els.ledgerEmpty = document.getElementById("ledger-empty");
    els.busca = document.getElementById("busca");
    els.filtroStatus = document.getElementById("filtro-status");
    els.quickFilters = document.querySelectorAll("[data-periodo]");
    els.stats = document.getElementById("stats");

    els.overlay = document.getElementById("overlay");
    els.drawer = document.getElementById("drawer");
    els.drawerTitle = document.getElementById("drawer-title");
    els.form = document.getElementById("form-tarefa");
    els.btnNovo = document.getElementById("btn-novo");
    els.drawerClose = document.getElementById("drawer-close");
    els.btnCancelar = document.getElementById("btn-cancelar");
    els.btnExcluir = document.getElementById("btn-excluir");

    els.fId = document.getElementById("f-id");
    els.fTitulo = document.getElementById("f-titulo");
    els.fData = document.getElementById("f-data");
    els.fHora = document.getElementById("f-hora");
    els.fLocal = document.getElementById("f-local");
    els.fDescricao = document.getElementById("f-descricao");
    els.fStatus = document.getElementById("f-status");
    els.errTitulo = document.getElementById("err-titulo");

    els.toastStack = document.getElementById("toast-stack");
  }

  function bindEvents() {
    let buscaTimer;
    els.busca.addEventListener("input", () => {
      clearTimeout(buscaTimer);
      buscaTimer = setTimeout(() => {
        state.busca = els.busca.value.trim();
        carregarTarefas();
      }, 300);
    });

    els.filtroStatus.addEventListener("change", () => {
      state.status = els.filtroStatus.value;
      carregarTarefas();
    });

    els.quickFilters.forEach((btn) => {
      btn.addEventListener("click", () => {
        els.quickFilters.forEach((b) => b.classList.remove("is-active"));
        btn.classList.add("is-active");
        state.periodo = btn.dataset.periodo;
        carregarTarefas();
      });
    });

    els.btnNovo.addEventListener("click", () => abrirDrawer());
    els.drawerClose.addEventListener("click", fecharDrawer);
    els.btnCancelar.addEventListener("click", fecharDrawer);
    els.overlay.addEventListener("click", fecharDrawer);

    els.form.addEventListener("submit", salvarTarefa);
    els.btnExcluir.addEventListener("click", excluirTarefaAtual);

    document.addEventListener("keydown", (ev) => {
      if (ev.key === "Escape") fecharDrawer();
    });
  }

  function periodoParaDatas(periodo) {
    const agora = new Date();
    if (periodo === "hoje") {
      const inicio = new Date(agora.getFullYear(), agora.getMonth(), agora.getDate(), 0, 0, 0);
      const fim = new Date(agora.getFullYear(), agora.getMonth(), agora.getDate(), 23, 59, 59);
      return { dataInicio: inicio.toISOString(), dataFim: fim.toISOString() };
    }
    if (periodo === "semana") {
      const fim = new Date(agora.getTime() + 7 * 24 * 60 * 60 * 1000);
      return { dataInicio: agora.toISOString(), dataFim: fim.toISOString() };
    }
    return {};
  }

  async function carregarTarefas() {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      if (state.busca) params.set("titulo", state.busca);
      if (state.status) params.set("status", state.status);
      params.set("tamanhoPagina", "200");

      const { dataInicio, dataFim } = periodoParaDatas(state.periodo);
      if (dataInicio) params.set("dataInicio", dataInicio);
      if (dataFim) params.set("dataFim", dataFim);

      const resp = await fetch(`${API_BASE}?${params.toString()}`);
      if (!resp.ok) throw new Error("Falha ao carregar agendamentos.");
      const data = await resp.json();

      state.tarefas = data.itens || [];
      renderLista(state.tarefas);
    } catch (err) {
      mostrarToast(err.message || "Erro ao carregar agendamentos.", "error");
      renderLista([]);
    } finally {
      setLoading(false);
    }
  }

  async function carregarEstatisticas() {
    try {
      const resp = await fetch(`${API_BASE}/estatisticas`);
      if (!resp.ok) throw new Error("Falha ao carregar estatísticas.");
      const stats = await resp.json();

      setStat("total", stats.total);
      setStat("pendentes", stats.pendentes);
      setStat("confirmados", stats.confirmados);
      setStat("concluidos", stats.concluidos);
      setStat("proximos", stats.proximosSeteDias);
    } catch (err) {
      // Painel continua funcional mesmo se as estatísticas falharem.
      console.error(err);
    }
  }

  function setStat(nome, valor) {
    const el = els.stats.querySelector(`[data-stat="${nome}"] .stat-value`);
    if (el) el.textContent = valor ?? "0";
  }

  function setLoading(isLoading) {
    els.ledgerLoading.hidden = !isLoading;
    if (isLoading) {
      els.ledgerEmpty.hidden = true;
    }
  }

  function renderLista(tarefas) {
    els.ledgerBody.innerHTML = "";

    if (!tarefas.length) {
      els.ledgerEmpty.hidden = false;
      return;
    }
    els.ledgerEmpty.hidden = true;

    const frag = document.createDocumentFragment();
    tarefas.forEach((t) => frag.appendChild(criarLinha(t)));
    els.ledgerBody.appendChild(frag);
    paintIcons(els.ledgerBody);
  }

  function criarLinha(t) {
    const row = document.createElement("div");
    row.className = "ledger-row";
    row.dataset.id = t.id;

    const data = new Date(t.dataAgendamento);
    const dia = data.toLocaleDateString("pt-BR", { day: "2-digit", month: "short" });
    const hora = data.toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit" });

    row.innerHTML = `
      <div class="col-data">
        <span class="data-dia">${escapeHtml(dia)}</span>
        <span class="data-hora">${escapeHtml(hora)}</span>
      </div>
      <div class="col-titulo">
        <span class="titulo-nome">${escapeHtml(t.titulo)}</span>
        ${t.descricao ? `<span class="titulo-desc">${escapeHtml(t.descricao)}</span>` : ""}
      </div>
      <div class="col-local">${escapeHtml(t.local || "—")}</div>
      <div class="col-status">
        <span class="status-pill status-${t.status}">${escapeHtml(STATUS_LABEL[t.status] || t.status)}</span>
      </div>
      <div class="col-acoes">
        <button class="icon-btn" type="button" data-acao="editar" aria-label="Editar"><span class="nav-icon" data-icon="edit"></span></button>
        <button class="icon-btn danger" type="button" data-acao="excluir" aria-label="Excluir"><span class="nav-icon" data-icon="trash"></span></button>
      </div>
    `;

    row.querySelector('[data-acao="editar"]').addEventListener("click", () => abrirDrawer(t));
    row.querySelector('[data-acao="excluir"]').addEventListener("click", () => confirmarExclusao(t));

    return row;
  }

  function escapeHtml(value) {
    const div = document.createElement("div");
    div.textContent = value ?? "";
    return div.innerHTML;
  }

  function abrirDrawer(tarefa) {
    els.form.reset();
    limparErros();

    if (tarefa) {
      els.drawerTitle.textContent = "Editar agendamento";
      els.btnExcluir.hidden = false;
      els.fId.value = tarefa.id;

      const data = new Date(tarefa.dataAgendamento);
      els.fTitulo.value = tarefa.titulo;
      els.fData.value = toDateInputValue(data);
      els.fHora.value = toTimeInputValue(data);
      els.fLocal.value = tarefa.local || "";
      els.fDescricao.value = tarefa.descricao || "";
      els.fStatus.value = tarefa.status;
    } else {
      els.drawerTitle.textContent = "Novo agendamento";
      els.btnExcluir.hidden = true;
      els.fId.value = "";
      const agora = new Date(Date.now() + 60 * 60 * 1000);
      els.fData.value = toDateInputValue(agora);
      els.fHora.value = toTimeInputValue(agora);
      els.fStatus.value = "Pendente";
    }

    els.overlay.hidden = false;
    els.drawer.classList.add("is-open");
    els.drawer.setAttribute("aria-hidden", "false");
    setTimeout(() => els.fTitulo.focus(), 50);
  }

  function fecharDrawer() {
    els.drawer.classList.remove("is-open");
    els.drawer.setAttribute("aria-hidden", "true");
    els.overlay.hidden = true;
  }

  function toDateInputValue(date) {
    const pad = (n) => String(n).padStart(2, "0");
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
  }

  function toTimeInputValue(date) {
    const pad = (n) => String(n).padStart(2, "0");
    return `${pad(date.getHours())}:${pad(date.getMinutes())}`;
  }

  function limparErros() {
    els.errTitulo.textContent = "";
  }

  function validarFormulario() {
    limparErros();
    let valido = true;

    if (!els.fTitulo.value.trim()) {
      els.errTitulo.textContent = "Informe um título para o agendamento.";
      valido = false;
    }
    if (!els.fData.value || !els.fHora.value) {
      mostrarToast("Informe a data e a hora do agendamento.", "error");
      valido = false;
    }

    return valido;
  }

  async function salvarTarefa(ev) {
    ev.preventDefault();
    if (!validarFormulario()) return;

    const id = els.fId.value;
    const dataAgendamento = new Date(`${els.fData.value}T${els.fHora.value}:00`).toISOString();

    const payload = {
      titulo: els.fTitulo.value.trim(),
      descricao: els.fDescricao.value.trim() || null,
      local: els.fLocal.value.trim() || null,
      dataAgendamento,
      status: els.fStatus.value
    };

    const btnSalvar = document.getElementById("btn-salvar");
    btnSalvar.disabled = true;

    try {
      let resp;
      if (id) {
        resp = await fetch(`${API_BASE}/${id}`, {
          method: "PUT",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload)
        });
      } else {
        resp = await fetch(API_BASE, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload)
        });
      }

      if (!resp.ok) {
        const erro = await extrairErro(resp);
        throw new Error(erro);
      }

      mostrarToast(id ? "Agendamento atualizado." : "Agendamento criado.", "success");
      fecharDrawer();
      carregarTarefas();
      carregarEstatisticas();
    } catch (err) {
      mostrarToast(err.message || "Não foi possível salvar o agendamento.", "error");
    } finally {
      btnSalvar.disabled = false;
    }
  }

  function confirmarExclusao(tarefa) {
    const confirmado = window.confirm(`Excluir o agendamento "${tarefa.titulo}"? Esta ação não pode ser desfeita.`);
    if (confirmado) excluirTarefa(tarefa.id);
  }

  function excluirTarefaAtual() {
    const id = els.fId.value;
    const titulo = els.fTitulo.value;
    if (!id) return;
    const confirmado = window.confirm(`Excluir o agendamento "${titulo}"? Esta ação não pode ser desfeita.`);
    if (confirmado) {
      excluirTarefa(id);
      fecharDrawer();
    }
  }

  async function excluirTarefa(id) {
    try {
      const resp = await fetch(`${API_BASE}/${id}`, { method: "DELETE" });
      if (!resp.ok && resp.status !== 204) {
        const erro = await extrairErro(resp);
        throw new Error(erro);
      }
      mostrarToast("Agendamento excluído.", "success");
      carregarTarefas();
      carregarEstatisticas();
    } catch (err) {
      mostrarToast(err.message || "Não foi possível excluir o agendamento.", "error");
    }
  }

  async function extrairErro(resp) {
    try {
      const data = await resp.json();
      return data.detail || data.title || "Ocorreu um erro inesperado.";
    } catch {
      return "Ocorreu um erro inesperado.";
    }
  }

  function mostrarToast(mensagem, tipo = "info") {
    const toast = document.createElement("div");
    toast.className = `toast ${tipo === "error" ? "toast-error" : tipo === "success" ? "toast-success" : ""}`;
    toast.textContent = mensagem;
    els.toastStack.appendChild(toast);
    setTimeout(() => toast.remove(), 3800);
  }
})();
