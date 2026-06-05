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
        private Text scoreText;
        private Text detailsText;

        public void Initialize()
        {
            var background = gameObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.04f, 0.06f, 0.98f);
            var outline = gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.75f, 0.16f, 0.18f);
            outline.effectDistance = new Vector2(4f, -4f);

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandHeight = false;

            var title = UiFactory.CreateText(transform, "Title", "Run perdue", 34, TextAnchor.MiddleCenter);
            title.color = new Color(1f, 0.32f, 0.28f);
            title.GetComponent<LayoutElement>().preferredHeight = 56f;
            scoreText = UiFactory.CreateText(transform, "Score", string.Empty, 22, TextAnchor.MiddleCenter);
            scoreText.GetComponent<LayoutElement>().preferredHeight = 52f;
            detailsText = UiFactory.CreateText(transform, "Details", string.Empty, 15, TextAnchor.MiddleCenter);
            detailsText.color = new Color(0.78f, 0.82f, 0.9f);
            detailsText.GetComponent<LayoutElement>().preferredHeight = 90f;

            var back = UiFactory.CreateButton(transform, "BackToMenuButton", "Retour au menu");
            back.GetComponent<Image>().color = new Color(0.32f, 0.16f, 0.18f);
            back.GetComponent<LayoutElement>().preferredWidth = 240f;
            back.GetComponent<LayoutElement>().preferredHeight = 56f;
            back.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.MainMenu));
        }

        public void Render(RunState run)
        {
            scoreText.text = $"Score: {run.CurrentLevel.CurrentScore} / {run.CurrentLevel.Quota}";
            detailsText.text = $"Floor {run.CurrentLevel.FloorIndex} | Level {run.CurrentLevel.LevelIndex}\nCredits finaux: ${run.Credits}\nRetente une run depuis le menu.";
        }
    }
}
