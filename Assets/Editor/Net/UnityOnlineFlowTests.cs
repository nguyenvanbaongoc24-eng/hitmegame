using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using HitMe.NetClientV1;
using HitMe.OnlineGlue;
using HitMe.UI;

namespace HitMe.Tests
{
    [TestFixture]
    public class UnityOnlineFlowTests
    {
        private class FakeOnlineUi : IOnlineUi
        {
            public readonly List<string> Calls = new List<string>();

            public void ShowLobby(RoomStateMessage room) => Calls.Add($"lobby:{room?.players?.Length ?? 0}");
            public void ShowWaitingForPlayers(int humans, int maxPlayers) => Calls.Add($"waiting:{humans}/{maxPlayers}");
            public void ShowNotEnoughPlayers(int minPlayers) => Calls.Add($"not_enough:{minPlayers}");
            public void ShowQuickMatchSearching() => Calls.Add("quick_searching");
            public void ShowQuickMatchHint() => Calls.Add("quick_hint");
            public void ShowCountdown(int seconds) => Calls.Add($"countdown:{seconds}");
            public void ShowPlacing(int round, int secondsLeft) => Calls.Add($"placing:{round}:{secondsLeft}");
            public void ShowLocked() => Calls.Add("locked");
            public void ShowReveal(RevealMessage reveal) => Calls.Add("reveal");
            public void ShowRoundResult(RoundResultMessage result) => Calls.Add("round_result");
            public void ShowMatchEnd(MatchEndMessage result) => Calls.Add("match_end");
            public void ShowError(string code, string textVi) => Calls.Add($"error:{code}");
            public void ShowReconnecting() => Calls.Add("reconnecting");
            public void ShowServerShutdown() => Calls.Add("server_shutdown");
            public void ShowEmote(string playerId, string emoteId, bool visibleOnCharacter) => Calls.Add($"emote:{playerId}:{emoteId}");
        }

        [Test]
        public void Test_UnityJsonProtocol_SerializesAndDeserializes()
        {
            var codec = UnityJsonProtocol.Instance;
            var join = new JoinMessage
            {
                type = "join",
                token = "guest:Player1",
                roomCode = "HM1234"
            };

            string json = codec.ToJson(join);
            Assert.IsNotEmpty(json);
            Assert.IsTrue(json.Contains("guest:Player1"));
            Assert.IsTrue(json.Contains("HM1234"));

            var parsed = codec.FromJson<JoinMessage>(json);
            Assert.IsNotNull(parsed);
            Assert.AreEqual("join", parsed.type);
            Assert.AreEqual("guest:Player1", parsed.token);
            Assert.AreEqual("HM1234", parsed.roomCode);
        }

        [Test]
        public void Test_LoopbackTransport_DeliversMessages()
        {
            var codec = UnityJsonProtocol.Instance;
            var sent = new List<string>();

            using var transport = new LoopbackTransport(codec, raw =>
            {
                sent.Add(raw);
                return Array.Empty<string>();
            });

            transport.Connect();
            Assert.IsTrue(transport.IsConnected);

            transport.Send(new StartMessage { type = "start" });
            Assert.AreEqual(1, sent.Count);
            Assert.IsTrue(sent[0].Contains("\"type\":\"start\""));
        }

        [Test]
        public void Test_OnlineFlowController_FullLifecycle_Loopback()
        {
            var codec = UnityJsonProtocol.Instance;
            var ui = new FakeOnlineUi();

            var serverResponses = new Queue<string>();

            using var transport = new LoopbackTransport(codec, clientPayload =>
            {
                var responses = new List<string>();
                while (serverResponses.Count > 0)
                {
                    responses.Add(serverResponses.Dequeue());
                }
                return responses;
            });

            using var session = new ClientMatchSession(transport);
            using var flow = new OnlineFlowController(session, ui);

            // Connect transport first
            transport.Connect();

            // Step 1: Join Room
            flow.Join("guest:Player1", "HM9999");

            // Simulate server response: room_state
            serverResponses.Enqueue("{\"v\":1,\"type\":\"room_state\",\"protocolVersion\":1,\"roomCode\":\"HM9999\",\"players\":[{\"id\":\"p1\",\"name\":\"Player1\",\"ready\":true}],\"config\":{\"maxPlayers\":6,\"minPlayersToStart\":2}}");
            transport.Send(new RoomReadyMessage { ready = true });
            flow.Tick();

            Assert.AreEqual("HM9999", session.RoomCode);
            Assert.IsTrue(ui.Calls.Contains("lobby:1"));

            // Step 2: Not Enough Players
            flow.HostStart();
            flow.Tick();

            // Simulate round start
            serverResponses.Enqueue("{\"v\":1,\"type\":\"round_start\",\"roundId\":1,\"deadline\":9999999999999}");
            transport.Send(new LockReadyMessage());
            flow.Tick();

            Assert.AreEqual(ClientMatchPhase.Placing, session.Phase);

            // Step 3: Commit placement
            flow.Commit(1, 2f, 0f, true, 0f, 1f);
            flow.Tick();

            // Simulate reveal
            serverResponses.Enqueue("{\"v\":1,\"type\":\"reveal\",\"roundId\":1,\"players\":[{\"id\":\"p1\",\"position\":{\"x\":2,\"y\":0},\"aim\":{\"x\":0,\"y\":1}}]}");
            transport.Send(new LockReadyMessage());
            flow.Tick();

            Assert.AreEqual(ClientMatchPhase.Revealed, session.Phase);
            Assert.IsTrue(ui.Calls.Contains("reveal"));

            // Step 4: Round Result
            serverResponses.Enqueue("{\"v\":1,\"type\":\"round_result\",\"roundId\":1,\"hits\":[],\"players\":[{\"id\":\"p1\",\"hp\":3,\"eliminated\":false}]}");
            transport.Send(new LockReadyMessage());
            flow.Tick();

            Assert.IsTrue(ui.Calls.Contains("round_result"));

            // Step 5: Match End
            serverResponses.Enqueue("{\"v\":1,\"type\":\"match_end\",\"winners\":[\"p1\"]}");
            transport.Send(new LockReadyMessage());
            flow.Tick();

            Assert.AreEqual(ClientMatchPhase.MatchEnd, session.Phase);
            Assert.IsTrue(ui.Calls.Contains("match_end"));
        }

