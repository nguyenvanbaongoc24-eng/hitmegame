using System;
using UnityEngine;
using UnityEngine.UI;
using HitMe.Core;
using HitMe.Audio;

namespace HitMe.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainMenuPanel;
        public GameObject inGameHUDPanel;
        public GameObject resultPanel;

        [Header("Main Menu Controls")]
        public Button btnTrainWithBot;
        public Button btnQuickMatch;
        public Button btnCreateRoom;
        public Text txtNotice;

        [Header("InGame HUD Controls")]
        public Text txtRound;
        public Text txtTimer;
        public Text txtPhaseBanner;
        public Text txtPlayerHP;
        public Text txtBotHP;
        public Button btnReady;
        public Text txtReadyBtn;
        public Image imgWeaponSlot;
        public Text txtWeaponName;

        [Header("Result Controls")]
        public Text txtResultTitle;
        public Text txtResultSubtitle;
        public Text txtPlayerStats;
        public Text txtBotStats;
        public Button btnRematch;
        public Button btnBackToHome;

        // Callbacks to MatchStateMachine
        public Action OnClickTrainWithBot;
        public Action OnClickReady;
        public Action OnClickRematch;
        public Action OnClickBackToHome;

        private void Start()
        {
            if (btnTrainWithBot != null)
                btnTrainWithBot.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    OnClickTrainWithBot?.Invoke();
                });

            if (btnQuickMatch != null)
            {
                btnQuickMatch.gameObject.SetActive(false);
                btnQuickMatch.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    ShowNotice("Chơi nhanh đang được phát triển. Hãy chọn Luyện với Bot.");
                });
            }

            if (btnCreateRoom != null)
                btnCreateRoom.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    ShowNotice("Tính năng Tạo phòng đang phát triển (Chưa khả dụng ở bản local)");
                });

            if (btnReady != null)
                btnReady.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayReady();
                    OnClickReady?.Invoke();
                });

            if (btnRematch != null)
                btnRematch.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    OnClickRematch?.Invoke();
                });

            if (btnBackToHome != null)
                btnBackToHome.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayClick();
                    OnClickBackToHome?.Invoke();
                });

            ShowMainMenu();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                if (mainMenuPanel != null && mainMenuPanel.activeSelf)
                {
                    AudioManager.Instance?.PlayClick();
                    OnClickTrainWithBot?.Invoke();
                }
                else if (inGameHUDPanel != null && inGameHUDPanel.activeSelf)
                {
                    if (btnReady != null && btnReady.interactable)
                    {
                        AudioManager.Instance?.PlayReady();
                        OnClickReady?.Invoke();
                    }
                }
                else if (resultPanel != null && resultPanel.activeSelf)
                {
                    AudioManager.Instance?.PlayClick();
                    OnClickRematch?.Invoke();
                }
            }
        }

        public void ShowNotice(string msg)
        {
            if (txtNotice != null)
            {
                txtNotice.text = msg;
                CancelInvoke(nameof(ClearNotice));
                Invoke(nameof(ClearNotice), 3.0f);
            }
        }

        private void ClearNotice()
        {
            if (txtNotice != null) txtNotice.text = "";
        }

        public void ShowMainMenu()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (inGameHUDPanel != null) inGameHUDPanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(false);
            ClearNotice();
        }

        public void ShowInGameHUD()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (inGameHUDPanel != null) inGameHUDPanel.SetActive(true);
            if (resultPanel != null) resultPanel.SetActive(false);
            if (btnReady != null) btnReady.gameObject.SetActive(true);
        }

        public void ShowResult(MatchResultType result, ActorStats playerStats, ActorStats botStats)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (inGameHUDPanel != null) inGameHUDPanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(true);

            if (txtResultTitle != null)
            {
                switch (result)
                {
                    case MatchResultType.PlayerWin:
                        txtResultTitle.text = "CHIẾN THẮNG!";
                        txtResultTitle.color = new Color(1f, 0.85f, 0.2f); // Gold
                        if (txtResultSubtitle != null) txtResultSubtitle.text = "Bạn đã hạ gục đối thủ xuất sắc!";
                        AudioManager.Instance?.PlayWin();
                        break;
                    case MatchResultType.BotWin:
                        txtResultTitle.text = "THẤT BẠI!";
                        txtResultTitle.color = new Color(1f, 0.35f, 0.35f); // Red
                        if (txtResultSubtitle != null) txtResultSubtitle.text = "Bot đã đánh bại bạn trong trận này!";
                        AudioManager.Instance?.PlayLose();
                        break;
                    case MatchResultType.Draw:
                        txtResultTitle.text = "HÒA NHAU!";
                        txtResultTitle.color = new Color(0.2f, 0.9f, 1f); // Cyan
                        if (txtResultSubtitle != null) txtResultSubtitle.text = "Cả hai cùng hết HP trong cùng đợt ném!";
                        AudioManager.Instance?.PlayWin();
                        break;
                }
            }

            if (txtPlayerStats != null)
            {
                txtPlayerStats.text = $"Bạn (Nam Làng)\nSố lượt ném: {playerStats.throwsCount}\nTrúng đích: {playerStats.hitsCount}\nTỉ lệ trúng: {playerStats.HitRate:F1}%";
            }

            if (txtBotStats != null)
            {
                txtBotStats.text = $"Bot Luyện Tập\nSố lượt ném: {botStats.throwsCount}\nTrúng đích: {botStats.hitsCount}\nTỉ lệ trúng: {botStats.HitRate:F1}%";
            }
        }

        public void UpdateRound(int round)
        {
            if (txtRound != null)
            {
                txtRound.text = $"VÒNG {Mathf.Max(1, round)}/{CombatRules.MaxRounds}";
            }
        }

        public void UpdateTimer(float remainingSeconds)
        {
            if (txtTimer != null)
            {
                int s = Mathf.CeilToInt(Mathf.Max(0f, remainingSeconds));
                txtTimer.text = (s > 0) ? $"{s}s" : "";
                if (s <= 2)
                    txtTimer.color = new Color(1f, 0.25f, 0.25f);
                else
                    txtTimer.color = Color.white;
            }
        }

        public void UpdatePhaseBanner(string text)
        {
            if (txtPhaseBanner != null)
            {
                txtPhaseBanner.text = text ?? "";
            }
        }

        public void UpdateHP(int playerHp, int playerMax, int botHp, int botMax)
        {
            if (txtPlayerHP != null)
            {
                txtPlayerHP.text = $"BẠN: HP {Mathf.Max(0, playerHp)}/{playerMax}";
            }
            if (txtBotHP != null)
            {
                txtBotHP.text = $"BOT: HP {Mathf.Max(0, botHp)}/{botMax}";
            }
        }

        public void SetReadyButtonState(bool isLocked)
        {
            if (btnReady != null)
            {
                btnReady.interactable = !isLocked;
                btnReady.gameObject.SetActive(true);
            }
            if (txtReadyBtn != null)
            {
                txtReadyBtn.text = isLocked ? "ĐÃ KHÓA" : "SẴN SÀNG";
                txtReadyBtn.color = isLocked ? new Color(0.45f, 0.45f, 0.45f) : new Color(0.24f, 0.12f, 0f);
            }
        }
    }
}
