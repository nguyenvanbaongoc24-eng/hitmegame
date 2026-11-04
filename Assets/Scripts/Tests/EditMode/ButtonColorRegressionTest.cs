using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

namespace HitMe.Tests
{
    public class ButtonColorRegressionTest
    {
        [Test]
        public void TestButtonColors_BTN06_NoWhiteOrTransparentButtons()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Graybox.unity");
            var canvas = GameObject.Find("ProductionCanvas");
            Assert.IsNotNull(canvas, "ProductionCanvas not found in Graybox.unity");

            // 1. Primary Button (BtnQuickMatch)
            var quickGo = GameObject.Find("BtnQuickMatch");
            Assert.IsNotNull(quickGo, "BtnQuickMatch not found");
            var quickImg = quickGo.GetComponent<Image>();
            Assert.IsNotNull(quickImg, "BtnQuickMatch has no Image component");
            Assert.IsNotNull(quickImg.sprite, "BtnQuickMatch has no Sprite assigned (fallback white texture)");
            Assert.AreNotEqual(Color.clear, quickImg.color, "BtnQuickMatch has Color.clear");
            Assert.IsTrue(quickImg.color.a > 0.5f, "BtnQuickMatch alpha is too low");

            var quickTex = quickImg.sprite.texture;
            Assert.IsNotNull(quickTex, "BtnQuickMatch sprite texture is null");
            Color centerQuick = quickTex.GetPixel(quickTex.width / 2, quickTex.height / 2);
            // Yellow check: R > 0.80, G > 0.60, B < 0.40
            Assert.IsTrue(centerQuick.r > 0.80f && centerQuick.g > 0.55f && centerQuick.b < 0.40f,
                $"BtnQuickMatch is not yellow/gold: {centerQuick}");

            // 2. Blue Button (BtnCreateRoom)
            var createGo = GameObject.Find("BtnCreateRoom");
            Assert.IsNotNull(createGo, "BtnCreateRoom not found");
            var createImg = createGo.GetComponent<Image>();
            Assert.IsNotNull(createImg.sprite, "BtnCreateRoom has no Sprite assigned");
            var createTex = createImg.sprite.texture;
            Color centerCreate = createTex.GetPixel(createTex.width / 2, createTex.height / 2);
            Assert.IsTrue(centerCreate.b > centerCreate.r && centerCreate.b > 0.70f,
                $"BtnCreateRoom is not blue: {centerCreate}");

            // 3. Green Button (BtnJoinRoom)
            var joinGo = GameObject.Find("BtnJoinRoom");
            Assert.IsNotNull(joinGo, "BtnJoinRoom not found");
            var joinImg = joinGo.GetComponent<Image>();
            Assert.IsNotNull(joinImg.sprite, "BtnJoinRoom has no Sprite assigned");
            var joinTex = joinImg.sprite.texture;
            Color centerJoin = joinTex.GetPixel(joinTex.width / 2, joinTex.height / 2);
            Assert.IsTrue(centerJoin.g > centerJoin.r && centerJoin.g > 0.50f,
                $"BtnJoinRoom is not green: {centerJoin}");

            // 4. Navy Button (BtnBotTraining)
            var botGo = GameObject.Find("BtnBotTraining");
            Assert.IsNotNull(botGo, "BtnBotTraining not found");
            var botImg = botGo.GetComponent<Image>();
            Assert.IsNotNull(botImg.sprite, "BtnBotTraining has no Sprite assigned");
            var botTex = botImg.sprite.texture;
            Color centerBot = botTex.GetPixel(botTex.width / 2, botTex.height / 2);
            Assert.IsTrue(centerBot.b > centerBot.r && centerBot.r < 0.35f,
                $"BtnBotTraining is not navy: {centerBot}");

            // 5. Bot Training Start Button (BtnStartBot inside inactive BotTrainingPanel)
            GameObject startBotGo = null;
            foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "BtnStartBot") { startBotGo = t.gameObject; break; }
            }
            Assert.IsNotNull(startBotGo, "BtnStartBot not found in canvas hierarchy");
            var startImg = startBotGo.GetComponent<Image>();
            Assert.IsNotNull(startImg.sprite, "BtnStartBot has no Sprite assigned");
            Color centerStart = startImg.sprite.texture.GetPixel(startImg.sprite.texture.width / 2, startImg.sprite.texture.height / 2);
            Assert.IsTrue(centerStart.r > 0.80f && centerStart.g > 0.55f && centerStart.b < 0.40f,
                $"BtnStartBot is not yellow: {centerStart}");
        }
    }
}
