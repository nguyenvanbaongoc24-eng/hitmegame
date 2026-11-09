using System;
using System.Collections.Generic;
using HitMe.NetClientV1;
using NUnit.Framework;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#else
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using StringAssert = NUnit.Framework.Legacy.StringAssert;
#endif

namespace HitMe.Tests
{
    [TestFixture]
    public sealed class NetClientV1LoopbackTests
    {
        [Test]
        public void NetConfig_DefaultsToDisabled()
        {
            Assert.IsFalse(NetConfig.Enabled, "NetConfig.Enabled must default to false so offline bot practice is unaffected.");
        }

        [Test]
        public void Loopback_OnePlayerStart_ReturnsNotEnoughPlayers()
        {
            var codec = UnityJsonProtocol.Instance;
            int joinedCount = 0;

            Func<string, IReadOnlyList<string>> serverLogic = payload =>
            {
                var envelope = codec.FromJson<Envelope>(payload);
                if (envelope.type == "join")
                {
                    joinedCount++;
                    var roomMsg = new RoomStateMessage
                    {
                        v = 1,
                        type = "room_state",
                        protocolVersion = 1,
                        roomCode = "HM1111",
                        started = false,
                        players = new[] { new RoomPlayerDto { id = "p1", name = "Player 1", ready = true } },
                        config = new RoomConfigDto { maxPlayers = 6, minPlayersToStart = 2 }
                    };
                    return new[] { codec.ToJson(roomMsg) };
                }
                if (envelope.type == "start")
                {
                    if (joinedCount < 2)
                    {
                        var errMsg = new ErrorMessage
                        {
                            v = 1,
                            type = "error",
                            code = "not_enough_players"
                        };
                        return new[] { codec.ToJson(errMsg) };
                    }
                }
                return Array.Empty<string>();
            };

            using var transport = new LoopbackTransport(codec, serverLogic);
            using var session = new ClientMatchSession(transport);

            ErrorMessage receivedError = null;
            session.MessageReceived += msg =>
            {
                if (msg is ErrorMessage err) receivedError = err;
            };

            session.Connect();
            session.Send(new JoinMessage { v = 1, type = "join", token = "guest:p1", roomCode = "HM1111" });
            Assert.IsTrue(session.WaitingForPlayers, "With N=1, session should report waiting for players.");

            session.Send(new StartMessage { v = 1, type = "start" });
            Assert.IsNotNull(receivedError, "Expected ErrorMessage from server on start with N=1.");
            Assert.AreEqual("not_enough_players", receivedError.code);
        }

        [Test]
        public void Loopback_TwoPlayers_JoinAndStart_TransitionsToPlacing()
        {
            var codec = UnityJsonProtocol.Instance;
            int joinedCount = 0;

            Func<string, IReadOnlyList<string>> serverLogic = payload =>
            {
                var envelope = codec.FromJson<Envelope>(payload);
                if (envelope.type == "join")
                {
                    joinedCount++;
                    var roomMsg = new RoomStateMessage
                    {
                        v = 1,
                        type = "room_state",
                        protocolVersion = 1,
                        roomCode = "HM2222",
                        started = false,
                        players = new[]
                        {
                            new RoomPlayerDto { id = "p1", name = "Player 1", ready = true },
                            new RoomPlayerDto { id = "p2", name = "Player 2", ready = true }
                        },
                        config = new RoomConfigDto { maxPlayers = 6, minPlayersToStart = 2 }
                    };
                    return new[] { codec.ToJson(roomMsg) };
                }
                if (envelope.type == "start" && joinedCount >= 2)
                {
                    var startMsg = new RoundStartMessage
                    {
                        v = 1,
                        type = "round_start",
                        roundId = 1,
                        deadline = 1800000000000L
                    };
                    return new[] { codec.ToJson(startMsg) };
                }
                return Array.Empty<string>();
            };

            using var transport = new LoopbackTransport(codec, serverLogic);
            using var session = new ClientMatchSession(transport);

            var phaseHistory = new List<ClientMatchPhase>();
            session.PhaseChanged += phaseHistory.Add;

            session.Connect();
            session.Send(new JoinMessage { v = 1, type = "join", token = "guest:p1", roomCode = "HM2222" });
            joinedCount = 2; // Simulate second player joining

            session.Send(new StartMessage { v = 1, type = "start" });

            Assert.AreEqual(ClientMatchPhase.Placing, session.Phase);
            Assert.AreEqual(1, session.RoundId);
            Assert.AreEqual(1800000000000L, session.DeadlineUnixMilliseconds);
            Assert.Contains(ClientMatchPhase.Lobby, phaseHistory);
            Assert.Contains(ClientMatchPhase.Placing, phaseHistory);
        }

