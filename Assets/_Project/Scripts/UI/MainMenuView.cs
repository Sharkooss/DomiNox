using System;
using System.Linq;
using DomiNox.Bosses;
using DomiNox.Core;
using DomiNox.Dominex;
using DomiNox.Patterns;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class MainMenuView : MonoBehaviour
    {
        private GameObject mainRoot;
        private GameObject collectionRoot;
        private Transform collectionContent;
        private Button dominexTab;
        private Button bossesTab;
        private Button patternsTab;
        private Button systemTab;

        private void Start()
        {
            EnsureEventSystem();
            var canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvasComponent = canvas.GetComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            UiFactory.ConfigureCanvasScaler(canvas.GetComponent<CanvasScaler>());

            mainRoot = new GameObject("MainMenuRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            mainRoot.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)mainRoot.transform);

            var layout = mainRoot.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 24f;

            var title = UiFactory.CreateText(mainRoot.transform, "Title", "DomiNox", 56, TextAnchor.MiddleCenter);
            title.color = new Color(0.95f, 0.85f, 0.42f);

            var subtitle = UiFactory.CreateText(mainRoot.transform, "Subtitle", "Phase 1 Core Prototype", 24, TextAnchor.MiddleCenter);
            subtitle.color = new Color(0.82f, 0.86f, 0.92f);

            var devMode = UiFactory.CreateText(mainRoot.transform, "DevMode", DevMode.Enabled ? "DEV MODE ACTIF" : string.Empty, 15, TextAnchor.MiddleCenter);
            devMode.color = new Color(1f, 0.45f, 0.25f);

            var playButton = UiFactory.CreateButton(mainRoot.transform, "PlayButton", "Jouer");
            playButton.GetComponent<LayoutElement>().preferredWidth = 220f;
            playButton.GetComponent<LayoutElement>().preferredHeight = 64f;
            playButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Game));

            var collectionButton = UiFactory.CreateButton(mainRoot.transform, "CollectionButton", "Collection");
            collectionButton.GetComponent<LayoutElement>().preferredWidth = 220f;
            collectionButton.GetComponent<LayoutElement>().preferredHeight = 54f;
            collectionButton.onClick.AddListener(ShowCollection);

            BuildCollection(canvas.transform);
            ShowMainMenu();
        }

        private void BuildCollection(Transform canvas)
        {
            collectionRoot = new GameObject("CollectionRoot", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            collectionRoot.transform.SetParent(canvas, false);
            Stretch((RectTransform)collectionRoot.transform);
            collectionRoot.GetComponent<Image>().color = new Color(0.035f, 0.045f, 0.06f, 1f);

            var rootLayout = collectionRoot.GetComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(36, 36, 24, 28);
            rootLayout.spacing = 14f;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            header.transform.SetParent(collectionRoot.transform, false);
            header.GetComponent<LayoutElement>().preferredHeight = 58f;
            var headerLayout = header.GetComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 12f;
            headerLayout.childAlignment = TextAnchor.MiddleCenter;

            var title = UiFactory.CreateText(header.transform, "Title", "Collection", 34, TextAnchor.MiddleLeft);
            title.color = new Color(0.95f, 0.84f, 0.36f);
            title.GetComponent<LayoutElement>().flexibleWidth = 1f;

            var back = UiFactory.CreateButton(header.transform, "Back", "Retour");
            back.GetComponent<LayoutElement>().preferredWidth = 130f;
            back.onClick.AddListener(ShowMainMenu);

            var tabs = new GameObject("Tabs", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            tabs.transform.SetParent(collectionRoot.transform, false);
            tabs.GetComponent<LayoutElement>().preferredHeight = 46f;
            var tabsLayout = tabs.GetComponent<HorizontalLayoutGroup>();
            tabsLayout.spacing = 10f;
            tabsLayout.childForceExpandWidth = false;

            dominexTab = CreateTab(tabs.transform, "DomiNex", () => ShowCollectionTab(CollectionTab.DomiNex));
            bossesTab = CreateTab(tabs.transform, "Boss", () => ShowCollectionTab(CollectionTab.Bosses));
            patternsTab = CreateTab(tabs.transform, "Patterns", () => ShowCollectionTab(CollectionTab.Patterns));
            systemTab = CreateTab(tabs.transform, "Systeme", () => ShowCollectionTab(CollectionTab.System));

            var scrollRoot = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(LayoutElement));
            scrollRoot.transform.SetParent(collectionRoot.transform, false);
            scrollRoot.GetComponent<LayoutElement>().flexibleHeight = 1f;
            scrollRoot.GetComponent<LayoutElement>().preferredHeight = 660f;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollRoot.transform, false);
            Stretch((RectTransform)viewport.transform);
            viewport.GetComponent<Image>().color = new Color(0.06f, 0.075f, 0.1f, 0.92f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            collectionContent = content.transform;
            var contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = Vector2.one;
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var contentLayout = content.GetComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(18, 18, 18, 24);
            contentLayout.spacing = 10f;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollRoot.GetComponent<ScrollRect>();
            scroll.viewport = (RectTransform)viewport.transform;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 28f;
        }

        private Button CreateTab(Transform parent, string label, Action clicked)
        {
            var button = UiFactory.CreateButton(parent, $"{label}Tab", label);
            button.GetComponent<LayoutElement>().preferredWidth = 150f;
            button.GetComponent<LayoutElement>().preferredHeight = 42f;
            button.onClick.AddListener(() => clicked());
            return button;
        }

        private void ShowMainMenu()
        {
            mainRoot.SetActive(true);
            collectionRoot.SetActive(false);
        }

        private void ShowCollection()
        {
            mainRoot.SetActive(false);
            collectionRoot.SetActive(true);
            ShowCollectionTab(CollectionTab.DomiNex);
        }

        private void ShowCollectionTab(CollectionTab tab)
        {
            ClearCollectionContent();
            SetTabColor(dominexTab, tab == CollectionTab.DomiNex);
            SetTabColor(bossesTab, tab == CollectionTab.Bosses);
            SetTabColor(patternsTab, tab == CollectionTab.Patterns);
            SetTabColor(systemTab, tab == CollectionTab.System);

            switch (tab)
            {
                case CollectionTab.DomiNex:
                    RenderDomiNexCollection();
                    break;
                case CollectionTab.Bosses:
                    RenderBossCollection();
                    break;
                case CollectionTab.Patterns:
                    RenderPatternCollection();
                    break;
                case CollectionTab.System:
                    RenderSystemCollection();
                    break;
            }
        }

        private void RenderDomiNexCollection()
        {
            AddIntro("DomiNex", "Toutes les cartes DomiNex connues du prototype. Les cartes desactivees ou futures restent visibles dans le carnet, mais ne sont pas forcement dans le shop.");
            foreach (var dominex in DomiNexRegistry.All.OrderBy(item => item.Rarity).ThenBy(item => item.Name))
            {
                var status = DomiNexRegistry.TemporarilyDisabledIds.Contains(dominex.Id)
                    ? "DESACTIVE TEMP"
                    : DomiNexRegistry.IsAvailableInPrototypeShop(dominex) ? "SHOP" : "FUTUR";
                AddCard(dominex.Name, $"{dominex.Rarity}  |  {status}  |  {string.Join(", ", dominex.Tags)}\n{dominex.Description}");
            }
        }

        private void RenderBossCollection()
        {
            AddIntro("Boss", "Les boss apparaissent tous les 5 niveaux et sont tires aleatoirement dans ce pool.");
            foreach (var boss in BossRegistry.DemoBosses)
            {
                AddCard(boss.Name, $"{boss.RuleType}  |  Quota x{boss.QuotaMultiplier:0.##}\n{boss.Description}");
            }
        }

        private void RenderPatternCollection()
        {
            AddIntro("Patterns", "Les patterns sont detectes au scoring. Le meilleur pattern de valeur et le meilleur pattern de design sont retenus.");
            foreach (var pattern in PatternCatalog.All.OrderBy(pattern => pattern.Category).ThenByDescending(pattern => pattern.Priority))
            {
                AddCard(pattern.Name, $"{pattern.Category}  |  Priorite {pattern.Priority}  |  {pattern.Effect}\n{pattern.Requirement}");
            }
        }

        private void RenderSystemCollection()
        {
            AddIntro("Systeme", "Valeurs de run et outils globaux du prototype.");
            AddCard("Dev Mode", DevMode.Enabled ? "Actif. Les futures fonctions de debug seront branchees sur DomiNox.Core.DevMode." : "Inactif.");
            AddCard("Round", $"Main {GameConstants.StartingHandSize}  |  Pose max {GameConstants.PhaseOneMaxPlacedDominoes}  |  Discards {GameConstants.PhaseOneDiscards}");
            AddCard("Economie", $"Credits depart {GameConstants.StartingCredits}  |  Gain niveau {GameConstants.LevelWinCredits}  |  Interets max {GameConstants.MaxInterestCredits}");
            AddCard("Grille", $"{GameConstants.GridWidth}x{GameConstants.GridHeight}  |  Dominos 0-{GameConstants.DominoMaxValue}");
        }

        private void AddIntro(string title, string body)
        {
            AddCard(title, body, 92f, new Color(0.11f, 0.13f, 0.18f, 1f), new Color(0.95f, 0.84f, 0.36f));
        }

        private void AddCard(string title, string body, float height = 108f, Color? backgroundColor = null, Color? titleColor = null)
        {
            var card = new GameObject($"Card_{title}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            card.transform.SetParent(collectionContent, false);
            card.GetComponent<Image>().color = backgroundColor ?? new Color(0.085f, 0.1f, 0.13f, 0.98f);
            var outline = card.GetComponent<Outline>();
            outline.effectColor = new Color(0.18f, 0.24f, 0.32f);
            outline.effectDistance = new Vector2(2f, -2f);
            card.GetComponent<LayoutElement>().preferredHeight = height;

            var layout = card.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 10, 10);
            layout.spacing = 6f;

            var titleText = UiFactory.CreateText(card.transform, "Title", title, 18, TextAnchor.MiddleLeft);
            titleText.color = titleColor ?? new Color(0.96f, 0.88f, 0.52f);
            titleText.GetComponent<LayoutElement>().preferredHeight = 24f;

            var bodyText = UiFactory.CreateText(card.transform, "Body", body, 13, TextAnchor.UpperLeft);
            bodyText.color = new Color(0.8f, 0.84f, 0.9f);
            bodyText.GetComponent<LayoutElement>().flexibleHeight = 1f;
        }

        private void ClearCollectionContent()
        {
            foreach (Transform child in collectionContent)
            {
                Destroy(child.gameObject);
            }
        }

        private static void SetTabColor(Button button, bool active)
        {
            button.GetComponent<Image>().color = active ? new Color(0.28f, 0.36f, 0.52f) : new Color(0.18f, 0.22f, 0.28f);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            }
        }

        private enum CollectionTab
        {
            DomiNex,
            Bosses,
            Patterns,
            System
        }
    }
}
