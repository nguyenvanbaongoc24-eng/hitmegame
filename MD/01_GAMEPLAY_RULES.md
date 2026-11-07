# HIT ME — gameplay rules v1.3

This file records the Stage 1 rule set. Core is authoritative for resolution.

- Each player starts with 3 HP. Placement lasts 5 seconds. Each player chooses a position and aim; when time expires, throws resolve simultaneously. A throw stops at the first player hit and deals 1 damage. Without aim, `ThrowFacing` applies. Other players' positions and aims remain hidden until reveal.
- A normal match ends when one player remains alive. There is no five-round end. At the start of normal round 10, every surviving player with more than 1 HP is reduced to 1 HP. This only applies once.
- If every player who was alive at the start of a round dies in that round, only those finalists enter a tiebreak. They are revived at 1 HP, placed again and given 5 seconds to aim. At most three tiebreak rounds occur. If all finalists die together in the third tiebreak, all are co-winners.
- Normal round 25 is a hard cap. Rank by HP, then successful hits. If leaders remain tied, only those leaders enter the same three-round tiebreak. If no tiebreak is configured, tied leaders co-win.
- Players eliminated in the same round share a competition rank: 1, 2, 3, 3, 5, 6. The final record includes throws, hits, accuracy, eliminated round and rank. Co-winners share rank 1.
- Existing AFK and reconnect rules remain unchanged. Online room behavior and rewards are separate from Stage 1.

`GameConfig`: `StartHp=3`, `PlaceSeconds=5`, `MaxRounds=0`, `SuddenDeathRound=10`, `HardCapRound=25`, `TiebreakMax=3`, `TiebreakHp=1`, `MaxHitsPerThrow=1`, `NoAimPolicy=ThrowFacing`.