        [Test]
        public void Loopback_CommitWithAimDir_SerializesHasAimTrueAndValidCoordinates()
        {
            var codec = UnityJsonProtocol.Instance;

            var commit = new CommitMessage
            {
                v = 1,
                type = "commit",
                roundId = 1,
                positionX = 1.25f,
                positionY = -0.75f,
                hasAim = true,
                aimX = 0.50f,
                aimY = 0.866f
            };

            string json = codec.ToJson(commit);

            // JsonUtility verification: must contain hasAim, aimX, aimY and NOT aimDir
            StringAssert.Contains("hasAim", json);
            StringAssert.Contains("aimX", json);
            StringAssert.Contains("aimY", json);
            StringAssert.DoesNotContain("aimDir", json);

            var roundTripped = codec.FromJson<CommitMessage>(json);
            Assert.IsNotNull(roundTripped);
            Assert.IsTrue(roundTripped.hasAim);
            Assert.AreEqual(1.25f, roundTripped.positionX, 0.001f);
            Assert.AreEqual(-0.75f, roundTripped.positionY, 0.001f);
            Assert.AreEqual(0.50f, roundTripped.aimX, 0.001f);
            Assert.AreEqual(0.866f, roundTripped.aimY, 0.001f);

            // Transport loopback roundtrip
            string deliveredPayload = null;
            using var transport = new LoopbackTransport(codec, payload =>
            {
                deliveredPayload = payload;
                var lockAck = new LockReadyMessage { v = 1, type = "lock_ready", ready = true };
                return new[] { codec.ToJson(lockAck) };
            });

            transport.Connect();
            transport.Send(commit);

            Assert.IsNotNull(deliveredPayload);
            StringAssert.Contains("commit", deliveredPayload);
            StringAssert.Contains("hasAim", deliveredPayload);
        }

        [Test]
        public void Loopback_CommitWithoutAimDir_SerializesHasAimFalse()
        {
            var codec = UnityJsonProtocol.Instance;

            var commit = new CommitMessage
            {
                v = 1,
                type = "commit",
                roundId = 2,
                positionX = -0.5f,
                positionY = 1.0f,
                hasAim = false,
                aimX = 0f,
                aimY = 0f
            };

            string json = codec.ToJson(commit);

            StringAssert.Contains("hasAim", json);
            StringAssert.DoesNotContain("aimDir", json);

            var roundTripped = codec.FromJson<CommitMessage>(json);
            Assert.IsNotNull(roundTripped);
            Assert.IsFalse(roundTripped.hasAim);
            Assert.AreEqual(-0.5f, roundTripped.positionX, 0.001f);
            Assert.AreEqual(1.0f, roundTripped.positionY, 0.001f);
        }

        [Test]
        public void Loopback_MatchEnd_DeliversWinners()
        {
            var codec = UnityJsonProtocol.Instance;

            Func<string, IReadOnlyList<string>> serverLogic = _ =>
            {
                var endMsg = new MatchEndMessage
                {
                    v = 1,
                    type = "match_end",
                    winners = new[] { "p1", "p2" }
                };
                return new[] { codec.ToJson(endMsg) };
            };

            using var transport = new LoopbackTransport(codec, serverLogic);
            using var session = new ClientMatchSession(transport);

            MatchEndMessage receivedEnd = null;
            session.MessageReceived += msg =>
            {
                if (msg is MatchEndMessage end) receivedEnd = end;
            };

            session.Connect();
            session.Send(new LeaveMessage { v = 1, type = "leave" });

            Assert.AreEqual(ClientMatchPhase.MatchEnd, session.Phase);
            Assert.IsNotNull(receivedEnd);
            Assert.AreEqual(2, receivedEnd.winners.Length);
            Assert.AreEqual("p1", receivedEnd.winners[0]);
            Assert.AreEqual("p2", receivedEnd.winners[1]);
        }
    }
}
