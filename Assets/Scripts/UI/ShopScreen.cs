using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The shop, where stars collected across runs buy permanent upgrades.
///
/// It builds itself: an open button on the menu, a panel over the top of it,
/// and one row per upgrade. Building in code rather than in the scene keeps
/// the whole layout in one readable place, and means the menu scene does not
/// have to carry a screen it only shows sometimes.
/// </summary>
public class ShopScreen : MonoBehaviour
{
    private const float CanvasWidth = 1080f;
    private const float CanvasHeight = 1920f;

    private static readonly Color Ink = new Color(0.96f, 0.96f, 1f);
    private static readonly Color Dim = new Color(0.72f, 0.72f, 0.82f);
    private static readonly Color Panel = new Color(0.07f, 0.06f, 0.14f, 1f);
    private static readonly Color RowBack = new Color(1f, 1f, 1f, 0.06f);
    private static readonly Color Buyable = new Color(0.32f, 0.72f, 0.42f);
    private static readonly Color TooDear = new Color(0.3f, 0.3f, 0.38f);
    private static readonly Color Owned = new Color(0.22f, 0.34f, 0.6f);

    private GameObject panel;
    private TextMeshProUGUI balanceLabel;
    private readonly List<RowWidgets> rows = new List<RowWidgets>();

    private struct RowWidgets
    {
        public string Id;
        public TextMeshProUGUI Name;
        public TextMeshProUGUI Level;
        public Button Buy;
        public TextMeshProUGUI BuyLabel;
        public Image BuyBack;
    }

    private void Start()
    {
        Canvas canvas = BuildCanvas("ShopCanvas", 20);
        BuildOpenButton(canvas.transform);
        BuildPanel(canvas.transform);
        panel.SetActive(false);
    }

    private Canvas BuildCanvas(string name, int order)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(transform, false);

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = order;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(CanvasWidth, CanvasHeight);
        scaler.matchWidthOrHeight = 0f;

        root.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    /// <summary>The button on the menu that opens the panel.</summary>
    private void BuildOpenButton(Transform parent)
    {
        // Below the Settings button, which sits at about 30 percent height.
        Image back = MakeImage(parent, "ShopButton", new Vector2(0.5f, 0.21875f),
                               new Vector2(600f, 100f), new Color(0.2f, 0.16f, 0.34f, 0.95f));
        Button button = back.gameObject.AddComponent<Button>();
        button.targetGraphic = back;
        button.onClick.AddListener(Open);

        TextMeshProUGUI label = MakeText(back.transform, "Label", new Vector2(0.5f, 0.5f),
                                         new Vector2(560f, 90f), 60f, Ink);
        label.text = "Shop";
        label.rectTransform.anchoredPosition = Vector2.zero;
    }

    private void BuildPanel(Transform parent)
    {
        Image back = MakeImage(parent, "ShopPanel", new Vector2(0.5f, 0.5f),
                               new Vector2(CanvasWidth, CanvasHeight), Panel);
        panel = back.gameObject;

        // Stretched to the full canvas rather than sized to the reference
        // resolution, so it covers the whole screen whatever its shape. It was
        // letting the menu show through underneath.
        RectTransform panelRect = back.rectTransform;
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI title = MakeText(panel.transform, "Title", new Vector2(0.5f, 0.88f),
                                          new Vector2(900f, 130f), 96f, Ink);
        title.text = "Shop";

        balanceLabel = MakeText(panel.transform, "Balance", new Vector2(0.5f, 0.815f),
                                 new Vector2(900f, 80f), 52f, Dim);

        // One row per upgrade, stacked downward from just under the balance.
        float top = 0.715f;
        float rowStep = 0.105f;

        for (int i = 0; i < PlayerUpgrades.Catalogue.Length; i++)
        {
            rows.Add(BuildRow(PlayerUpgrades.Catalogue[i], top - rowStep * i));
        }

        Image closeBack = MakeImage(panel.transform, "Close", new Vector2(0.5f, 0.13f),
                                     new Vector2(560f, 110f), new Color(0.24f, 0.2f, 0.4f, 1f));
        Button close = closeBack.gameObject.AddComponent<Button>();
        close.targetGraphic = closeBack;
        close.onClick.AddListener(Close);

        TextMeshProUGUI closeLabel = MakeText(closeBack.transform, "Label", new Vector2(0.5f, 0.5f),
                                               new Vector2(520f, 100f), 60f, Ink);
        closeLabel.text = "Back";
        closeLabel.rectTransform.anchoredPosition = Vector2.zero;
    }

