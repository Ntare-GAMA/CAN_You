# Chronicles of the Lost Dungeon — Vaults of the Elixir

A 2D top-down dungeon game built in Unity. Five vaults, five distinct guardians, one
Elixir of Life waiting at the end for whoever survives them all.

## Story

An excavation engineer falls through a collapsed shaft into a buried vault complex.
Each chamber is sealed by a guardian from a different era, and each guardian's
defeat yields a relic. Collect all four relics from Vaults 0–3 to unlock Vault 4 —
defeat the three Dinosaurs waiting there, and the Elixir of Life is yours.

## Guardian roster

| Vault | Guardian | Combat identity | Relic dropped |
|---|---|---|---|
| 0 | Scorpion | Melee sting, tutorial-tier | Stinger Shard |
| 1 | Anaconda | Grapple / damage-over-time constrict | Serpent Coil |
| 2 | Armed Guard | Ranged pooled-projectile fire, calls nearby guards | Access Sigil |
| 3 | Dragon | Ranged AoE fire-breath | Ember Core |
| 4 (final) | 3× Dinosaur | Bite / telegraphed roar-charge, fought concurrently | — (drops the Elixir of Life once all three are defeated) |

Vault 4 only unlocks once Vault 3 is completed **and** all four relics from
Vaults 0–3 have been collected — clearing the guardian alone isn't enough, so
progression rewards full exploration, not just combat.

## Architecture

### Design patterns

