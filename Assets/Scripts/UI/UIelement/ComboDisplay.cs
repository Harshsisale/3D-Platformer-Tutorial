using UnityEngine;
using UnityEngine.UI;

public class ComboDisplay : MonoBehaviour
{
    private GameManager gameManager;
    private Health playerHealth;
    private CanvasGroup canvasGroup;
    private Text multiplierText;
    private Text chainText;
    private Text controlsText;
    private RectTransform timerFill;
    private Image timerImage;

    public static void Create(GameManager manager)
    {
        UIManager ui = UIManager.instance;
        if (manager.player == null || ui == null || ui.pages == null ||
            ui.defaultPage < 0 || ui.defaultPage >= ui.pages.Count || ui.pages[ui.defaultPage] == null)
        {
            return;
        }

        Transform page = ui.pages[ui.defaultPage].transform;
        if (page.GetComponentInChildren<ComboDisplay>(true) != null)
        {
            return;
        }

        Font font = null;
        ScoreDisplay scoreDisplay = page.GetComponentInChildren<ScoreDisplay>(true);
        if (scoreDisplay != null && scoreDisplay.displayText != null)
        {
            font = scoreDisplay.displayText.font;
        }
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        GameObject panel = new GameObject("Pickup Combo", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        panel.transform.SetParent(page, false);
        panel.layer = page.gameObject.layer;

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -64f);
        rect.sizeDelta = new Vector2(240f, 68f);

        Image background = panel.GetComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.78f);
        background.raycastTarget = false;

        ComboDisplay display = panel.AddComponent<ComboDisplay>();
        display.gameManager = manager;
        display.playerHealth = manager.player.GetComponent<Health>();
        display.canvasGroup = panel.GetComponent<CanvasGroup>();
        display.canvasGroup.interactable = false;
        display.canvasGroup.blocksRaycasts = false;
        display.multiplierText = CreateText(panel.transform, "Multiplier", font, 14, new Vector2(0f, -17f));
        display.chainText = CreateText(panel.transform, "Chain Timer", font, 10, new Vector2(0f, -39f));

        RectTransform track = CreateBar(panel.transform, "Timer Track", new Color(0.15f, 0.2f, 0.24f, 0.2f));
        track.anchorMin = track.anchorMax = new Vector2(0.5f, 0f);
        track.anchoredPosition = new Vector2(0f, 10f);
        track.sizeDelta = new Vector2(212f, 5f);

        display.timerFill = CreateBar(track, "Time Remaining", new Color(0.08f, 0.6f, 0.55f, 1f));
        display.timerFill.anchorMin = Vector2.zero;
        display.timerFill.anchorMax = Vector2.one;
        display.timerFill.offsetMin = display.timerFill.offsetMax = Vector2.zero;
        display.timerImage = display.timerFill.GetComponent<Image>();

        display.controlsText = CreateText(page, "Movement Controls", font, 10, new Vector2(0f, 20f));
        display.controlsText.text = "SHIFT: Sprint   E: Dash   SPACE: Jump";
        display.controlsText.rectTransform.anchorMin = display.controlsText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        display.controlsText.rectTransform.sizeDelta = new Vector2(560f, 24f);
        display.controlsText.color = Color.white;
        Shadow shadow = display.controlsText.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(1f, -1f);
        display.Refresh();
    }

    private static Text CreateText(Transform parent, string name, Font font, int size, Vector2 position)
    {
        GameObject label = new GameObject(name, typeof(RectTransform), typeof(Text));
        label.transform.SetParent(parent, false);
        label.layer = parent.gameObject.layer;
        Text text = label.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.color = new Color(0.16f, 0.2f, 0.24f, 1f);
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        text.supportRichText = false;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(224f, 24f);
        return text;
    }

    private static RectTransform CreateBar(Transform parent, string name, Color color)
    {
        GameObject bar = new GameObject(name, typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(parent, false);
        bar.layer = parent.gameObject.layer;
        Image image = bar.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return bar.GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (gameManager == null)
        {
            return;
        }

        bool visible = !gameManager.gameIsOver && gameManager.player != null &&
            gameManager.player.activeInHierarchy && (playerHealth == null || playerHealth.currentHealth > 0);
        canvasGroup.alpha = visible ? 1f : 0f;
        controlsText.enabled = visible;
        if (!visible)
        {
            return;
        }

        PickupCombo combo = gameManager.Combo;
        multiplierText.text = "PICKUP COMBO  x" + combo.Multiplier;
        chainText.text = combo.ChainCount > 0
            ? combo.ChainCount + " collected  |  " + combo.RemainingTime.ToString("0.0") + "s left"
            : "Chain pickups within " + combo.Window.ToString("0.#") + "s";
        timerFill.anchorMax = new Vector2(Mathf.Clamp01(combo.RemainingTime / combo.Window), 1f);
        timerImage.color = combo.RemainingTime < 1f
            ? new Color(0.85f, 0.28f, 0.22f, 1f)
            : combo.Multiplier == combo.MaximumMultiplier
                ? new Color(0.9f, 0.58f, 0.08f, 1f)
                : new Color(0.08f, 0.6f, 0.55f, 1f);
    }
}
