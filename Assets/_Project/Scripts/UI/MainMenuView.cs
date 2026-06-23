using System;
using System.Collections.Generic;
using System.Linq;
using DomiNox.Bosses;
using DomiNox.Consumables;
using DomiNox.Core;
using DomiNox.Dominex;
using DomiNox.Objectives;
using DomiNox.Patterns;
using DomiNox.Persistence;
using DomiNox.Run;
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
        private Text pageTitle;
        private Text pageProgress;
        private Text sidebarProgress;
        private readonly Dictionary<CollectionTab, Button> tabButtons = new Dictionary<CollectionTab, Button>();
        private readonly Dictionary<CollectionTab, Text> tabCounts = new Dictionary<CollectionTab, Text>();
        private MetaProfile profile = new MetaProfile();
        private CollectionTab currentTab = CollectionTab.Progression;
        private Text devStatus;

        // Row-batching state for the responsive card grid.
        private Transform currentRow;
        private int currentRowCount;
        private int currentRowColumns = 2;
        private float currentCardHeight = 96f;

        private void Start()
        {
            EnsureEventSystem();
            var canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvasComponent = canvas.GetComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            UiFactory.ConfigureCanvasScaler(canvas.GetComponent<CanvasScaler>());

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)bg.transform);
            bg.GetComponent<Image>().color = DomiNoxTheme.BgDeep;

            mainRoot = new GameObject("MainMenuRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            mainRoot.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)mainRoot.transform);
            var layout = mainRoot.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 22f;

            var title = UiFactory.CreateText(mainRoot.transform, "Title", "DomiNox", 64, TextAnchor.MiddleCenter);
            title.color = DomiNoxTheme.Gold;
            title.fontStyle = FontStyle.Bold;
            DomiNoxTheme.AddShadow(title.gameObject, new Color(0f, 0f, 0f, 0.7f), new Vector2(3f, -3f));
            title.GetComponent<LayoutElement>().preferredHeight = 86f;

            var subtitle = UiFactory.CreateText(mainRoot.transform, "Subtitle", "Phase 1 Core Prototype", DomiNoxTheme.FontLG, TextAnchor.MiddleCenter);
            subtitle.color = DomiNoxTheme.TextSecondary;
            subtitle.GetComponent<LayoutElement>().preferredHeight = 34f;

            UiFactory.CreateSeparator(mainRoot.transform, 1f, DomiNoxTheme.WithAlpha(DomiNoxTheme.Gold, 0.28f));

            var devMode = UiFactory.CreateText(mainRoot.transform, "DevMode", DevMode.Enabled ? "DEV MODE ACTIF" : string.Empty, DomiNoxTheme.FontSM, TextAnchor.MiddleCenter);
            devMode.color = DomiNoxTheme.Danger;
            devMode.fontStyle = FontStyle.Bold;
            devMode.GetComponent<LayoutElement>().preferredHeight = devMode.text.Length > 0 ? 24f : 0f;

            var playButton = UiFactory.CreateButton(mainRoot.transform, "PlayButton", "Jouer");
            playButton.GetComponent<LayoutElement>().preferredWidth  = 240f;
            playButton.GetComponent<LayoutElement>().preferredHeight = 72f;
            UiFactory.StyleButton(playButton, new Color(0.06f, 0.26f, 0.10f), DomiNoxTheme.Success, 72f);
            var playLabel = playButton.GetComponentInChildren<Text>();
            if (playLabel != null)
            {
                playLabel.fontSize = 26;
                playLabel.fontStyle = FontStyle.Bold;
            }
            DomiNoxTheme.AddShadow(playButton.gameObject, new Color(0f, 0f, 0f, 0.4f), new Vector2(2f, -2f));
            playButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Game));

            var collectionButton = UiFactory.CreateButton(mainRoot.transform, "CollectionButton", "Collection");
            collectionButton.GetComponent<LayoutElement>().preferredWidth  = 240f;
            collectionButton.GetComponent<LayoutElement>().preferredHeight = 54f;
            UiFactory.StyleButton(collectionButton, DomiNoxTheme.BgCard, DomiNoxTheme.TextPrimary, 54f);
            collectionButton.onClick.AddListener(ShowCollection);

            BuildCollection(canvas.transform);
            if (DevMode.Enabled)
            {
                BuildDevCheats(canvas.transform);
            }

            ShowMainMenu();
        }

        // ----------------------------------------------------------------------------------
        // Dev cheats (only when DevMode.Enabled): quick save/profile manipulation for testing.
        // ----------------------------------------------------------------------------------

        private void BuildDevCheats(Transform canvas)
        {
            var panel = new GameObject("DevCheats", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            panel.transform.SetParent(canvas, false);
            var r = (RectTransform)panel.transform;
            r.anchorMin = new Vector2(0f, 0f);
            r.anchorMax = new Vector2(0f, 0f);
            r.pivot = new Vector2(0f, 0f);
            r.anchoredPosition = new Vector2(18f, 18f);
            r.sizeDelta = new Vector2(244f, 232f);
            panel.GetComponent<Image>().color = DomiNoxTheme.WithAlpha(DomiNoxTheme.BgCard, 0.96f);
            DomiNoxTheme.AddOutline(panel, DomiNoxTheme.WithAlpha(DomiNoxTheme.Danger, 0.7f), 1.5f);
            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 10);
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var title = UiFactory.CreateText(panel.transform, "DevTitle", "DEV CHEATS", DomiNoxTheme.FontSM, TextAnchor.MiddleCenter);
            title.color = DomiNoxTheme.Danger;
            title.fontStyle = FontStyle.Bold;
            title.GetComponent<LayoutElement>().preferredHeight = 22f;

            devStatus = UiFactory.CreateText(panel.transform, "DevStatus", "Pret.", DomiNoxTheme.FontXS, TextAnchor.MiddleCenter);
            devStatus.color = DomiNoxTheme.TextSecondary;
            devStatus.GetComponent<LayoutElement>().preferredHeight = 18f;

            AddCheatButton(panel.transform, "Tout debloquer", DomiNoxTheme.Success, CheatUnlockAll);
            AddCheatButton(panel.transform, "Reset save (0 unlock)", DomiNoxTheme.Warning, CheatResetProfile);
            AddCheatButton(panel.transform, "Debloquer Boucle 2", DomiNoxTheme.CountBlue, CheatUnlockEndless);
            AddCheatButton(panel.transform, "Supprimer run en cours", DomiNoxTheme.Danger, CheatDeleteRun);
        }

        private void AddCheatButton(Transform parent, string label, Color color, Action action)
        {
            var button = UiFactory.CreateButton(parent, $"Cheat_{label}", label);
            button.GetComponent<LayoutElement>().preferredHeight = 34f;
            UiFactory.StyleButton(button, DomiNoxTheme.BgPanel, color, 34f);
            var lbl = button.GetComponentInChildren<Text>();
            if (lbl != null) lbl.fontSize = DomiNoxTheme.FontXS;
            button.onClick.AddListener(() => action());
        }

        private void CheatUnlockAll()
        {
            var unlocked = new MetaProfile { endlessUnlocked = true, bestFloorReached = 3, bestLevelReached = 15 };
            foreach (var definition in DomiNexRegistry.All)
            {
                unlocked.discoveredDomiNexIds.Add(definition.Id);
                if (DomiNexUnlockService.IsLockedByDefault(definition.Id))
                {
                    unlocked.unlockedDomiNexIds.Add(definition.Id);
                }
            }

            foreach (var objective in ObjectiveRegistry.All)
            {
                unlocked.completedObjectiveIds.Add(objective.Id);
            }

            foreach (var boss in BossRegistry.DemoBosses)
            {
                unlocked.encounteredBossIds.Add(boss.Id);
            }

            MetaProfileService.Save(unlocked);
            profile = unlocked;
            RefreshTabCounts();
            ShowCollectionTab(currentTab);
            SetDevStatus("Tout debloque.");
        }

        private void CheatResetProfile()
        {
            MetaProfileService.Save(new MetaProfile());
            RunSaveService.Delete();
            profile = new MetaProfile();
            RefreshTabCounts();
            ShowCollectionTab(currentTab);
            SetDevStatus("Save reset, 0 unlock.");
        }

        private void CheatUnlockEndless()
        {
            var loaded = MetaProfileService.Load();
            loaded.endlessUnlocked = true;
            if (!loaded.unlockedDomiNexIds.Contains(ObjectiveRegistry.SchismDomiNexId))
            {
                loaded.unlockedDomiNexIds.Add(ObjectiveRegistry.SchismDomiNexId);
            }

            var floorObjective = ObjectiveRegistry.GetObjectiveForReward(ObjectiveRegistry.SchismDomiNexId);
            if (floorObjective != null && !loaded.completedObjectiveIds.Contains(floorObjective.Id))
            {
                loaded.completedObjectiveIds.Add(floorObjective.Id);
            }

            MetaProfileService.Save(loaded);
            profile = loaded;
            RefreshTabCounts();
            SetDevStatus("Boucle 2 + Schisme debloques.");
        }

        private void CheatDeleteRun()
        {
            RunSaveService.Delete();
            SetDevStatus("Run en cours supprimee.");
        }

        private void SetDevStatus(string message)
        {
            if (devStatus != null)
            {
                devStatus.text = message;
            }
        }

        // ----------------------------------------------------------------------------------
        // Collection codex. Stretches to the screen (with margins) so it never overflows, and
        // lays cards out in responsive rows that share the page width (no horizontal cropping).
        // ----------------------------------------------------------------------------------

        private void BuildCollection(Transform canvas)
        {
            collectionRoot = new GameObject("CollectionRoot", typeof(RectTransform), typeof(Image));
            collectionRoot.transform.SetParent(canvas, false);
            Stretch((RectTransform)collectionRoot.transform);
            collectionRoot.GetComponent<Image>().color = DomiNoxTheme.BgDeep;

            // Codex panel: anchored stretch with inset margins -> always fits the screen.
            // Everything inside is anchored manually (no layout group) so the title bar stays a
            // thin strip and the body fills the rest precisely.
            var codex = new GameObject("Codex", typeof(RectTransform), typeof(Image), typeof(Outline));
            codex.transform.SetParent(collectionRoot.transform, false);
            var codexRect = (RectTransform)codex.transform;
            codexRect.anchorMin = Vector2.zero;
            codexRect.anchorMax = Vector2.one;
            codexRect.offsetMin = new Vector2(40f, 28f);
            codexRect.offsetMax = new Vector2(-40f, -26f);
            codex.GetComponent<Image>().color = DomiNoxTheme.BgPanel;
            var codexOutline = codex.GetComponent<Outline>();
            codexOutline.effectColor = DomiNoxTheme.WithAlpha(DomiNoxTheme.Gold, 0.55f);
            codexOutline.effectDistance = new Vector2(2f, -2f);

            const float pad = 22f;
            const float titleH = 52f;
            const float gap1 = 8f;
            const float sepH = 2f;
            const float gap2 = 12f;
            var topConsumed = pad + titleH + gap1 + sepH + gap2;

            // Title bar strip pinned to the top.
            var titleBar = new GameObject("TitleBar", typeof(RectTransform));
            titleBar.transform.SetParent(codex.transform, false);
            var tr = (RectTransform)titleBar.transform;
            tr.anchorMin = new Vector2(0f, 1f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.offsetMin = new Vector2(pad, -(pad + titleH));
            tr.offsetMax = new Vector2(-pad, -pad);
            BuildTitleBar(titleBar.transform);

            // Separator under the title bar.
            var sep = new GameObject("TopSep", typeof(RectTransform), typeof(Image));
            sep.transform.SetParent(codex.transform, false);
            var spr = (RectTransform)sep.transform;
            spr.anchorMin = new Vector2(0f, 1f);
            spr.anchorMax = new Vector2(1f, 1f);
            spr.pivot = new Vector2(0.5f, 1f);
            spr.offsetMin = new Vector2(pad, -(pad + titleH + gap1 + sepH));
            spr.offsetMax = new Vector2(-pad, -(pad + titleH + gap1));
            sep.GetComponent<Image>().color = DomiNoxTheme.WithAlpha(DomiNoxTheme.Gold, 0.45f);

            // Body fills everything below the separator. Sidebar + page anchor inside it.
            var body = new GameObject("Body", typeof(RectTransform));
            body.transform.SetParent(codex.transform, false);
            var br = (RectTransform)body.transform;
            br.anchorMin = new Vector2(0f, 0f);
            br.anchorMax = new Vector2(1f, 1f);
            br.offsetMin = new Vector2(pad, pad);
            br.offsetMax = new Vector2(-pad, -topConsumed);

            BuildSidebar(body.transform);
            BuildPage(body.transform);
        }

        private void BuildTitleBar(Transform parent)
        {
            var book = UiFactory.CreateText(parent, "BookTitle", "CARNET", DomiNoxTheme.FontXXL, TextAnchor.MiddleLeft);
            book.color = DomiNoxTheme.Gold;
            book.fontStyle = FontStyle.Bold;
            DomiNoxTheme.AddShadow(book.gameObject, new Color(0f, 0f, 0f, 0.5f), new Vector2(2f, -2f));
            var bookRect = book.rectTransform;
            bookRect.anchorMin = new Vector2(0f, 0f);
            bookRect.anchorMax = new Vector2(0f, 1f);
            bookRect.pivot = new Vector2(0f, 0.5f);
            bookRect.sizeDelta = new Vector2(232f, 0f);
            bookRect.anchoredPosition = Vector2.zero;

            var subtitle = UiFactory.CreateText(parent, "BookSub", "Tout ce que tu as decouvert et ce qu'il te reste a debloquer.", DomiNoxTheme.FontSM, TextAnchor.MiddleLeft);
            subtitle.color = DomiNoxTheme.TextSecondary;
            var subRect = subtitle.rectTransform;
            subRect.anchorMin = new Vector2(0f, 0f);
            subRect.anchorMax = new Vector2(1f, 1f);
            subRect.offsetMin = new Vector2(246f, 0f);
            subRect.offsetMax = new Vector2(-152f, 0f);

            var back = UiFactory.CreateButton(parent, "Back", "Retour");
            UiFactory.StyleButton(back, DomiNoxTheme.BgCard, DomiNoxTheme.Gold, 42f);
            var backRect = (RectTransform)back.transform;
            backRect.anchorMin = new Vector2(1f, 0.5f);
            backRect.anchorMax = new Vector2(1f, 0.5f);
            backRect.pivot = new Vector2(1f, 0.5f);
            backRect.sizeDelta = new Vector2(132f, 42f);
            backRect.anchoredPosition = Vector2.zero;
            back.onClick.AddListener(ShowMainMenu);
        }

        private void BuildSidebar(Transform parent)
        {
            var sidebar = new GameObject("Sidebar", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            sidebar.transform.SetParent(parent, false);
            var sr = (RectTransform)sidebar.transform;
            sr.anchorMin = new Vector2(0f, 0f);
            sr.anchorMax = new Vector2(0f, 1f);
            sr.pivot = new Vector2(0f, 0.5f);
            sr.sizeDelta = new Vector2(196f, 0f);
            sr.anchoredPosition = Vector2.zero;
            sidebar.GetComponent<Image>().color = DomiNoxTheme.BgPanel;
            DomiNoxTheme.AddOutline(sidebar, DomiNoxTheme.WithAlpha(DomiNoxTheme.BorderNormal, 0.8f), 1f);
            var layout = sidebar.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(9, 9, 10, 10);
            layout.spacing = 4f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateBookmark(sidebar.transform, CollectionTab.Progression, "Progression");
            CreateBookmark(sidebar.transform, CollectionTab.DomiNex, "DomiNex");
            CreateBookmark(sidebar.transform, CollectionTab.Objectives, "Objectifs");
            CreateBookmark(sidebar.transform, CollectionTab.Bosses, "Boss");
            CreateBookmark(sidebar.transform, CollectionTab.Patterns, "Patterns");
            CreateBookmark(sidebar.transform, CollectionTab.Combos, "Combos");
            CreateBookmark(sidebar.transform, CollectionTab.Gems, "Gemmes");
            CreateBookmark(sidebar.transform, CollectionTab.System, "Systeme");

            var spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(sidebar.transform, false);
            spacer.GetComponent<LayoutElement>().flexibleHeight = 1f;

            sidebarProgress = UiFactory.CreateText(sidebar.transform, "SidebarProgress", string.Empty, DomiNoxTheme.FontXS, TextAnchor.MiddleCenter);
            sidebarProgress.color = DomiNoxTheme.GoldDim;
            sidebarProgress.GetComponent<LayoutElement>().preferredHeight = 38f;
        }

        private void CreateBookmark(Transform parent, CollectionTab tab, string label)
        {
            var button = UiFactory.CreateButton(parent, $"Tab_{tab}", label);
            var le = button.GetComponent<LayoutElement>();
            le.preferredHeight = 37f;
            le.flexibleWidth = 1f;
            var lbl = button.GetComponentInChildren<Text>();
            if (lbl != null)
            {
                lbl.alignment = TextAnchor.MiddleLeft;
                lbl.rectTransform.offsetMin = new Vector2(14f, 0f);
                lbl.rectTransform.offsetMax = new Vector2(-44f, 0f);
            }

            var count = UiFactory.CreateText(button.transform, "Count", string.Empty, DomiNoxTheme.FontXS, TextAnchor.MiddleRight);
            count.color = DomiNoxTheme.TextMuted;
            count.raycastTarget = false;
            var cr = count.rectTransform;
            cr.anchorMin = Vector2.zero;
            cr.anchorMax = Vector2.one;
            cr.offsetMin = new Vector2(0f, 0f);
            cr.offsetMax = new Vector2(-12f, 0f);

            button.onClick.AddListener(() => ShowCollectionTab(tab));
            tabButtons[tab] = button;
            tabCounts[tab] = count;
        }

        private void BuildPage(Transform parent)
        {
            // Page fills everything to the right of the sidebar (196 width + 16 gap = 212 inset).
            var page = new GameObject("Page", typeof(RectTransform));
            page.transform.SetParent(parent, false);
            var pr = (RectTransform)page.transform;
            pr.anchorMin = new Vector2(0f, 0f);
            pr.anchorMax = new Vector2(1f, 1f);
            pr.offsetMin = new Vector2(212f, 0f);
            pr.offsetMax = new Vector2(0f, 0f);

            // Header strip pinned to the top of the page.
            var header = new GameObject("PageHeader", typeof(RectTransform));
            header.transform.SetParent(page.transform, false);
            var hr = (RectTransform)header.transform;
            hr.anchorMin = new Vector2(0f, 1f);
            hr.anchorMax = new Vector2(1f, 1f);
            hr.pivot = new Vector2(0.5f, 1f);
            hr.offsetMin = new Vector2(0f, -42f);
            hr.offsetMax = new Vector2(0f, 0f);

            pageTitle = UiFactory.CreateText(header.transform, "PageTitle", string.Empty, DomiNoxTheme.FontXL, TextAnchor.MiddleLeft);
            pageTitle.color = DomiNoxTheme.Gold;
            pageTitle.fontStyle = FontStyle.Bold;
            AnchorStretch(pageTitle.rectTransform, new Vector2(0f, 0f), new Vector2(0.6f, 1f));

            pageProgress = UiFactory.CreateText(header.transform, "PageProgress", string.Empty, DomiNoxTheme.FontSM, TextAnchor.MiddleRight);
            pageProgress.color = DomiNoxTheme.TextSecondary;
            AnchorStretch(pageProgress.rectTransform, new Vector2(0.5f, 0f), new Vector2(1f, 1f));

            // Thin separator just below the header.
            var sep = new GameObject("PageSep", typeof(RectTransform), typeof(Image));
            sep.transform.SetParent(page.transform, false);
            var spr = (RectTransform)sep.transform;
            spr.anchorMin = new Vector2(0f, 1f);
            spr.anchorMax = new Vector2(1f, 1f);
            spr.pivot = new Vector2(0.5f, 1f);
            spr.offsetMin = new Vector2(0f, -48f);
            spr.offsetMax = new Vector2(0f, -47f);
            sep.GetComponent<Image>().color = DomiNoxTheme.WithAlpha(DomiNoxTheme.GoldDim, 0.45f);

            // Scroll area fills the rest of the page below the header.
            var scrollRoot = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            scrollRoot.transform.SetParent(page.transform, false);
            var scr = (RectTransform)scrollRoot.transform;
            scr.anchorMin = new Vector2(0f, 0f);
            scr.anchorMax = new Vector2(1f, 1f);
            scr.offsetMin = new Vector2(0f, 0f);
            scr.offsetMax = new Vector2(0f, -54f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewport.transform.SetParent(scrollRoot.transform, false);
            Stretch((RectTransform)viewport.transform);
            viewport.GetComponent<Image>().color = DomiNoxTheme.WithAlpha(DomiNoxTheme.BgDeep, 0.5f);

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            collectionContent = content.transform;
            var contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            var cl = content.GetComponent<VerticalLayoutGroup>();
            cl.padding = new RectOffset(14, 14, 14, 18);
            cl.spacing = 10f;
            cl.childControlWidth = true;
            cl.childControlHeight = true;
            cl.childForceExpandWidth = true;
            cl.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollRoot.GetComponent<ScrollRect>();
            scroll.viewport = (RectTransform)viewport.transform;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.scrollSensitivity = 32f;
            scroll.movementType = ScrollRect.MovementType.Clamped;
        }

        private static void AnchorStretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void ShowMainMenu()
        {
            mainRoot.SetActive(true);
            collectionRoot.SetActive(false);
        }

        private void ShowCollection()
        {
            profile = MetaProfileService.Load();
            mainRoot.SetActive(false);
            collectionRoot.SetActive(true);
            RefreshTabCounts();
            ShowCollectionTab(currentTab);
        }

        private void RefreshTabCounts()
        {
            var domiNexTotal = DomiNexRegistry.All.Count;
            var domiNexUnlocked = CollectionService.CountUnlocked(profile);
            var objTotal = ObjectiveRegistry.All.Count;
            var objDone = ObjectiveRegistry.All.Count(o => profile.completedObjectiveIds.Contains(o.Id));

            tabCounts[CollectionTab.DomiNex].text = $"{domiNexUnlocked}/{domiNexTotal}";
            tabCounts[CollectionTab.Objectives].text = $"{objDone}/{objTotal}";
            tabCounts[CollectionTab.Bosses].text = BossRegistry.DemoBosses.Length.ToString();
            tabCounts[CollectionTab.Patterns].text = PatternCatalog.All.Count.ToString();
            tabCounts[CollectionTab.Combos].text = PatternComboCatalog.All.Count.ToString();
            tabCounts[CollectionTab.Gems].text = GemTileRegistry.All.Count.ToString();

            sidebarProgress.text = $"DomiNex {domiNexUnlocked}/{domiNexTotal}\nObjectifs {objDone}/{objTotal}";
        }

        private void ShowCollectionTab(CollectionTab tab)
        {
            currentTab = tab;
            ClearCollectionContent();
            currentRow = null;
            pageProgress.text = string.Empty;
            foreach (var pair in tabButtons)
            {
                SetBookmarkActive(pair.Value, pair.Key == tab);
            }

            switch (tab)
            {
                case CollectionTab.Progression: pageTitle.text = "Progression"; RenderProgression();        break;
                case CollectionTab.DomiNex:     pageTitle.text = "DomiNex";      RenderDomiNexCollection();   break;
                case CollectionTab.Objectives:  pageTitle.text = "Objectifs";    RenderObjectives();          break;
                case CollectionTab.Bosses:      pageTitle.text = "Boss";         RenderBossCollection();      break;
                case CollectionTab.Patterns:    pageTitle.text = "Patterns";     RenderPatternCollection();   break;
                case CollectionTab.Combos:      pageTitle.text = "Combos";       RenderComboCollection();     break;
                case CollectionTab.Gems:        pageTitle.text = "Gemmes";       RenderConsumableCollection();break;
                case CollectionTab.System:      pageTitle.text = "Systeme";      RenderSystemCollection();    break;
            }
        }

        private void RenderProgression()
        {
            var domiNexTotal = DomiNexRegistry.All.Count;
            var domiNexUnlocked = CollectionService.CountUnlocked(profile);
            var objTotal = ObjectiveRegistry.All.Count;
            var objDone = ObjectiveRegistry.All.Count(o => profile.completedObjectiveIds.Contains(o.Id));
            pageProgress.text = $"Etage record {profile.bestFloorReached}";

            AddSectionHeader("Vue d'ensemble", "Ta progression a travers les runs.");
            BeginCardSection(2, 104f);
            AddCard("DomiNex debloques", $"{domiNexUnlocked} / {domiNexTotal}\nDes cartes se debloquent en accomplissant des objectifs.", DomiNoxTheme.Gold, "COLLECTION");
            AddCard("Objectifs accomplis", $"{objDone} / {objTotal}\nVois l'onglet Objectifs pour les indices.", DomiNoxTheme.Success, "DEFIS");
            AddCard("Etage record", $"Etage {profile.bestFloorReached}  -  Niveau {profile.bestLevelReached}", DomiNoxTheme.CountBlue, "RECORD");
            AddCard("Boucle 2",
                profile.endlessUnlocked
                    ? "Debloquee. Tu peux depasser le 3e etage et poser une 2e region."
                    : "Verrouillee. Termine le 3e etage pour debloquer Le Schisme.",
                profile.endlessUnlocked ? DomiNoxTheme.Success : DomiNoxTheme.TextMuted,
                profile.endlessUnlocked ? "OUVERTE" : "VERROUILLEE");
            AddCard("Patterns secrets", $"{profile.discoveredSecretPatternIds.Count} decouverts", DomiNoxTheme.RarityEpic, "SECRETS");
            AddCard("Boss rencontres", $"{profile.encounteredBossIds.Count} / {BossRegistry.DemoBosses.Length}", DomiNoxTheme.MultRed, "BOSS");
        }

        private void RenderDomiNexCollection()
        {
            var entries = CollectionService.BuildEntries(profile, null);
            var unlocked = entries.Count(entry => entry.Status != CollectionStatus.Locked);
            pageProgress.text = $"{unlocked} / {entries.Count} debloques";

            AddSectionHeader("DomiNex", "Cartes joker. Les verrouillees se debloquent via les objectifs.");
            BeginCardSection(2, 104f);
            foreach (var entry in entries
                .OrderBy(entry => entry.Status == CollectionStatus.Locked ? 1 : 0)
                .ThenBy(entry => entry.Rarity)
                .ThenBy(entry => entry.Name))
            {
                if (entry.Status == CollectionStatus.Locked)
                {
                    AddCard("? ? ? ?", $"Carte verrouillee.\nIndice : {entry.Hint}", DomiNoxTheme.TextMuted, "VERROUILLE", dimmed: true);
                    continue;
                }

                var def = DomiNexRegistry.GetById(entry.DomiNexId);
                AddCard(entry.Name, def?.Description ?? string.Empty, DomiNoxTheme.GetRarityColor(entry.Rarity),
                    $"{entry.Rarity.ToString().ToUpperInvariant()}  -  {StatusLabel(entry.Status)}");
            }
        }

        private void RenderObjectives()
        {
            var done = ObjectiveRegistry.All.Count(objective => profile.completedObjectiveIds.Contains(objective.Id));
            pageProgress.text = $"{done} / {ObjectiveRegistry.All.Count} accomplis";

            AddSectionHeader("Objectifs", "Accomplis-les pour debloquer de nouveaux DomiNex.");
            BeginCardSection(2, 100f);
            foreach (var objective in ObjectiveRegistry.All)
            {
                var completed = profile.completedObjectiveIds.Contains(objective.Id);
                if (completed)
                {
                    var reward = DomiNexRegistry.GetById(objective.RewardDomiNexId);
                    AddCard(objective.Name, $"Accompli.\nRecompense debloquee : {reward?.Name ?? objective.RewardDomiNexId}", DomiNoxTheme.Success, "ACCOMPLI");
                }
                else
                {
                    AddCard(objective.Hidden ? "Objectif secret" : objective.Name, $"{objective.DisplayHint}\nRecompense : ???", DomiNoxTheme.Gold, "EN COURS", dimmed: objective.Hidden);
                }
            }
        }

        private void RenderBossCollection()
        {
            pageProgress.text = $"{BossRegistry.DemoBosses.Length} boss";
            AddSectionHeader("Boss", "Un boss tous les 5 niveaux, tire aleatoirement dans ce pool.");
            BeginCardSection(2, 104f);
            foreach (var boss in BossRegistry.DemoBosses)
            {
                var seen = profile.encounteredBossIds.Contains(boss.Id);
                AddCard(boss.Name, $"Quota x{boss.QuotaMultiplier:0.##}\n{boss.Description}", DomiNoxTheme.MultRed, seen ? "RENCONTRE" : "INCONNU");
            }
        }

        private void RenderPatternCollection()
        {
            var values = PatternCatalog.All
                .Where(pattern => pattern.Category == PatternCategory.Value && CollectableVisibilityService.IsPatternVisible(pattern, null))
                .OrderByDescending(pattern => pattern.Priority)
                .ToList();
            var designs = PatternCatalog.All
                .Where(pattern => pattern.Category == PatternCategory.Design && CollectableVisibilityService.IsPatternVisible(pattern, null))
                .OrderByDescending(pattern => pattern.Priority)
                .ToList();
            pageProgress.text = $"{values.Count + designs.Count} patterns visibles";

            AddSectionHeader("Patterns de valeur", "Bases sur les dominos joues. Un seul est retenu par region.");
            BeginCardSection(2, 96f);
            foreach (var pattern in values)
            {
                AddCard(pattern.Name, $"Priorite {pattern.Priority}  -  {pattern.Effect}\n{pattern.Requirement}", DomiNoxTheme.CountBlue, "VALEUR");
            }

            AddSectionHeader("Patterns de design", "Formes sur la grille. Avec la Faille, chaque region marque le sien.");
            BeginCardSection(2, 96f);
            foreach (var pattern in designs)
            {
                AddCard(pattern.Name, $"Priorite {pattern.Priority}  -  {pattern.Effect}\n{pattern.Requirement}", DomiNoxTheme.Success, "DESIGN");
            }
        }

        private void RenderComboCollection()
        {
            var visible = PatternComboCatalog.All.Where(combo => CollectableVisibilityService.IsComboVisible(combo, null)).ToList();
            pageProgress.text = $"{visible.Count} / {PatternComboCatalog.All.Count} visibles";

            AddSectionHeader("Combos", "Value Pattern + Design Pattern. Les combos secrets suivent la visibilite du design.");
            BeginCardSection(2, 96f);
            foreach (var combo in visible.OrderBy(combo => combo.Difficulty).ThenBy(combo => combo.Name))
            {
                var accent = combo.Difficulty == PatternComboDifficulty.Legendary ? DomiNoxTheme.RarityLegendary : DomiNoxTheme.RarityEpic;
                AddCard(combo.Name,
                    $"{PatternCatalog.GetById(combo.ValuePatternId)?.Name} + {PatternCatalog.GetById(combo.DesignPatternId)?.Name}\n+{combo.CountBonus} Tile, +{combo.MultBonus} Mult",
                    accent, combo.Difficulty.ToString().ToUpperInvariant());
            }
        }

        private void RenderConsumableCollection()
        {
            var visible = GemTileRegistry.All.Where(gem => CollectableVisibilityService.IsConsumableVisible(gem, null)).OrderBy(gem => gem.Name).ToList();
            pageProgress.text = $"{visible.Count} / {GemTileRegistry.All.Count} visibles";

            AddSectionHeader("Gemmes", "Gem Tiles stockables. Elles ameliorent le niveau du pattern cible.");
            BeginCardSection(2, 96f);
            foreach (var consumable in visible)
            {
                var pattern = PatternCatalog.GetById(consumable.TargetPatternId);
                AddCard(consumable.Name, $"Prix ${consumable.Price}  -  Cible : {pattern?.Name ?? consumable.TargetPatternId}\n{consumable.Description}", new Color(0.22f, 0.72f, 0.65f), "GEM");
            }
        }

        private void RenderSystemCollection()
        {
            AddSectionHeader("Systeme", "Constantes de run et outils globaux du prototype.");
            BeginCardSection(2, 88f);
            AddCard("Round", $"Main {GameConstants.StartingHandSize}  -  Pose max {GameConstants.PhaseOneMaxPlacedDominoes}  -  Discards {GameConstants.PhaseOneDiscards}", DomiNoxTheme.CountBlue, "REGLES");
            AddCard("Economie", $"Depart {GameConstants.StartingCredits}  -  Gain niveau {GameConstants.LevelWinCredits}  -  Interets max {GameConstants.MaxInterestCredits}", DomiNoxTheme.Gold, "CREDITS");
            AddCard("Grille", $"{GameConstants.GridWidth}x{GameConstants.GridHeight}  -  Dominos 0-{GameConstants.DominoMaxValue}", DomiNoxTheme.Success, "PLATEAU");
            AddCard("Dev Mode", DevMode.Enabled ? "Actif." : "Inactif.", DevMode.Enabled ? DomiNoxTheme.Danger : DomiNoxTheme.TextMuted, "DEBUG");
        }

        private static string StatusLabel(CollectionStatus status)
        {
            switch (status)
            {
                case CollectionStatus.Owned:      return "POSSEDE";
                case CollectionStatus.Discovered: return "DECOUVERT";
                case CollectionStatus.Available:  return "DISPONIBLE";
                default:                          return "VERROUILLE";
            }
        }

        // --- Card grid (responsive rows) ---------------------------------------------------

        private void AddSectionHeader(string title, string subtitle)
        {
            currentRow = null; // next card starts a fresh row
            var header = new GameObject($"Section_{title}", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            header.transform.SetParent(collectionContent, false);
            header.GetComponent<Image>().color = DomiNoxTheme.WithAlpha(DomiNoxTheme.Gold, 0.10f);
            DomiNoxTheme.AddOutline(header, DomiNoxTheme.WithAlpha(DomiNoxTheme.Gold, 0.4f), 1f);
            header.GetComponent<LayoutElement>().preferredHeight = string.IsNullOrEmpty(subtitle) ? 38f : 54f;
            var layout = header.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 6, 6);
            layout.spacing = 1f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var titleText = UiFactory.CreateText(header.transform, "Title", title, DomiNoxTheme.FontLG, TextAnchor.MiddleLeft);
            titleText.color = DomiNoxTheme.Gold;
            titleText.fontStyle = FontStyle.Bold;
            titleText.GetComponent<LayoutElement>().preferredHeight = 25f;
            if (!string.IsNullOrEmpty(subtitle))
            {
                var sub = UiFactory.CreateText(header.transform, "Sub", subtitle, DomiNoxTheme.FontXS, TextAnchor.MiddleLeft);
                sub.color = DomiNoxTheme.TextSecondary;
                sub.GetComponent<LayoutElement>().preferredHeight = 15f;
            }
        }

        private void BeginCardSection(int columns, float cardHeight)
        {
            currentRow = null;
            currentRowCount = 0;
            currentRowColumns = Mathf.Max(1, columns);
            currentCardHeight = cardHeight;
        }

        private Transform NextCardSlot()
        {
            if (currentRow == null || currentRowCount >= currentRowColumns)
            {
                var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                row.transform.SetParent(collectionContent, false);
                row.GetComponent<LayoutElement>().preferredHeight = currentCardHeight;
                var hl = row.GetComponent<HorizontalLayoutGroup>();
                hl.spacing = 12f;
                hl.childControlWidth = true;
                hl.childControlHeight = true;
                hl.childForceExpandWidth = true;
                hl.childForceExpandHeight = true;
                currentRow = row.transform;
                currentRowCount = 0;
            }

            currentRowCount++;
            return currentRow;
        }

        private void AddCard(string title, string body, Color accent, string badge = null, bool dimmed = false)
        {
            var slot = NextCardSlot();
            var card = new GameObject($"Card_{title}", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            card.transform.SetParent(slot, false);
            var background = Color.Lerp(DomiNoxTheme.BgCard, accent, 0.12f);
            card.GetComponent<Image>().color = dimmed ? DomiNoxTheme.WithAlpha(DomiNoxTheme.BgCard, 0.6f) : background;
            DomiNoxTheme.AddOutline(card, DomiNoxTheme.WithAlpha(accent, dimmed ? 0.3f : 0.75f), 1.5f);
            var layout = card.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(13, 13, 9, 9);
            layout.spacing = 3f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            if (!string.IsNullOrWhiteSpace(badge))
            {
                var badgeText = UiFactory.CreateText(card.transform, "Badge", badge, DomiNoxTheme.FontXS, TextAnchor.MiddleLeft);
                badgeText.color = dimmed ? DomiNoxTheme.TextMuted : accent;
                badgeText.fontStyle = FontStyle.Bold;
                badgeText.GetComponent<LayoutElement>().preferredHeight = 15f;
            }

            var titleText = UiFactory.CreateText(card.transform, "Title", title, DomiNoxTheme.FontMD, TextAnchor.MiddleLeft);
            titleText.color = dimmed ? DomiNoxTheme.TextMuted : DomiNoxTheme.TextPrimary;
            titleText.fontStyle = FontStyle.Bold;
            titleText.GetComponent<LayoutElement>().preferredHeight = 21f;

            var bodyText = UiFactory.CreateText(card.transform, "Body", body, DomiNoxTheme.FontXS, TextAnchor.UpperLeft);
            bodyText.color = dimmed ? DomiNoxTheme.TextMuted : DomiNoxTheme.TextSecondary;
            bodyText.GetComponent<LayoutElement>().flexibleHeight = 1f;
        }

        private void ClearCollectionContent()
        {
            foreach (Transform child in collectionContent) Destroy(child.gameObject);
        }

        private static void SetBookmarkActive(Button button, bool active)
        {
            if (button == null) return;
            var img = button.GetComponent<Image>();
            if (img != null) img.color = active ? DomiNoxTheme.Gold : DomiNoxTheme.BgCard;
            var outline = button.GetComponent<Outline>();
            if (outline != null) outline.effectColor = active ? DomiNoxTheme.GoldDim : DomiNoxTheme.WithAlpha(DomiNoxTheme.BorderNormal, 0.7f);
            var texts = button.GetComponentsInChildren<Text>();
            foreach (var text in texts)
            {
                if (text.name == "Count")
                {
                    text.color = active ? DomiNoxTheme.WithAlpha(DomiNoxTheme.BgDeep, 0.75f) : DomiNoxTheme.TextMuted;
                }
                else
                {
                    text.color = active ? DomiNoxTheme.BgDeep : DomiNoxTheme.TextSecondary;
                }
            }
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
            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private enum CollectionTab { Progression, DomiNex, Objectives, Bosses, Patterns, Combos, Gems, System }
    }
}
