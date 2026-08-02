# Chronicles of the Lost Dungeon: Vaults of the Elixir

A 2D top-down dungeon crawler built in Unity. The player descends through five guardian-sealed vaults, each holding a fragment of knowledge needed to reach the final vault — home to the Elixir of Life.

---

## Table of Contents
- [Game Overview](#game-overview)
- [Vault / Guardian Lineup](#vault--guardian-lineup)
- [Architecture](#architecture)
- [Design Patterns](#design-patterns)
- [Algorithms](#algorithms)
- [Project Structure](#project-structure)
- [Setup Instructions](#setup-instructions)
- [Controls](#controls)
- [Save Data](#save-data)
- [HUD & UI](#hud--ui) *(new)*
- [Known Issues / To-Do](#known-issues--to-do)
- [Credits & Asset Sources](#credits--asset-sources)

---

## Game Overview

An excavation engineer falls through a collapsed mine shaft into a buried vault complex — five sealed chambers, each guarded by a different era's final line of defense. To escape, she must fight through each guardian, recover the relic they leave behind, and use the knowledge gathered to reach the sixth and final chamber: the source of the Elixir of Life.

Each vault introduces a new enemy archetype, a new combat mechanic, and a distinct visual identity. The final vault is only reachable once relics from all four preceding guardians have been collected — progression is gated by genuine accomplishment, not just linear level completion.

---

## Vault / Guardian Lineup

| Vault | Guardian | Combat Identity | Relic Dropped |
|---|---|---|---|
| 0 | Scorpion | Melee, low HP — tutorial-tier | Stinger Shard |
| 1 | Anaconda | Constrict grapple, damage-over-time | Serpent Coil |
| 2 | Armed Guard | Ranged, pooled projectiles | Access Sigil |
| 3 | Dragon | Ranged AoE fire breath | Ember Core |
| 4 (Final) | 3× Dinosaur (pack) | Multi-enemy concurrent encounter | — (drops the Elixir of Life once all three are defeated) |

Vault 4 unlocks only when Vault 3 is completed and all four relics (Vaults 0–3) have been collected.

---

## Architecture

The game runs on a multi-scene structure: one persistent `HOME` scene plus one scene per vault.

```
HOME (persistent — never unloaded)
 ├── Singletons: GameManager, SaveManager, AudioManager, LevelManager, ObjectPoolManager
 ├── Canvas (all UI panels) + UIManager
 ├── EventSystem
 ├── Player (persists via PersistAcrossScenes)
 └── Main Camera (persists via PersistAcrossScenes, follows Player)

Vault0_Scorpion / Vault1_Anaconda / Vault2_ArmedGuard / Vault3_Dragon / Vault4_Dinosaurs
 ├── Guardian(s) for that vault
 ├── Relic pickup (or DinosaurPackManager + Elixir pickup, for Vault 4)
 ├── Background art
 └── PlayerSpawnPoint
```

When `GameManager.LoadLevel(index)` is called, it loads the matching vault scene, repositions the persistent Player at that scene's `PlayerSpawnPoint`, and snaps the camera instantly to avoid a visible sweep across the map.

### Core Singletons

| Singleton | Responsibility |
|---|---|
| `GameManager` | Game state (MainMenu, Playing, Paused, GameOver), scene loading, current level index |
| `SaveManager` | JSON read/write via `Application.persistentDataPath` |
| `AudioManager` | SFX/music playback, reacts to gameplay events, owns two `AudioSource`s (Music, SFX) |
| `LevelManager` | Vault lock/unlock/completion status, relic tracking, Elixir gate logic, level display names |
| `ObjectPoolManager` | Generic object pooling for projectiles (guard bullets, dragon fireballs) |

### Core Interfaces

- `IDamageable` — `TakeDamage(int)`, `Die()`
- `IInteractable` — `Interact(GameObject source)`
- `IAbility` — `Activate(GameObject user)`, `Cooldown`
- `IEnemyBehaviour` — `Move()`, `Attack()`, `OnPlayerDetected()`
- `ISaveable` — `CaptureState()`, `RestoreState(data)`
- `ICollectable` — `Collect(GameObject collector)`

### Era-Specific Guardian Interfaces

Each guardian archetype implements the shared `IEnemyBehaviour` plus an additive, era-specific interface — this is what lets a new guardian type be added without touching any existing guardian's code:

- `IVenomousGuardian` (Scorpion)
- `IReptilianGuardian` (Anaconda) — `Constrict()`, `Camouflage()`
- `ITacticalGuardian` (Armed Guard) — `TakeCover()`, `FireShot()`
- `IElementalGuardian` (Dragon) — `BreathAttack()`
- `IPrehistoricGuardian` (Dinosaurs) — `Roar()`, `ChargeAttack()`

### Events (Observer Pattern)

A static `GameEvents` class decouples every system — UI, audio, save, and progression logic all subscribe independently with no direct references to each other:

```csharp
public static class GameEvents {
    public static event Action<int, int> OnHealthChanged;
    public static event Action<GameObject> OnEnemyDefeated;
    public static event Action<int> OnRelicCollected;
    public static event Action<string> OnAbilityActivated;
    public static event Action<int> OnLevelCompleted;
    public static event Action<int> OnLevelUnlocked;
    public static event Action OnElixirFound;
    public static event Action OnPlayerDied;
    public static event Action<GameState> OnGameStateChanged;
}
```

All HUD/audio reactions (health bars, sound cues, win/lose panels) hook into these events rather than being called directly by gameplay code.

---

## Design Patterns

| Pattern | Where Used | Why |
|---|---|---|
| Singleton | `GameManager`, `SaveManager`, `AudioManager`, `LevelManager`, `ObjectPoolManager` | Single global source of truth for state that must persist across scenes |
| Observer | `GameEvents` + all UI/audio/save listeners | Decouples systems — e.g. UI never directly references the player or a guardian |
| Strategy | Player abilities (`IAbility`: `EmberDashAbility`, `VaultPulseAbility`, `GuardianWardAbility`) | Abilities are swappable components rather than hardcoded branches |
| State | `Guardian` base class FSM (Idle → Chase → Attack → Dead), `GameManager` state | Each guardian's behavior is a clean state transition, not a tangle of booleans |
| Object Pooling | `ObjectPoolManager` — guard bullets, dragon fireballs | Avoids per-shot instantiate/destroy overhead |

---

## Algorithms

- **Gameplay Logic — Difficulty Scaling:** `DifficultyCurve.cs` scales guardian damage and health per vault index (1.00×–1.75× damage, 1.00×–1.60× health across Vaults 0–4), giving every guardian's stats a single tunable source of truth. *O(1) per guardian.*
- **Sorting — Inventory:** `InventoryManager.GetSortedInventory()` sorts collected relics/items by vault order for display. *O(n log n) via `List.Sort`.*
- **Searching — Combat Detection:** guardian detection uses a trigger-collider overlap check (`OnTriggerEnter2D` / `Physics2D.OverlapCircleAll`) to find valid targets within range — used both for guardian aggro and melee/ranged attack hit detection. *O(n) over colliders in range.*

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/        — Singletons, GameEvents, SaveData, GuardianRelic, VaultGate,
│   │                  DifficultyCurve, CameraFollow, PersistAcrossScenes
│   ├── Interfaces/  — All shared + era-specific interfaces
│   ├── Enemies/     — Guardian base class + 5 concrete guardians + DinosaurPackManager
│   ├── Player/      — PlayerController, PlayerAttack, PlayerShoot, VirtualJoystick, ability implementations
│   └── UI/          — UIManager, MainMenuUI, LevelSelectUI, VaultButtonUI, WorldHealthBar,
│                      ObjectivesPanel, GameOverPanel, GameWinPanel, HUDUI, etc.
├── Animations/       — Per-guardian Animator Controllers
├── Prefabs/          — Guardian, relic, and projectile prefabs
└── Scenes/           — HOME + Vault0–Vault4
```

---

## Setup Instructions

1. Open the project in Unity (6000.4.5f1 confirmed working).
2. Ensure **Active Input Handling** is set to `Both` (Edit → Project Settings → Player → Other Settings) — the codebase uses the legacy `Input` class.
3. Open `HOME.unity` — this is the required entry scene. Do not press Play from a vault scene directly; the camera and singletons only exist in `HOME`.
4. Confirm all six scenes are registered in File → Build Settings, with `HOME` at index 0.
5. Press Play from `HOME` to test.
6. **For Android builds:** File → Build Settings → Android → Switch Platform. Set target Orientation to Landscape. Minimum API Level 7+ recommended. Touch controls (joystick + Jump/Attack/Shoot buttons) work automatically via Unity's UI EventSystem — no platform-specific code needed.

---

## Controls

| Action | Key / Input |
|---|---|
| Move | WASD / Arrow Keys (or on-screen joystick, touch/WebGL builds) |
| Jump | Space / W (or on-screen Jump button) |
| Attack | F (or on-screen Attack button) |
| Shoot | Mouse0 (or on-screen Shoot button) |
| Interact | E |
| Ability Slot 1 / 2 | Q / R |
| Pause | Esc |

---

## Save Data

Progress is saved as JSON via `SaveManager` to `Application.persistentDataPath`, and includes:

```csharp
[Serializable]
public class SaveData {
    public int[] levelStatus;       // 0 = locked, 1 = available, 2 = completed
    public int playerHealth, playerMaxHealth;
    public List<string> relicsCollected;
    public float musicVolume, sfxVolume;
    public bool musicMuted;
}
```

Progress persists across sessions — closing and reopening the game and selecting Continue returns the player to Level Select with all unlocked/completed vault states intact.

---

## HUD & UI

Previously listed as not-yet-built — now implemented:

- **World-space health bars** (`WorldHealthBar.cs`) — a small filled-image bar above the Player and every Guardian's head, billboarded to face the camera. Player's bar auto-subscribes to `GameEvents.OnHealthChanged`; each Guardian's bar is updated via a direct call from `Guardian.TakeDamage()`. Set up once per prefab, inherited across all scenes automatically.
- **Objectives panel** (`ObjectivesPanel.cs`) — shown on game start, lists overall objectives (not per-level), freezes `Time.timeScale` until dismissed via a Begin button.
- **Level Select** (`LevelSelectUI.cs` / `VaultButtonUI.cs`) — dynamically spawns one button per vault, name label + status sprite (locked/available/completed — status communicated via sprite/tint only, per rubric), refreshes live on `OnLevelUnlocked`/`OnLevelCompleted`.
- **Settings panel** — mute Toggle + Music/SFX Sliders wired directly to `AudioManager.SetMusicMuted()` / `SetMusicVolume()` / `SetSfxVolume()`.
- **Game Over panel** (`GameOverPanel.cs`) — shown on `OnPlayerDied`, auto-reloads the current scene after a short delay.
- **Game Win panel** (`GameWinPanel.cs`) — shown on `OnElixirFound`, Main Menu button returns to `HOME`.

---

## Known Issues / To-Do

- [ ] Replace placeholder/temporary sprite art with final, copyright-cleared assets
- [x] ~~HUD content (health bar, ability cooldown icons, objective text) not yet built~~ — health bars, objectives panel, and win/lose panels now implemented. Ability cooldown icons still pending.
- [x] ~~Pause / Game Over / Level Complete / Elixir Win screens need visible content~~ — Game Over and Win panels now implemented. Pause and Level Complete screens still need content.
- [ ] REST API leaderboard (jsonbin.io) not yet implemented
- [ ] Unit tests (8+ NUnit tests) not yet written
- [ ] Conditional compilation for WebGL/PC/Mobile input not yet added beyond one editor-only example (`#if UNITY_EDITOR` gizmo drawing in `PlayerController`)
- [ ] Platform builds (PC / WebGL / Mobile) not yet produced
- [ ] **New:** Guardian `Attack Range` / `Detection Range` values need calibration for this project's actual world scale — currently mid-tuning, with a temporary debug log in `Guardian.TickAttack()` still in place pending final numbers
- [ ] **New:** `PlayerShoot.FirePoint` needs to be set per-scene (only configured in gameplay scenes so far, not relevant in `HOME`)

---

## Credits & Asset Sources

List final sourced asset packs - https://assetstore.unity.com/
Attributions here before submission - https://claude.ai/
