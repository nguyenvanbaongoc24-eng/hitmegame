using System;
using System.Collections.Generic;
using HitMe.NetClientV1;
using NUnit.Framework;
using UnityEngine;

namespace HitMe.Tests
{
    [TestFixture]
    public sealed class GameClientTests
    {
        [Test]
        public void ClientAuthService_GeneratesAndPersistsGuestToken()
        {
            string token = ClientAuthService.GetOrCreateGuestToken();
            Assert.IsNotNull(token);
            Assert.IsTrue(token.StartsWith("guest:"), $"Token should start with guest:, got {token}");

            // Verify persistence
            string retrieved = ClientAuthService.GetOrCreateGuestToken();
            Assert.AreEqual(token, retrieved, "Guest token must persist across calls.");
        }

        [Test]
        public void ClientAuthService_ResolvesDefaultServerUrl()
        {
            string url = ClientAuthService.GetServerUrl();
            Assert.IsNotNull(url);
            Assert.IsTrue(url.StartsWith("ws://") || url.StartsWith("wss://"), $"Server URL must be WebSocket protocol, got {url}");
        }

        [Test]
        public void MockGameClient_LifecycleAndRoomCreation()
        {
            using var client = new MockGameClient();
            Assert.IsTrue(client.IsConnected);
            Assert.AreEqual(ClientMatchPhase.Lobby, client.CurrentPhase);

            RoomStateMessage receivedRoom = null;
            client.OnRoomStateUpdated += room => receivedRoom = room;

            client.CreateRoom("guest:Tester");
            Assert.IsNotNull(receivedRoom);
            Assert.IsTrue(receivedRoom.roomCode.StartsWith("HM"));
            Assert.AreEqual(1, receivedRoom.players.Length);

            // Starting with 1 player should trigger not_enough_players
            string errorReceived = null;
            client.OnErrorReceived += (code, msg) => errorReceived = code;
            client.HostStart();
            Assert.AreEqual("not_enough_players", errorReceived);
        }

        [Test]
        public void MockGameClient_FullMatchProgression()
        {
            using var client = new MockGameClient();
            client.JoinRoom("HM1234", "guest:Tester");

            int startedRound = 0;
            client.OnRoundStarted += (round, deadline) => startedRound = round;

            client.SetRoomReady(true);
            Assert.AreEqual(1, startedRound);
            Assert.AreEqual(ClientMatchPhase.Placing, client.CurrentPhase);

            // Simulate lock ready -> reveal -> round result
            RevealMessage revealMsg = null;
            client.OnRevealReceived += rev => revealMsg = rev;
            client.LockReady(true);
            Assert.IsNotNull(revealMsg);
            Assert.AreEqual(1, revealMsg.roundId);
        }

        [Test]
        public void WebSocketGameClient_TranslatesAllNetProtocolErrors()
        {
            var errSessionReplaced = new ErrorMessage { v = 1, type = "error", code = "session_replaced" };
            string textReplaced = WebSocketGameClient.TranslateNetError(errSessionReplaced);
            Assert.IsTrue(textReplaced.Contains("thiết bị khác"), "session_replaced must warn about other device");

            var errRoomFull = new ErrorMessage { v = 1, type = "error", code = "room_full" };
            string textFull = WebSocketGameClient.TranslateNetError(errRoomFull);
            Assert.IsTrue(textFull.Contains("đầy"), "room_full must warn room is full");

            var errBanned = new ErrorMessage { v = 1, type = "error", code = "banned", remainingSeconds = 115 };
            string textBanned = WebSocketGameClient.TranslateNetError(errBanned);
            Assert.IsTrue(textBanned.Contains("cấm đấu") && textBanned.Contains("115"), "banned must show remaining seconds");

            var errNotEnough = new ErrorMessage { v = 1, type = "error", code = "not_enough_players" };
            string textNotEnough = WebSocketGameClient.TranslateNetError(errNotEnough);
            Assert.IsTrue(textNotEnough.Contains("ít nhất 2 người"), "not_enough_players must explain requirement");
        }

        [Test]
        public void WebSocketGameClient_LoopbackIntegration_ReceivesServerMessages()
        {
            var codec = UnityJsonProtocol.Instance;
            Func<string, IReadOnlyList<string>> serverLogic = payload =>
            {
                var env = codec.FromJson<Envelope>(payload);
                if (env.type == "join")
                {
                    var state = new RoomStateMessage
                    {
                        v = 1,
                        type = "room_state",
                        protocolVersion = 1,
                        roomCode = "HM9999",
                        started = false,
                        players = new[] { new RoomPlayerDto { id = "p1", name = "TestPlayer", ready = true } },
                        config = new RoomConfigDto { maxPlayers = 6, minPlayersToStart = 2, maxRounds = 5 }
                    };
                    return new[] { codec.ToJson(state) };
                }
                return Array.Empty<string>();
            };

            using var transport = new LoopbackTransport(codec, serverLogic);
            using var client = new WebSocketGameClient(transport);

            RoomStateMessage stateReceived = null;
            client.OnRoomStateUpdated += state => stateReceived = state;

            client.JoinRoom("HM9999", "guest:TestPlayer");
            Assert.IsNotNull(stateReceived);
            Assert.AreEqual("HM9999", stateReceived.roomCode);
            Assert.AreEqual(5, stateReceived.config.maxRounds);
        }
    }
}
