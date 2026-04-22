function safeText(value, fallback = '--') {
    return value === null || value === undefined || value === '' ? fallback : String(value);
}

function setHealthBar(elementId, current, max) {
    const element = document.getElementById(elementId);
    if (!element) return;
    const ratio = max > 0 ? Math.max(0, Math.min(100, (current / max) * 100)) : 0;
    element.style.width = `${ratio}%`;
}

function resultLabel(result) {
    switch (result) {
        case 'win': return 'Victory';
        case 'lose': return 'Defeat';
        case 'draw': return 'Draw';
        case 'next': return 'In progress';
        default: return 'Awaiting game';
    }
}

function renderTokenGrid(elementId, items, renderer, emptyText) {
    const element = document.getElementById(elementId);
    if (!element) return;

    if (!items || items.length === 0) {
        element.className = 'token-grid empty-state-box';
        element.textContent = emptyText;
        return;
    }

    element.className = 'token-grid';
    element.innerHTML = items.map(renderer).join('');
}

function renderActor(prefix, actor) {
    const hp = document.getElementById(prefix === 'player' ? 'pHP' : 'eHP');
    const name = document.getElementById(prefix === 'player' ? 'playerName' : 'enemyName');
    const profession = document.getElementById(prefix === 'player' ? 'playerProfession' : 'enemyProfession');

    if (!actor) {
        if (name) name.textContent = prefix === 'player' ? 'You' : 'Robert';
        if (profession) profession.textContent = 'None';
        if (hp) hp.textContent = 'HP --/--';
        setHealthBar(prefix === 'player' ? 'playerHealthFill' : 'enemyHealthFill', 0, 1);
        renderTokenGrid(`${prefix}Resources`, [], () => '', 'No data yet.');
        renderTokenGrid(`${prefix}Defenses`, [], () => '', 'No active defenses.');
        renderTokenGrid(`${prefix}Skills`, [], () => '', 'No data yet.');
        return;
    }

    if (name) name.textContent = safeText(actor.name, prefix === 'player' ? 'You' : 'Robert');
    if (profession) profession.textContent = safeText(actor.profession, 'None');
    if (hp) hp.textContent = `HP ${actor.hp}/${actor.maxHP}`;
    setHealthBar(prefix === 'player' ? 'playerHealthFill' : 'enemyHealthFill', actor.hp, actor.maxHP);

    renderTokenGrid(
        `${prefix}Resources`,
        actor.resources,
        item => item.name === 'Iron'
            ? `<div class="token"><strong>${item.name}</strong><div>Common ${item.common}</div><div>Gold ${item.gold}</div><div>Total ${item.total}</div></div>`
            : `<div class="token"><strong>${item.name}</strong><div>Total ${item.total}</div></div>`,
        'No data yet.'
    );

    renderTokenGrid(
        `${prefix}Defenses`,
        actor.defenses,
        item => `<div class="token"><strong>${item.name}</strong><div>Power ${item.power}</div></div>`,
        'No active defenses.'
    );

    renderTokenGrid(
        `${prefix}Skills`,
        actor.availableSkills,
        item => `<div class="token"><strong>${item}</strong></div>`,
        'No data yet.'
    );
}

function buildTurnSummary(turn) {
    if (!turn) {
        return '<div class="summary-card"><h3>Battle Summary</h3><div class="summary-line">Start a game to see structured turn details.</div></div>';
    }

    return `
        <div class="summary-card">
            <h3>Player Action</h3>
            <div class="summary-line">Skill: ${safeText(turn.player.skillName)}</div>
            <div class="summary-line">Param: ${safeText(turn.player.param, 0)}</div>
        </div>
        <div class="summary-card">
            <h3>Enemy Action</h3>
            <div class="summary-line">Skill: ${safeText(turn.enemy.skillName)}</div>
            <div class="summary-line">Param: ${safeText(turn.enemy.param, 0)}</div>
        </div>
    `;
}

function renderHistory() {
    const historyList = document.getElementById('historyList');
    if (!historyList) return;

    if (!State.turns.length) {
        historyList.innerHTML = '<div class="history-empty">No turns yet. Once the battle starts, each round will be archived here.</div>';
        return;
    }

    historyList.innerHTML = State.turns.map((turn, index) => `
        <button class="history-item ${index === State.currentTurn ? 'active' : ''}" data-turn-index="${index}" type="button">
            <div class="history-title">
                <span>Turn ${turn.index}</span>
                <span>${resultLabel(turn.result)}</span>
            </div>
            <div>You: ${turn.player.skillName} ${turn.player.param} | Enemy: ${turn.enemy.skillName} ${turn.enemy.param}</div>
        </button>
    `).join('');

    historyList.querySelectorAll('[data-turn-index]').forEach(button => {
        button.addEventListener('click', () => {
            State.currentTurn = Number(button.getAttribute('data-turn-index'));
            renderTurn();
        });
    });
}