        [Test]
        public void Test_NetConfig_DefaultDisabled()
        {
            Assert.IsFalse(NetConfig.Enabled, "NetConfig.Enabled must be false by default for release safety.");
        }

        [Test]
        public void Test_AllNetErrorCodes_AreLocalized_NoRawCodes()
        {
            string[] knownCodes =
            {
                "not_enough_players", "protocol_mismatch", "auth_required_use_join_token",
                "auth_invalid", "token_invalid", "reconnect_invalid", "room_full", "room_limit",
                "room_code_exhausted", "room_not_found", "room_started", "banned",
                "rate_limited", "message_too_large", "legacy_ready_use_room_ready_or_lock_ready",
                "invalid_json", "invalid_envelope", "unknown_type", "already_joined",
                "not_joined", "not_in_room", "not_in_round", "wrong_phase", "wrong_round",
                "round_locked", "ready_required", "match_not_started", "invalid_commit",
                "invalid_emote", "emote_rate_limited", "emote_phase_limit", "host_only",
                "duplicate_player", "invalid_player", "no_response", "server_shutdown"
            };

            foreach (var code in knownCodes)
            {
                string msgVi = UiStrings.GetNetErrorMessage(code);
                Assert.IsFalse(string.IsNullOrEmpty(msgVi), $"Missing VI message for code: {code}");
                Assert.IsFalse(msgVi.Contains("Lỗi mạng: " + code), $"Code {code} was not localized in VI!");
            }
        }

        [Test]
        public void Test_SessionReplaced_DispatchesErrorToUi()
        {
            var codec = UnityJsonProtocol.Instance;
            var ui = new FakeOnlineUi();
            using var transport = new LoopbackTransport(codec, _ => Array.Empty<string>());
            using var session = new ClientMatchSession(transport);
            using var flow = new OnlineFlowController(session, ui);

            transport.Connect();
            transport.Deliver("{\"v\":1,\"type\":\"error\",\"code\":\"session_replaced\"}");

            Assert.IsTrue(ui.Calls.Contains("error:session_replaced"));
        }

        [Test]
        public void Test_BannedPlayer_IncludesRemainingSecondsInMessage()
        {
            var codec = UnityJsonProtocol.Instance;
            var ui = new FakeOnlineUi();
            using var transport = new LoopbackTransport(codec, _ => Array.Empty<string>());
            using var session = new ClientMatchSession(transport);
            using var flow = new OnlineFlowController(session, ui);

            transport.Connect();
            transport.Deliver("{\"v\":1,\"type\":\"error\",\"code\":\"banned\",\"remainingSeconds\":120}");

            Assert.IsTrue(ui.Calls.Contains("error:banned"));
        }

        [Test]
        public void Test_Reconnect_IncludesPrivateReconnectToken()
        {
            var codec = UnityJsonProtocol.Instance;
            var ui = new FakeOnlineUi();
            var sent = new List<string>();
            using var transport = new LoopbackTransport(codec, raw =>
            {
                sent.Add(raw);
                return Array.Empty<string>();
            });
            using var session = new ClientMatchSession(transport);
            using var flow = new OnlineFlowController(session, ui);

            transport.Connect();
            flow.Join("guest:Player1", "HM4826");

            // Receive private reconnect token
            transport.Deliver("{\"v\":1,\"type\":\"room_state\",\"protocolVersion\":1,\"roomCode\":\"HM4826\",\"reconnectToken\":\"secret_tok_123\",\"players\":[{\"id\":\"p1\",\"name\":\"Player1\",\"ready\":false}],\"config\":{\"maxPlayers\":6,\"minPlayersToStart\":2}}");
            Assert.AreEqual("secret_tok_123", session.ReconnectToken);

            sent.Clear();
            flow.Reconnect();

            Assert.AreEqual(1, sent.Count);
            Assert.IsTrue(sent[0].Contains("secret_tok_123"));
            Assert.IsTrue(sent[0].Contains("guest:Player1"));
        }
    }
}
