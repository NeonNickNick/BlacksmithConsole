// types.js — Lightweight runtime contract for API payloads.
// Defines expected shapes and validates at the boundary so
// format drift between frontend and backend is caught early.

const Types = (() => {
    const is = {
        string: v => typeof v === 'string',
        number: v => typeof v === 'number' && Number.isFinite(v),
        boolean: v => typeof v === 'boolean',
        array: v => Array.isArray(v),
        object: v => v !== null && typeof v === 'object' && !Array.isArray(v),
        maybe: check => v => v == null || check(v),
    };

    // --- Shape definitions (add fields freely — only listed keys are checked) ---

    const ActorShape = {
        professions: is.array,
        hp: is.number,
        maxHP: is.number,
        defenses: is.array,
        resources: is.array,
        futureAttacks: is.array,
        futureDefenses: is.array,
        availableSkills: is.array,
    };

    const SnapshotShape = {
        player: is.maybe(is.object),
        enemy: is.maybe(is.object),
        turns: is.array,
        started: is.boolean,
        manualMode: is.boolean,
        modeName: is.string,
        result: is.string,
    };

    // --- Validate a value against a shape ---

    function checkShape(value, shape, path = '') {
        if (value == null) return null; // null/undefined tolerated for root

        const errors = [];
        for (const [key, test] of Object.entries(shape)) {
            const fullPath = path ? `${path}.${key}` : key;
            if (!(key in value)) {
                errors.push(`${fullPath}: missing`);
                continue;
            }
            if (!test(value[key])) {
                const actual = typeof value[key];
                errors.push(`${fullPath}: expected compatible type, got ${actual}`);
            }
        }
        return errors.length > 0 ? errors : null;
    }

    function validateSnapshot(snapshot) {
        const errors = checkShape(snapshot, SnapshotShape);
        if (errors) {
            console.warn('[Types] Snapshot shape mismatch:', errors, snapshot);
        }
        if (snapshot && snapshot.player) {
            const actorErrors = checkShape(snapshot.player, ActorShape, 'player');
            if (actorErrors) console.warn('[Types] Player shape mismatch:', actorErrors, snapshot.player);
        }
        if (snapshot && snapshot.enemy) {
            const actorErrors = checkShape(snapshot.enemy, ActorShape, 'enemy');
            if (actorErrors) console.warn('[Types] Enemy shape mismatch:', actorErrors, snapshot.enemy);
        }
    }

    return { validateSnapshot };
})();