function renderTurn() {
    const turnIndex = document.getElementById('turnIndex');
    const turnCounterPill = document.getElementById('turnCounterPill');
    const actionText = document.getElementById('actionText');
    const turnSummary = document.getElementById('turnSummary');
    const resultBadge = document.getElementById('resultBadge');
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');

    const turn = State.currentTurn >= 0 && State.currentTurn < State.turns.length
        ? State.turns[State.currentTurn]
        : null;

    if (turnIndex) turnIndex.textContent = turn ? `Turn ${turn.index} / ${State.turns.length}` : `Turn 0 / ${State.turns.length}`;
    if (turnCounterPill) turnCounterPill.textContent = turn ? `Turn ${turn.index}` : `Turn ${State.turns.length}`;
    if (actionText) {
        actionText.textContent = turn
            ? `You used ${turn.player.skillName} ${turn.player.param}. Enemy used ${turn.enemy.skillName} ${turn.enemy.param}.`
            : (State.gameStarted ? 'Battle initialized. Declare the first turn.' : 'No actions recorded yet.');
    }
    if (turnSummary) turnSummary.innerHTML = buildTurnSummary(turn);
    if (resultBadge) resultBadge.textContent = State.lastResult;
    if (prevBtn) prevBtn.disabled = State.currentTurn <= 0;
    if (nextBtn) nextBtn.disabled = State.currentTurn < 0 || State.currentTurn >= State.turns.length - 1;

    renderHistory();
}

function updateHeroVisibility() {
    const heroPanel = document.getElementById('heroPanel');
    if (!heroPanel) return;

    heroPanel.open = !State.heroCollapsed;
}

function renderSnapshot(snapshot, options = {}) {
    const autoFocusLatest = Boolean(options.autoFocusLatest);
    State.snapshot = snapshot;
    State.turns = snapshot?.turns || [];
    State.gameStarted = Boolean(snapshot?.started);
    State.isManual = Boolean(snapshot?.manualMode);
    State.selectedModeName = safeText(snapshot?.modeName, 'Not started');
    State.lastResult = resultLabel(snapshot?.result);
    State.heroCollapsed = State.gameStarted && !['Victory', 'Defeat', 'Draw'].includes(State.lastResult);

    if (State.turns.length === 0) {
        State.currentTurn = -1;
    } else if (autoFocusLatest || State.currentTurn < 0 || State.currentTurn >= State.turns.length) {
        State.currentTurn = State.turns.length - 1;
    }

    const modeBadge = document.getElementById('modeBadge');
    if (modeBadge) modeBadge.textContent = State.selectedModeName;

    renderActor('player', snapshot?.player || null);
    renderActor('enemy', snapshot?.enemy || null);
    renderTurn();
    updateEnemyInputVisibility();
    updateHeroVisibility();
}

function updateEnemyInputVisibility() {
    const enemyField = document.getElementById('enemyField');
    const modeHint = document.getElementById('modeHint');
    const actionHint = document.getElementById('actionHint');

    if (enemyField) enemyField.classList.toggle('is-hidden', !State.isManual);
    if (modeHint) {
        modeHint.textContent = State.isManual
            ? 'Manual mode lets you enter both sides of the turn.'
            : 'AI mode hides enemy input and lets the selected strategy play automatically.';
    }
    if (actionHint) {
        actionHint.textContent = State.gameStarted
            ? (State.isManual ? 'Enter your action and the enemy action for this round.' : 'Enter your action. The enemy move will be chosen by AI.')
            : 'Start a game to enable turn declaration.';
    }
}

function updateBusyState() {
    const startBtn = document.getElementById('startBtn');
    const restartBtn = document.getElementById('restartBtn');
    const declareBtn = document.getElementById('declareBtn');
    const connectionState = document.getElementById('connectionState');

    if (startBtn) startBtn.disabled = State.busy;
    if (restartBtn) restartBtn.disabled = State.busy || !State.gameStarted;
    if (declareBtn) declareBtn.disabled = State.busy || !State.gameStarted;
    if (connectionState) connectionState.textContent = State.busy ? 'Working' : 'Ready';
}
