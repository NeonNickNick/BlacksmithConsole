function parseSkill(text) {
    const raw = (text || '').trim();
    if (!raw) return { name: 'iron', param: 0 };

    const parts = raw.split(/\s+/, 2);
    const name = parts[0] || 'iron';
    const parsed = Number.parseInt(parts[1], 10);

    return { name, param: Number.isFinite(parsed) ? parsed : 0 };
}

async function withBusy(task) {
    if (State.busy) return;

    State.busy = true;
    updateBusyState();

    try {
        await task();
    } catch (error) {
        const message = error instanceof Error ? error.message : 'Unexpected error';
        State.lastResult = message;
        renderTurn();
        alert(message);
    } finally {
        State.busy = false;
        updateBusyState();
    }
}

const startBtn = document.getElementById('startBtn');
const restartBtn = document.getElementById('restartBtn');
const strategy = document.getElementById('strategy');
const skillInput = document.getElementById('skill');
const eskill = document.getElementById('eskill');
const declareBtn = document.getElementById('declareBtn');
const prevBtn = document.getElementById('prevBtn');
const nextBtn = document.getElementById('nextBtn');
const heroPanel = document.getElementById('heroPanel');

async function startOrRestartGame() {
    const mode = Number.parseInt(strategy.value, 10);
    const response = await startGame(mode);
    if (!response.ok) {
        throw new Error(response.message || 'Unable to start game.');
    }
    renderSnapshot(response.snapshot, { autoFocusLatest: true });
}

startBtn?.addEventListener('click', () => withBusy(startOrRestartGame));
restartBtn?.addEventListener('click', () => withBusy(startOrRestartGame));

strategy?.addEventListener('change', () => {
    const selectedOption = strategy.options[strategy.selectedIndex];
    State.selectedModeName = selectedOption ? selectedOption.textContent : 'Not started';
    State.isManual = Number.parseInt(strategy.value, 10) === 1;
    const modeBadge = document.getElementById('modeBadge');
    if (modeBadge) modeBadge.textContent = State.selectedModeName;
    updateEnemyInputVisibility();
});

declareBtn?.addEventListener('click', () => withBusy(async () => {
    const skill = parseSkill(skillInput?.value || '');
    const enemySkill = parseSkill(eskill?.value || '');
    const response = await declareAPI({
        skillName: skill.name,
        param: skill.param,
        esn: enemySkill.name,
        ep: enemySkill.param
    });

    if (!response.ok) {
        renderSnapshot(response.snapshot, { autoFocusLatest: true });
        throw new Error(response.message || 'Turn declaration failed.');
    }

    renderSnapshot(response.snapshot, { autoFocusLatest: true });
}));

prevBtn?.addEventListener('click', () => {
    if (State.currentTurn > 0) {
        State.currentTurn -= 1;
        renderTurn();
    }
});

nextBtn?.addEventListener('click', () => {
    if (State.currentTurn < State.turns.length - 1) {
        State.currentTurn += 1;
        renderTurn();
    }
});

heroPanel?.addEventListener('toggle', () => {
    State.heroCollapsed = !heroPanel.open;
    updateHeroVisibility();
});

(async function init() {
    await withBusy(async () => {
        const list = await loadStrategies();
        strategy.innerHTML = '';
        list.forEach(item => {
            const option = document.createElement('option');
            option.value = item.id;
            option.textContent = item.name;
            strategy.appendChild(option);
        });

        if (strategy.options.length > 0) {
            strategy.selectedIndex = 0;
            State.selectedModeName = strategy.options[0].textContent;
            State.isManual = Number.parseInt(strategy.value, 10) === 1;
        }

        const status = await loadStatus();
        if (status.ok) {
            renderSnapshot(status.snapshot, { autoFocusLatest: true });
        } else {
            updateEnemyInputVisibility();
            renderTurn();
        }
    });
})();