| Pattern | Where |
|---|---|
| **Singleton** | `GameManager`, `SaveManager`, `AudioManager`, `LevelManager`, `ObjectPoolManager` |
| **Observer** | `GameEvents` (static C# events) — UI, audio, and save systems all react independently; none reference each other directly |
| **Strategy** | Player abilities — `IAbility` implementations (`EmberDashAbility`, `VaultPulseAbility`, `GuardianWardAbility`) swapped into ability slots at runtime |
| **State** | Guardian FSM (`Idle → Chase → Attack → Dead`) in the shared `Guardian` base class; also `GameManager`'s `GameState` |
| **Object Pooling** | `ObjectPoolManager` — reused projectile instances (`GuardBullet`, `DragonFireball`) instead of instantiate/destroy per shot |

### Core interfaces

- `IDamageable` — `TakeDamage(int)`, `Die()`
- `IInteractable` — `Interact(GameObject source)`
- `IAbility` — `Activate(GameObject user)`, `Cooldown`
- `IEnemyBehaviour` — `Move()`, `Attack()`, `OnPlayerDetected()`
- `ISaveable` — `CaptureState()`, `RestoreState(data)`
- `ICollectable` — `Collect(GameObject collector)`

### Era-specific guardian interfaces

Each guardian implements the shared `IEnemyBehaviour` plus one interface unique
to its era — this is the concrete answer to "add a new enemy type without
touching existing systems": `Guardian` (the shared base) only knows about
`IEnemyBehaviour`; nothing else in the codebase needs to change to add a sixth.

- `IVenomousGuardian` — Scorpion
- `IReptilianGuardian` — Anaconda (`Constrict()`, `Camouflage()`)
- `ITacticalGuardian` — Armed Guard (`CallSquad()`, `TakeCover()`)
- `IElementalGuardian` — Dragon (`BreathAttack()`, `TakeFlight()`)
- `IPrehistoricGuardian` — Dinosaur (`Roar()`, `ChargeAttack()`)

### Event hub (`GameEvents`)

```csharp
public static class GameEvents {
    public static event Action<int> OnHealthChanged;
    public static event Action<GameObject> OnEnemyDefeated;
    public static event Action<string> OnItemCollected;
    public static event Action<int> OnRelicCollected;
    public static event Action<int> OnLevelCompleted;
    public static event Action OnElixirFound;
    public static event Action<GameState> OnGameStateChanged;
}
```
UI, audio, and save systems all subscribe independently — combat code never
calls into UI or audio directly.

## Scene structure

The project uses a **persistent hub + per-vault scenes** approach rather than
one giant scene or fully separate standalone levels:

- **HOME** — always loaded first. Contains all 5 singletons, the Canvas + all
  7 UI panels, `EventSystem`, and the `Player`/`Main Camera` (both marked
  `PersistAcrossScenes` so they survive every scene swap).
- **Vault0_Scorpion, Vault1_Anaconda, Vault2_ArmedGuard, Vault3_Dragon,
  Vault4_Dinosaurs** — each contains only that vault's guardian(s), relic,
  background art, and a `PlayerSpawnPoint`. `GameManager.LoadLevel()` swaps
  the active vault scene and repositions the persistent Player at that
  scene's spawn point; `CameraFollow` snaps instantly to avoid a visible pan
  across the map on transition.

## Algorithms (3 required)

1. **Gameplay logic — `DifficultyCurve`**: static class scaling guardian
   damage and health by vault index (1.00×–1.75× damage, 1.00×–1.60× health
   across Vaults 0–4), so every guardian's stats run through one tunable
   curve rather than being hardcoded per guardian.
2. **Sorting — `InventoryManager.GetSortedInventory()`**: sorts collected
   relics/items for display, relics first and in vault order.
3. **Searching — combat/ability range checks**: `Physics2D.OverlapCircleAll`
   based nearest-target detection, used by `VaultPulseAbility` (AoE hit
   detection) and each guardian's detection-radius trigger collider.

## Save system

`SaveManager` reads/writes a JSON `SaveData` object to
`Application.persistentDataPath`, covering level status (locked / available
/ completed) per vault, collected relics, and player health — so progress
persists across sessions. `MainMenuUI.OnContinueClicked()` opens Level
Select rather than resuming directly into a scene, since resuming needs the
player to pick which unlocked vault to enter.

## Audio

`AudioManager` (Observer pattern) owns a music `AudioSource` and an SFX
`AudioSource`, and reacts to `GameEvents` for shared sounds (enemy defeated,
relic collected, level complete, Elixir found, player damaged) without
combat code calling it directly.

- **Music**: each vault scene has a `VaultAmbience` component that calls
  `AudioManager.PlayMusic()` with that scene's own clip on `Start()`.
- **Guardian SFX**: each guardian script has its own `AudioClip` field(s)
  (e.g. `attackSound`, and Dinosaur's `biteSound` / `roarSound` /
  `chargeSound`), played via `AudioManager.PlaySFX()` at the exact moment
  that attack fires — so every guardian sounds distinct even though they
  share the same `AudioManager`.

## Setup

1. Open the project in Unity (2D template).
2. In **Edit → Project Settings → Player → Other Settings → Active Input
   Handling**, set to **Both** (the project uses the legacy `Input` class).
3. In **Edit → Project Settings → Script Execution Order**, add
   `GameManager`, `SaveManager`, `AudioManager`, `LevelManager`,
   `ObjectPoolManager`, all set to a negative order (e.g. -2000 to -1600) so
   they initialize before any UI script asks for their `Instance`.
4. In **File → Build Settings**, confirm all 6 scenes are listed with
   **HOME at index 0**.
5. Always press Play from the **HOME** scene — vault scenes have no camera
   of their own and rely on the persistent one from HOME.

## Controls

| Action | Key |
|---|---|
| Move | WASD / Arrow keys |
| Attack | Space |
| Ability 1 / 2 | (bound per `PlayerController` ability slots) |
| Pause | Esc |

## Known limitations / next steps

- Guardian and player sprites are currently placeholders pending final,
  copyright-safe 2D art.
- REST API leaderboard, NUnit unit test suite, and conditional-compilation
  input handling for WebGL/Mobile are in progress — see project board for
  current status.
- Multiplayer was considered but deliberately deprioritized given the
  solo/timeline constraints; the `IAbility` Strategy pattern and the single
  `PlayerController.TryActivateAbility()` call site were kept intentionally
  centralized so a future networking pass has one place to wrap in
  ownership/RPC checks rather than many.
