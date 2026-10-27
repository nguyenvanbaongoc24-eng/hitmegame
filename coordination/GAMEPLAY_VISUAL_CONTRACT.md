# HIT ME VN - Gameplay / visual contract

## Authority

`MatchStateMachine` is the sole authoritative match phase, timer, HP, lock, reveal, and result source. `AimController` is authoritative for aim direction. `ProjectileView` and `MatchStateMachine` own projectile lifetime/spawn. AG presentation never instantiates a projectile.

## Read adapter

`HitMe.Presentation.MatchPresentationAdapter` is attached to the existing `MatchStateMachine`. It publishes a read-only `MatchPresentationState` through `StateChanged`:

- `characterId`, `characterName`, `weaponId`, `weaponName`
- `round`, `phase`, `phaseSecondsRemaining`
- `playerHp`, `playerMaxHp`, `opponentHp`, `opponentMaxHp`
- `aimDirection`, `isPlayerActionLocked`

AG may subscribe and render this state. It must not write it back or maintain parallel countdown/HP/phase data.

## Event mapping

| Contract event | Authoritative sender | Visual responsibility |
|---|---|---|
| `PlanningStarted` | `MatchStateMachine` enters planning | show planning HUD |
| `AimChanged` | `AimController`; adapter state poll | draw aim ruler from exact vector |
| `CommandLocked` | `MatchStateMachine` after valid release | lock local UI immediately |
| `RevealStarted` | `MatchStateMachine` | show reveal transition only |
| `ThrowReleased` | **gameplay only** during reveal | play throw presentation; do not spawn projectile |
| `DamageApplied` | gameplay collision resolution | animate HP from authoritative state |
| `MatchEnded` | `MatchStateMachine` | open result presentation |

The existing project does not yet expose a C# event for every named phase. Until Codex adds explicit events, AG subscribes to `StateChanged` and detects phase edges locally for presentation only. It must never infer or advance gameplay state.

## Coordinate rule

Aim ruler and projectile use the same `Vector2 aimDirection` from `AimController`. Sprite visual rotation offsets belong in presentation only and must not alter the movement/flight vector.
