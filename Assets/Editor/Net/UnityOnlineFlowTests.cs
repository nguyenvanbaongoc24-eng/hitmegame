using System;
using System.Collections.Generic;
using NUnit.Framework;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#else
using Assert = NUnit.Framework.Legacy.ClassicAssert;
#endif
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
            public void ShowEmote(string playerId, string emoteId, bool visibleOnCharacter) => Calls.Add($"emote:{playerId}:{emoteId}:{visibleOnCharacter}");
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
                "duplicate_player", "invalid_player", "no_response", "server_shutdown",
                "server_restarting", "idle_timeout", "session_replaced", "bad_message",
                "origin_not_allowed", "client_outdated", "room_expired", "server_busy",
                "protocol_violation_eliminated", "name_taken_in_room", "character_not_found",
                "character_locked", "weapon_not_found", "weapon_not_owned", "profile_unavailable",
                "invalid_chat_id", "chat_rate_limited", "invalid_friend_target",
                "friend_target_unavailable", "friend_request_pending", "friend_request_not_found",
                "already_friends", "friend_request_closed", "friend_unavailable"
            };

            UiStrings.SetLanguage(AppLanguage.Vietnamese);
            foreach (var code in knownCodes)
            {
                string msgVi = UiStrings.GetNetErrorMessage(code, 120);
                Assert.IsFalse(string.IsNullOrEmpty(msgVi), $"Missing VI message for code: {code}");
                Assert.IsFalse(msgVi.Contains("Lỗi mạng: " + code), $"Code {code} was not localized in VI!");
                Assert.IsFalse(msgVi.Contains(code), $"Raw code {code} leaked in VI message: {msgVi}");
            }

            UiStrings.SetLanguage(AppLanguage.English);
            foreach (var code in knownCodes)
            {
                string msgEn = UiStrings.GetNetErrorMessage(code, 120);
                Assert.IsFalse(string.IsNullOrEmpty(msgEn), $"Missing EN message for code: {code}");
                Assert.IsFalse(msgEn.Contains("Network Error: " + code), $"Code {code} was not localized in EN!");
                Assert.IsFalse(msgEn.Contains(code), $"Raw code {code} leaked in EN message: {msgEn}");
            }
            UiStrings.SetLanguage(AppLanguage.Vietnamese);
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

        [Test]
        public void Test_RoomReady_IsSentExactlyOnce_NotRepeatedOnRoomState()
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
            sent.Clear();

            // 1. User clicks ready -> exactly 1 room_ready is sent
            flow.SetRoomReady(true);
            Assert.AreEqual(1, sent.Count, "SetRoomReady should send exactly 1 message");
            Assert.IsTrue(sent[0].Contains("room_ready"), "Message must be room_ready");

            // 2. Server broadcasts 5 consecutive room_state updates (peers joining, ready status updates)
            for (int i = 1; i <= 5; i++)
            {
                transport.Deliver($"{{\"v\":1,\"type\":\"room_state\",\"protocolVersion\":1,\"roomCode\":\"HM4826\",\"players\":[{{\"id\":\"p1\",\"name\":\"Player1\",\"ready\":true}},{{\"id\":\"p{i+1}\",\"name\":\"Player{i+1}\",\"ready\":false}}],\"config\":{{\"maxPlayers\":6,\"minPlayersToStart\":2}}}}");
            }

            // 3. Verify sent count is STILL 1 (no automatic repetition of room_ready)
            Assert.AreEqual(1, sent.Count, "Client must NEVER automatically re-send room_ready upon receiving room_state");
        }

        [Test]
        public void Replay_FullMatch_2Players_JoinToMatchEnd_ProtocolJsonl()
        {
            var codec = UnityJsonProtocol.Instance;
            var ui = new FakeOnlineUi();
            using var transport = new LoopbackTransport(codec, _ => Array.Empty<string>());
            using var session = new ClientMatchSession(transport);
            using var flow = new OnlineFlowController(session, ui);

            string[] jsonlTrace =
            {
                "{\"v\":1,\"type\":\"room_state\",\"protocolVersion\":1,\"roomCode\":\"HM4826\",\"hostId\":\"p1\",\"started\":false,\"players\":[{\"id\":\"p1\",\"name\":\"Nam\",\"ready\":false}],\"reconnectToken\":\"tok_p1_a\",\"config\":{\"maxPlayers\":6,\"minPlayersToStart\":2,\"startHp\":3,\"placeSeconds\":5,\"maxRounds\":5}}",
                "{\"v\":1,\"type\":\"room_state\",\"protocolVersion\":1,\"roomCode\":\"HM4826\",\"hostId\":\"p1\",\"started\":false,\"players\":[{\"id\":\"p1\",\"name\":\"Nam\",\"ready\":true},{\"id\":\"p2\",\"name\":\"An\",\"ready\":true}],\"reconnectToken\":\"tok_p1_a\",\"config\":{\"maxPlayers\":6,\"minPlayersToStart\":2,\"startHp\":3,\"placeSeconds\":5,\"maxRounds\":5}}",
                "{\"v\":1,\"type\":\"round_start\",\"roundId\":1,\"deadline\":1800000005000}",
                "{\"v\":1,\"type\":\"emote\",\"playerId\":\"p2\",\"emoteId\":\"happy\",\"phase\":\"placing\"}",
                "{\"v\":1,\"type\":\"reveal\",\"roundId\":1,\"players\":[{\"id\":\"p1\",\"position\":{\"x\":-1.5,\"y\":0.0},\"aim\":{\"x\":1.0,\"y\":0.0}},{\"id\":\"p2\",\"position\":{\"x\":1.5,\"y\":0.0},\"aim\":{\"x\":-1.0,\"y\":0.0}}]}",
                "{\"v\":1,\"type\":\"round_result\",\"roundId\":1,\"hits\":[{\"shooterId\":\"p1\",\"targetId\":\"p2\",\"point\":{\"x\":1.2,\"y\":0.0},\"distance\":2.7}],\"players\":[{\"id\":\"p1\",\"hp\":3,\"eliminated\":false},{\"id\":\"p2\",\"hp\":2,\"eliminated\":false}]}",
                "{\"v\":1,\"type\":\"match_end\",\"winners\":[\"p1\"]}"
            };

            transport.Connect();
            foreach (var line in jsonlTrace)
            {
                transport.Deliver(line);
            }

            Assert.AreEqual("HM4826", session.RoomCode);
            Assert.AreEqual(ClientMatchPhase.MatchEnd, session.Phase);
            Assert.IsTrue(ui.Calls.Contains("lobby:1"));
            Assert.IsTrue(ui.Calls.Contains("not_enough:2"));
            Assert.IsTrue(ui.Calls.Contains("lobby:2"));
            Assert.IsTrue(ui.Calls.Contains("countdown:3"));
            Assert.IsTrue(ui.Calls.Contains("emote:p2:happy:False"));
            Assert.IsTrue(ui.Calls.Contains("reveal"));
            Assert.IsTrue(ui.Calls.Contains("round_result"));
            Assert.IsTrue(ui.Calls.Contains("match_end"));
        }

        [Test]
        public void Replay_QuickMatch_Waiting_HintAfter30s_AndLeave()
        {
            var codec = UnityJsonProtocol.Instance;
            var ui = new FakeOnlineUi();
            using var transport = new LoopbackTransport(codec, _ => Array.Empty<string>());
            using var session = new ClientMatchSession(transport);
            using var flow = new OnlineFlowController(session, ui);

            transport.Connect();
            flow.QuickMatch("guest:Player1");
            transport.Deliver("{\"v\":1,\"type\":\"quick_match_waiting\",\"quickMatchHintSeconds\":30}");
            Assert.AreEqual(ClientMatchPhase.Queueing, session.Phase);
            Assert.IsTrue(ui.Calls.Contains("quick_searching"));

            transport.Deliver("{\"v\":1,\"type\":\"quick_match_hint\",\"code\":\"no_opponent_yet\",\"quickMatchHintSeconds\":30}");
            Assert.IsTrue(ui.Calls.Contains("quick_hint"));
            Assert.AreEqual(0, session.RoomPlayers.Length, "QuickMatchHint must never add bots to online room.");

            transport.Deliver("{\"v\":1,\"type\":\"quick_match_left\"}");
            Assert.AreEqual(ClientMatchPhase.Disconnected, session.Phase);
        }

        [Test]
        public void Replay_Countdown_CancelsWhenPlayerUnreadies()
        {
            var codec = UnityJsonProtocol.Instance;
            var ui = new FakeOnlineUi();
            using var transport = new LoopbackTransport(codec, _ => Array.Empty<string>());
            using var session = new ClientMatchSession(transport);
            using var flow = new OnlineFlowController(session, ui);

            transport.Connect();
            transport.Deliver("{\"v\":1,\"type\":\"room_state\",\"protocolVersion\":1,\"roomCode\":\"HM4826\",\"started\":false,\"players\":[{\"id\":\"p1\",\"name\":\"P1\",\"ready\":true},{\"id\":\"p2\",\"name\":\"P2\",\"ready\":true}],\"config\":{\"maxPlayers\":6,\"minPlayersToStart\":2}}");
            Assert.IsTrue(ui.Calls.Contains("countdown:3"));

            transport.Deliver("{\"v\":1,\"type\":\"room_state\",\"protocolVersion\":1,\"roomCode\":\"HM4826\",\"started\":false,\"players\":[{\"id\":\"p1\",\"name\":\"P1\",\"ready\":true},{\"id\":\"p2\",\"name\":\"P2\",\"ready\":false}],\"config\":{\"maxPlayers\":6,\"minPlayersToStart\":2}}");
            Assert.AreEqual("countdown:0", ui.Calls[ui.Calls.Count - 1]);
        }

        [Test]
        public void Replay_CloseReason_ServerShutdown_And_ServerRestarting()
        {
            var codec = UnityJsonProtocol.Instance;
            var ui = new FakeOnlineUi();
            using var transport = new LoopbackTransport(codec, _ => Array.Empty<string>());
            using var session = new ClientMatchSession(transport);
            using var flow = new OnlineFlowController(session, ui);

            transport.Connect();
            transport.Deliver("{\"v\":1,\"type\":\"error\",\"code\":\"server_restarting\"}");
            Assert.IsTrue(ui.Calls.Contains("server_shutdown"));
            Assert.IsTrue(ui.Calls.Contains("error:server_restarting"));

            ui.Calls.Clear();
            transport.SimulateClose("server_shutdown");
            Assert.IsTrue(ui.Calls.Contains("server_shutdown"));

            ui.Calls.Clear();
            transport.Connect();
            transport.SimulateClose("network_lost");
            Assert.IsTrue(ui.Calls.Contains("reconnecting"));
        }

        [Test]
        public void Replay_ProtocolMismatch_ThrowsInvalidOperationException()
        {
            var codec = UnityJsonProtocol.Instance;
            using var transport = new LoopbackTransport(codec, _ => Array.Empty<string>());
            using var session = new ClientMatchSession(transport);

            transport.Connect();
            Assert.Throws<InvalidOperationException>(() =>
            {
                transport.Deliver("{\"v\":1,\"type\":\"room_state\",\"protocolVersion\":99,\"roomCode\":\"HM4826\",\"started\":false}");
            });
        }

        [Test]
        public void Replay_Emote_CardOnlyDuringPlacing_CharacterOnReveal()
        {
            var codec = UnityJsonProtocol.Instance;
            var ui = new FakeOnlineUi();
            using var transport = new LoopbackTransport(codec, _ => Array.Empty<string>());
            using var session = new ClientMatchSession(transport);
            using var flow = new OnlineFlowController(session, ui);

            transport.Connect();
            transport.Deliver("{\"v\":1,\"type\":\"round_start\",\"roundId\":1,\"deadline\":1800000005000}");
            transport.Deliver("{\"v\":1,\"type\":\"emote\",\"playerId\":\"p2\",\"emoteId\":\"thumbs_up\",\"phase\":\"placing\"}");
            Assert.IsTrue(ui.Calls.Contains("emote:p2:thumbs_up:False"));

            transport.Deliver("{\"v\":1,\"type\":\"reveal\",\"roundId\":1,\"players\":[]}");
            transport.Deliver("{\"v\":1,\"type\":\"emote\",\"playerId\":\"p2\",\"emoteId\":\"thumbs_up\",\"phase\":\"reveal\"}");
            Assert.IsTrue(ui.Calls.Contains("emote:p2:thumbs_up:True"));
        }

        [Test]
        public void Replay_TwoPlayersInProcess_LoopbackHub_Full5Rounds()
        {
            var codec = UnityJsonProtocol.Instance;
            LoopbackTransport transportA = null;
            LoopbackTransport transportB = null;

            void BroadcastToBoth(string json)
            {
                transportA.Deliver(json);
                transportB.Deliver(json);
            }

            transportA = new LoopbackTransport(codec, _ => Array.Empty<string>());
            transportB = new LoopbackTransport(codec, _ => Array.Empty<string>());

            using (transportA)
            using (transportB)
            using (var sessionA = new ClientMatchSession(transportA))
            using (var sessionB = new ClientMatchSession(transportB))
            {
                var uiA = new FakeOnlineUi();
                var uiB = new FakeOnlineUi();
                using var flowA = new OnlineFlowController(sessionA, uiA);
                using var flowB = new OnlineFlowController(sessionB, uiB);

                transportA.Connect();
                transportB.Connect();

                BroadcastToBoth("{\"v\":1,\"type\":\"room_state\",\"protocolVersion\":1,\"roomCode\":\"HM5555\",\"hostId\":\"p1\",\"started\":false,\"players\":[{\"id\":\"p1\",\"name\":\"HostA\",\"ready\":true},{\"id\":\"p2\",\"name\":\"GuestB\",\"ready\":true}],\"config\":{\"maxPlayers\":6,\"minPlayersToStart\":2}}");
                Assert.AreEqual(ClientMatchPhase.Lobby, sessionA.Phase);
                Assert.AreEqual(ClientMatchPhase.Lobby, sessionB.Phase);

                for (int r = 1; r <= 5; r++)
                {
                    BroadcastToBoth($"{{\"v\":1,\"type\":\"round_start\",\"roundId\":{r},\"deadline\":1800000005000}}");
                    Assert.AreEqual(ClientMatchPhase.Placing, sessionA.Phase);
                    Assert.AreEqual(ClientMatchPhase.Placing, sessionB.Phase);

                    flowA.Commit(r, -1.5f, 0f, true, 1f, 0f);
                    flowB.Commit(r, 1.5f, 0f, true, -1f, 0f);
                    flowA.LockReady();
                    flowB.LockReady();

                    BroadcastToBoth($"{{\"v\":1,\"type\":\"reveal\",\"roundId\":{r},\"players\":[{{\"id\":\"p1\",\"position\":{{\"x\":-1.5,\"y\":0}},\"aim\":{{\"x\":1,\"y\":0}}}},{{\"id\":\"p2\",\"position\":{{\"x\":1.5,\"y\":0}},\"aim\":{{\"x\":-1,\"y\":0}}}}]}}");
                    Assert.AreEqual(ClientMatchPhase.Revealed, sessionA.Phase);
                    Assert.AreEqual(ClientMatchPhase.Revealed, sessionB.Phase);

                    BroadcastToBoth($"{{\"v\":1,\"type\":\"round_result\",\"roundId\":{r},\"hits\":[],\"players\":[{{\"id\":\"p1\",\"hp\":3,\"eliminated\":false}},{{\"id\":\"p2\",\"hp\":2,\"eliminated\":false}}]}}");
                    Assert.AreEqual(ClientMatchPhase.RoundResult, sessionA.Phase);
                    Assert.AreEqual(ClientMatchPhase.RoundResult, sessionB.Phase);
                }

                BroadcastToBoth("{\"v\":1,\"type\":\"match_end\",\"winners\":[\"p1\"]}");
                Assert.AreEqual(ClientMatchPhase.MatchEnd, sessionA.Phase);
                Assert.AreEqual(ClientMatchPhase.MatchEnd, sessionB.Phase);
            }
        }
    }
}
