# HIT ME VN — AG Handoff

Codex owns authoritative match data and gameplay. AG owns presentation assets and UI hierarchy; it must not create a second match timer, HP model, aiming model, projectile spawner, or phase state machine.

Current canonical data sources are `MatchStateMachine`, `AimController`, `ActorView`, `CharacterData`, `WeaponData`, and `UIManager`. A stable presentation adapter and exact event surface will be added before AG integrates runtime UI.

AG-owned delivery paths are reserved under `Assets/HITME/UI`, `Assets/HITME/Art/UI`, and `Assets/HITME/Prefabs/Presentation`. Codex will wire reviewed presentation prefabs to `MainMatch`.
