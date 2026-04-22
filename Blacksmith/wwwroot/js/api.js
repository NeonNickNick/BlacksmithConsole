async function readJson(response) {
    const data = await response.json().catch(() => null);
    if (!response.ok) {
        throw new Error(data?.message || `Request failed: ${response.status}`);
    }
    return data;
}

async function loadStrategies() {
    return await readJson(await fetch('/api/strategies'));
}

async function startGame(mode) {
    return await readJson(await fetch('/api/start', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mode })
    }));
}

async function declareAPI(payload) {
    return await readJson(await fetch('/api/declare', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    }));
}

async function loadStatus() {
    return await readJson(await fetch('/api/status'));
}
