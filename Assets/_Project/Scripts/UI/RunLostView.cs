using DomiNox.Core;
using DomiNox.Run;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class RunLostView : MonoBehaviour
    {
        private Text titleText;
        private Text scoreText;
        private Text detailsText;

        public void Initialize()
        {
            var bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.03f, 0.04f, 0.98f);
            DomiNoxTheme.AddOutline(gameObject, DomiNoxTheme.MultRed, 4f);
            DomiNoxTheme.AddShadow(gameObject, new Color(1f, 0.1f, 0.05f, 0.25f), new Vector2(0f, -4f));

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 28, 28);
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandHeight = false;

            titleText = UiFactory.CreateText(transform, "Title", "RUN OVER", DomiNoxTheme.FontXXL, TextAnchor.MiddleCenter);
            titleText.color = DomiNoxTheme.MultRed;
            titleText.fontStyle = FontStyle.Bold;
            DomiNoxTheme.AddShadow(titleText.gameObject, new Color(1f, 0.1f, 0.05f, 0.45f), new Vector2(2f, -2f));
            titleText.GetComponent<LayoutElement>().preferredHeight = 56f;

            UiFactory.CreateSeparator(transform, 1f, DomiNoxTheme.WithAlpha(DomiNoxTheme.MultRed, 0.4f));

            scoreText = UiFactory.CreateText(transform, "Score", string.Empty, DomiNoxTheme.FontLG, TextAnchor.MiddleCenter);
            scoreText.color = DomiNoxTheme.TextPrimary;
            scoreText.GetComponent<LayoutElement>().preferredHeight = 48f;

            detailsText = UiFactory.CreateText(transform, "Details", string.Empty, DomiNoxTheme.FontSM, TextAnchor.MiddleCenter);
            detailsText.color = DomiNoxTheme.TextSecondary;
            detailsText.GetComponent<LayoutElement>().preferredHeight = 80f;

            var back = UiFactory.CreateButton(transform, "BackToMenuButton", "Main Menu");
            back.GetComponent<LayoutElement>().preferredWidth = 260f;
            back.GetComponent<LayoutElement>().preferredHeight = 56f;
            UiFactory.StyleButton(back, new Color(0.28f, 0.10f, 0.10f), DomiNoxTheme.MultRed, 56f);
            var backLabel = back.GetComponentInChildren<Text>();
            if (backLabel != null) backLabel.fontSize = DomiNoxTheme.FontLG;
            back.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.MainMenu));
        }

        public void Render(RunState run)
        {
            var won = run.Phase == RunPhase.RunWon;
            if (titleText != null)
            {
                titleText.text = won ? "BOUCLE 1 COMPLETE" : "RUN OVER";
                titleText.color = won ? DomiNoxTheme.Gold : DomiNoxTheme.MultRed;
            }

            scoreText.text = won
                ? "Le Schisme est debloque !"
                : $"Score: {run.CurrentLevel.CurrentScore} / {run.CurrentLevel.Quota}";
            detailsText.text = won
                ? $"Floor {run.CurrentLevel.FloorIndex} franchi  ·  Final credits: ${run.Credits}\nRelance une partie : tu peux desormais poser une 2e region\net depasser le 3e etage."
                : $"Floor {run.CurrentLevel.FloorIndex}  ·  Level {run.CurrentLevel.LevelIndex}\nFinal credits: ${run.Credits}\nTry again from the main menu.";
        }
    }
}
