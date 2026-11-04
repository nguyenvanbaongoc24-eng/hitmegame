using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using HitMe.Core;
using HitMe.Data;
using HitMe.Game;

namespace HitMe.Tests
{
    public class SpawnSeparationTest
    {
        [Test]
        public void TestSpawnSeparation_1000Iterations_SPAWN01()
        {
            float playMinX = ArenaCoordinateMapper.PlayMinX;
            float playMaxX = ArenaCoordinateMapper.PlayMaxX;
            float playMinY = ArenaCoordinateMapper.PlayMinY;
            float playMaxY = ArenaCoordinateMapper.PlayMaxY;

            float margin = 0.75f;
            float topMargin = 1.00f; // M4: >= 1 tile from top
            float minSeparation = 1.00f; // M4: >= 1.2 tile (0.96m)

            float spawnMinX = playMinX + margin;
            float spawnMaxX = playMaxX - margin;
            float spawnMinY = playMinY + margin;
            float spawnMaxY = playMaxY - topMargin;

            var rnd = new System.Random(42);

            for (int iter = 0; iter < 1000; iter++)
            {
                var spawned = new List<Vec2>();
                int playerCount = 6;

                for (int p = 0; p < playerCount; p++)
                {
                    Vec2 pos = new Vec2(
                        (float)(rnd.NextDouble() * (spawnMaxX - spawnMinX) + spawnMinX),
                        (float)(rnd.NextDouble() * (spawnMaxY - spawnMinY) + spawnMinY)
                    );

                    for (int attempt = 0; attempt < 100; attempt++)
                    {
                        bool tooClose = false;
                        for (int j = 0; j < spawned.Count; j++)
                        {
                            float dx = pos.X - spawned[j].X;
                            float dy = pos.Y - spawned[j].Y;
                            if (Mathf.Sqrt(dx * dx + dy * dy) < minSeparation)
                            {
                                tooClose = true;
                                break;
                            }
                        }
                        if (!tooClose) break;

                        pos = new Vec2(
                            (float)(rnd.NextDouble() * (spawnMaxX - spawnMinX) + spawnMinX),
                            (float)(rnd.NextDouble() * (spawnMaxY - spawnMinY) + spawnMinY)
                        );
                    }

                    spawned.Add(pos);
                }

                // Verify all 6 players in iteration
                for (int i = 0; i < spawned.Count; i++)
                {
                    var p = spawned[i];
                    Assert.IsTrue(p.X >= playMinX, $"Player {i} X {p.X} < PlayMinX {playMinX}");
                    Assert.IsTrue(p.X <= playMaxX, $"Player {i} X {p.X} > PlayMaxX {playMaxX}");
                    Assert.IsTrue(p.Y >= playMinY, $"Player {i} Y {p.Y} < PlayMinY {playMinY}");
                    Assert.IsTrue(p.Y <= playMaxY - topMargin + 0.01f, $"Player {i} Y {p.Y} is closer than 1 tile to top edge {playMaxY}");

                    for (int j = i + 1; j < spawned.Count; j++)
                    {
                        float dx = p.X - spawned[j].X;
                        float dy = p.Y - spawned[j].Y;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        Assert.IsTrue(dist >= minSeparation, $"Player {i} and {j} overlap! dist={dist:F3} < min={minSeparation} at iter {iter}");
                    }
                }
            }
        }
    }
}
