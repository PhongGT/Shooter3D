using UnityEngine;
using UnityEngine.UI;

namespace Shooter3D
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private float missionDuration = 50f;
        [SerializeField] private EnemyController[] targets;
        [SerializeField] private Text hpText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Text resultText;

        private int hp = 100;
        private int score;
        private float elapsed;
        private float feedbackUntil;
        private bool finished;

        public void Configure(EnemyController[] encounterTargets, Text hp, Text scoreLabel, Text timer, Text feedback, GameObject results, Text result)
        {
            targets = encounterTargets;
            hpText = hp;
            scoreText = scoreLabel;
            timerText = timer;
            feedbackText = feedback;
            resultPanel = results;
            resultText = result;
        }

        private void Awake()
        {
            Instance = this;
            if (targets != null)
            {
                foreach (EnemyController target in targets)
                {
                    target.Prepare();
                }
            }
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }
            RefreshHUD();
        }

        private void Update()
        {
            if (finished)
            {
                return;
            }

            elapsed += Time.deltaTime;
            if (targets != null)
            {
                foreach (EnemyController target in targets)
                {
                    if (!target.Triggered && elapsed >= target.ActivationTime)
                    {
                        target.ActivateTarget();
                    }
                }
            }

            if (feedbackText != null && feedbackText.gameObject.activeSelf && Time.time >= feedbackUntil)
            {
                feedbackText.gameObject.SetActive(false);
            }

            RefreshHUD();
            if (elapsed >= missionDuration || hp <= 0)
            {
                FinishMission();
            }
        }

        public void DamagePlayer(int amount)
        {
            if (finished)
            {
                return;
            }
            hp = Mathf.Max(0, hp - amount);
            ShowFeedback("INCOMING FIRE  -" + amount + " HP", new Color(1f, 0.35f, 0.28f));
        }

        public void RegisterEnemyDown(int points, HitZone zone)
        {
            score += points;
            string label = zone == HitZone.Head ? "PRECISION SHOT" : zone == HitZone.Arm ? "JUSTICE SHOT" : "TARGET DOWN";
            ShowFeedback(label + "  +" + points, new Color(1f, 0.82f, 0.2f));
        }

        public void RegisterCivilianHit()
        {
            score = Mathf.Max(0, score - 250);
            DamagePlayer(20);
            ShowFeedback("CIVILIAN HIT  -250", new Color(0.35f, 0.85f, 1f));
        }

        private void ShowFeedback(string message, Color color)
        {
            if (feedbackText == null)
            {
                return;
            }
            feedbackText.text = message;
            feedbackText.color = color;
            feedbackText.gameObject.SetActive(true);
            feedbackUntil = Time.time + 1.3f;
        }

        private void RefreshHUD()
        {
            if (hpText != null) hpText.text = "HP  " + hp;
            if (scoreText != null) scoreText.text = "SCORE  " + score.ToString("0000");
            if (timerText != null) timerText.text = Mathf.Max(0f, missionDuration - elapsed).ToString("00.0");
        }

        private void FinishMission()
        {
            finished = true;
            if (resultPanel != null) resultPanel.SetActive(true);
            if (resultText != null)
            {
                resultText.text = hp > 0 ? "ROUTE CLEAR\nSCORE  " + score : "MISSION FAILED\nSCORE  " + score;
            }
        }
    }
}
