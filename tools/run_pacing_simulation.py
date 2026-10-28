import math
import random
import json
import os
from collections import Counter

class Vec2:
    def __init__(self, x, y): self.x, self.y = float(x), float(y)
    def __add__(self, o): return Vec2(self.x + o.x, self.y + o.y)
    def __sub__(self, o): return Vec2(self.x - o.x, self.y - o.y)
    def __mul__(self, s): return Vec2(self.x * s, self.y * s)
    def __truediv__(self, s): return Vec2(self.x / s, self.y / s)
    def dot(self, o): return self.x * o.x + self.y * o.y
    @property
    def length(self): return math.hypot(self.x, self.y)
    @property
    def normalized(self):
        l = self.length
        return Vec2(self.x / l, self.y / l) if l > 1e-5 else Vec2(0, 1)

def calc_hits(shooter_pos, targets_pos, direction, w, h, radius):
    dir = direction.normalized
    min_x, max_x = -w*0.5 + radius, w*0.5 - radius
    min_y, max_y = -h*0.5 + radius, h*0.5 - radius
    tx = (max_x - shooter_pos.x)/dir.x if dir.x > 1e-5 else ((min_x - shooter_pos.x)/dir.x if dir.x < -1e-5 else 1e9)
    ty = (max_y - shooter_pos.y)/dir.y if dir.y > 1e-5 else ((min_y - shooter_pos.y)/dir.y if dir.y < -1e-5 else 1e9)
    dist = min(tx, ty)
    end = shooter_pos + dir * dist
    seg = end - shooter_pos
    L = seg.length
    if L < 1e-5: return []
    unit = seg / L
    hits = []
    for tid, tpos in targets_pos:
        diff = tpos - shooter_pos
        proj = diff.dot(unit)
        if 0 <= proj <= L:
            closest = shooter_pos + unit * proj
            if (tpos - closest).length <= radius:
                hits.append((proj, tid))
    hits.sort(key=lambda x: x[0])
    return [h[1] for h in hits[:1]]

def run_simulation(matches=1000, max_rounds=None, w=10.0, h=16.0, radius=0.45, bot_mode='mixed'):
    rounds_list = []
    winners = Counter()
    per_round_hits_samples = []

    for m in range(matches):
        rng = random.Random(10000 + m)
        hp = {f'p{i}': 3 for i in range(6)}
        hits_count = {f'p{i}': 0 for i in range(6)}
        obs = {f'p{i}': [] for i in range(6)}
        r = 0
        while True:
            alive = [p for p in hp if hp[p] > 0]
            if len(alive) <= 1:
                break
            if max_rounds and r >= max_rounds:
                break
            r += 1
            # placements
            placements = {}
            for p in alive:
                px = rng.uniform(-w*0.5 + radius, w*0.5 - radius)
                py = rng.uniform(-h*0.5 + radius, h*0.5 - radius)
                placements[p] = Vec2(px, py)
                obs[p].append(placements[p])
            # aims
            aims = {}
            for i, p in enumerate(alive):
                pos = placements[p]
                if bot_mode == 'mixed':
                    mode = i % 3
                elif bot_mode == 'center':
                    mode = 1
                elif bot_mode == 'predict':
                    mode = 2
                else:
                    mode = 0
                
                if mode == 1:
                    target = Vec2(0, 0)
                elif mode == 2:
                    other_alive = [o for o in alive if o != p]
                    opp = other_alive[i % len(other_alive)] if other_alive else None
                    if opp and len(obs[opp]) >= 2:
                        target = obs[opp][-1] + (obs[opp][-1] - obs[opp][-2])
                    elif opp and len(obs[opp]) >= 1:
                        target = obs[opp][-1]
                    else:
                        target = Vec2(0, 0)
                else:
                    target = Vec2(0, 0)
                aims[p] = (target - pos).normalized
            # resolve
            round_hit_total = 0
            damage = {p: 0 for p in alive}
            for p in alive:
                targets = [(o, placements[o]) for o in alive if o != p]
                hit_targets = calc_hits(placements[p], targets, aims[p], w, h, radius)
                for ht in hit_targets:
                    damage[ht] += 1
                    hits_count[p] += 1
                    round_hit_total += 1
            per_round_hits_samples.append((len(alive), round_hit_total))
            for p in alive:
                hp[p] = max(0, hp[p] - damage[p])
        
        rounds_list.append(r)
        alive = [p for p in hp if hp[p] > 0]
        if len(alive) == 1:
            winners[alive[0]] += 1
        elif len(alive) == 0:
            winners['draw_all_dead'] += 1
        else:
            sorted_p = sorted(alive, key=lambda x: (hp[x], hits_count[x]), reverse=True)
            if hp[sorted_p[0]] > hp[sorted_p[1]] or hits_count[sorted_p[0]] > hits_count[sorted_p[1]]:
                winners[sorted_p[0]] += 1
            else:
                winners['tie_hp_hits'] += 1

    avg_r = sum(rounds_list)/len(rounds_list)
    max_r = max(rounds_list)
    min_r = min(rounds_list)
    tie_count = winners['draw_all_dead'] + winners['tie_hp_hits']
    
    # Calculate hit probability by alive count
    alive_hits = {}
    for alive_cnt, h_cnt in per_round_hits_samples:
        if alive_cnt not in alive_hits: alive_hits[alive_cnt] = []
        alive_hits[alive_cnt].append(h_cnt / alive_cnt) # hits per throw
    prob_by_alive = {k: round(sum(v)/len(v)*100, 2) for k, v in sorted(alive_hits.items())}

    return {
        'avg_rounds': round(avg_r, 2),
        'min_rounds': min_r,
        'max_rounds': max_r,
        'tie_percentage': round(tie_count / matches * 100, 2),
        'prob_by_alive': prob_by_alive
    }

variants = [
    ('Baseline (Arena 10x16, R=0.45, No Round Limit)', None, 10.0, 16.0, 0.45, 'mixed'),
    ('Option 1A: maxRounds = 5 (HP tiebreaker)', 5, 10.0, 16.0, 0.45, 'mixed'),
    ('Option 1B: maxRounds = 8 (HP tiebreaker)', 8, 10.0, 16.0, 0.45, 'mixed'),
    ('Option 1C: maxRounds = 10 (HP tiebreaker)', 10, 10.0, 16.0, 0.45, 'mixed'),
    ('Option 2A: Arena thu nho 25% (7.5x12), No Round Limit', None, 7.5, 12.0, 0.45, 'mixed'),
    ('Option 2B: PlayerRadius tang len 0.60, No Round Limit', None, 10.0, 16.0, 0.60, 'mixed'),
    ('Option 3: Bot AI thong minh (Predictor+Hunter)', None, 10.0, 16.0, 0.45, 'predict'),
    ('Option 4: Arena 8x12.8 (giam 20%) + maxRounds = 5', 5, 8.0, 12.8, 0.45, 'mixed'),
    ('Option 5: Arena 8x12.8 (giam 20%) + maxRounds = 8', 8, 8.0, 12.8, 0.45, 'mixed'),
]

results = {}
for name, mr, w, h, rad, bm in variants:
    res = run_simulation(matches=1000, max_rounds=mr, w=w, h=h, radius=rad, bot_mode=bm)
    results[name] = res
    print(f"DONE: {name} => Avg: {res['avg_rounds']}, Max: {res['max_rounds']}, Tie: {res['tie_percentage']}%")

out_file = r"d:\HITME GAME\MD\pacing_sim_results.json"
with open(out_file, "w", encoding="utf-8") as f:
    json.dump(results, f, indent=2)
print("Saved simulation data to", out_file)