    private RowWidgets BuildRow(PlayerUpgrades.Upgrade upgrade, float height)
    {
        Image back = MakeImage(panel.transform, "Row_" + upgrade.Id, new Vector2(0.5f, height),
                               new Vector2(940f, 150f), RowBack);

        TextMeshProUGUI name = MakeText(back.transform, "Name", new Vector2(0f, 0.68f),
                                         new Vector2(600f, 60f), 48f, Ink);
        name.alignment = TextAlignmentOptions.Left;
        name.rectTransform.anchoredPosition = new Vector2(330f, 0f);
        name.text = upgrade.Name;

        TextMeshProUGUI effect = MakeText(back.transform, "Effect", new Vector2(0f, 0.3f),
                                           new Vector2(600f, 50f), 38f, Dim);
        effect.alignment = TextAlignmentOptions.Left;
        effect.rectTransform.anchoredPosition = new Vector2(330f, 0f);
        effect.text = upgrade.Effect;

        TextMeshProUGUI level = MakeText(back.transform, "Level", new Vector2(1f, 0.68f),
                                          new Vector2(240f, 60f), 38f, Dim);
        level.alignment = TextAlignmentOptions.Right;
        level.rectTransform.anchoredPosition = new Vector2(-140f, 0f);

        Image buyBack = MakeImage(back.transform, "Buy", new Vector2(1f, 0.32f),
                                   new Vector2(240f, 76f), Buyable);
        buyBack.rectTransform.anchoredPosition = new Vector2(-140f, 0f);

        Button buy = buyBack.gameObject.AddComponent<Button>();
        buy.targetGraphic = buyBack;

        string id = upgrade.Id;
        buy.onClick.AddListener(() => Purchase(id));

        TextMeshProUGUI buyLabel = MakeText(buyBack.transform, "Label", new Vector2(0.5f, 0.5f),
                                             new Vector2(230f, 70f), 40f, Ink);
        buyLabel.rectTransform.anchoredPosition = Vector2.zero;

        return new RowWidgets
        {
            Id = id,
            Name = name,
            Level = level,
            Buy = buy,
            BuyLabel = buyLabel,
            BuyBack = buyBack,
        };
    }

    private void Open()
    {
        Refresh();
        panel.SetActive(true);
    }

    private void Close()
    {
        panel.SetActive(false);

        // The menu's own progress line shows the balance, so bring it up to date.
        MenuProgress progress = FindAnyObjectByType<MenuProgress>();
        if (progress != null)
        {
            progress.Refresh();
        }
    }

    private void Purchase(string id)
    {
        if (PlayerUpgrades.TryBuy(id))
        {
            Refresh();
        }
    }

    /// <summary>Redraws every row against the current balance.</summary>
    private void Refresh()
    {
        int balance = PlayerUpgrades.AvailableStars;

        if (balanceLabel != null)
        {
            balanceLabel.text = balance + " stars to spend";
        }

        for (int i = 0; i < rows.Count; i++)
        {
            RowWidgets row = rows[i];

            PlayerUpgrades.Upgrade upgrade;
            if (!PlayerUpgrades.TryGet(row.Id, out upgrade))
            {
                continue;
            }

            int owned = PlayerUpgrades.LevelOf(row.Id);
            row.Level.text = "Level " + owned + " / " + upgrade.MaxLevel;

            if (owned >= upgrade.MaxLevel)
            {
                row.BuyLabel.text = "Maxed";
                row.BuyBack.color = Owned;
                row.Buy.interactable = false;
                continue;
            }

            int cost = upgrade.CostAt(owned);
            row.BuyLabel.text = cost + " *";

            bool affordable = balance >= cost;
            row.BuyBack.color = affordable ? Buyable : TooDear;
            row.Buy.interactable = affordable;
        }
    }

    // Small builders, so the layout above reads as a layout.

    private static Image MakeImage(Transform parent, string name, Vector2 anchor,
                                   Vector2 size, Color colour)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        Image image = go.AddComponent<Image>();
        image.color = colour;

        RectTransform rect = image.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        return image;
    }

    private static TextMeshProUGUI MakeText(Transform parent, string name, Vector2 anchor,
                                            Vector2 size, float fontSize, Color colour)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = colour;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.raycastTarget = false;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        return text;
    }
}
