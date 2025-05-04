using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a test scene for verifying input bindings
/// </summary>
public class InputTestScene : MonoBehaviour
{
    [SerializeField] private bool createAtRuntime = true;

    void Start()
    {
        if (createAtRuntime)
        {
            SetupTestScene();
        }
    }

    public void SetupTestScene()
    {
        // Create Canvas for UI
        GameObject canvasObj = new GameObject("Input Test Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Add CanvasScaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Add GraphicRaycaster
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create parent for button indicators
        GameObject indicatorsParent = new GameObject("Button Indicators");
        indicatorsParent.transform.SetParent(canvasObj.transform, false);

        RectTransform parentRect = indicatorsParent.AddComponent<RectTransform>();
        parentRect.anchorMin = new Vector2(0, 0);
        parentRect.anchorMax = new Vector2(1, 1);
        parentRect.offsetMin = new Vector2(50, 50);
        parentRect.offsetMax = new Vector2(-50, -50);

        // Add VerticalLayoutGroup
        VerticalLayoutGroup layout = indicatorsParent.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        // Create Content Size Fitter
        ContentSizeFitter fitter = indicatorsParent.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Add Scroll Rect
        GameObject scrollObj = new GameObject("Scroll View");
        scrollObj.transform.SetParent(canvasObj.transform, false);

        RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.content = parentRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 10;
        scroll.viewport = scrollRect;

        // Create Title Text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(canvasObj.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(0, 80);
        titleRect.anchoredPosition = new Vector2(0, -40);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "INPUT BINDING TEST";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 30;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;

        // Create button indicator prefab
        GameObject indicatorPrefab = CreateButtonIndicatorPrefab();

        // Add the InputTester component
        GameObject testerObj = new GameObject("Input Tester");
        InputTester tester = testerObj.AddComponent<InputTester>();

        // Try to find and assign the input action asset
        tester.GetType().GetField("buttonIndicatorsParent",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(tester, parentRect);

        tester.GetType().GetField("buttonIndicatorPrefab",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(tester, indicatorPrefab);

        // Add the InputBindingFixer component
        GameObject fixerObj = new GameObject("Input Binding Fixer");
        fixerObj.AddComponent<InputBindingFixer>();

        // Add instructions
        GameObject instructionsObj = new GameObject("Instructions");
        instructionsObj.transform.SetParent(canvasObj.transform, false);

        RectTransform instructionsRect = instructionsObj.AddComponent<RectTransform>();
        instructionsRect.anchorMin = new Vector2(0, 0);
        instructionsRect.anchorMax = new Vector2(1, 0);
        instructionsRect.sizeDelta = new Vector2(0, 80);
        instructionsRect.anchoredPosition = new Vector2(0, 40);

        Text instructionsText = instructionsObj.AddComponent<Text>();
        instructionsText.text = "Press any controller button to test. Green = Pressed, Red = Released";
        instructionsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        instructionsText.fontSize = 20;
        instructionsText.alignment = TextAnchor.MiddleCenter;
        instructionsText.color = Color.white;
    }

    private GameObject CreateButtonIndicatorPrefab()
    {
        GameObject prefab = new GameObject("ButtonIndicator");

        // Create layout
        RectTransform rectTransform = prefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0, 50);

        // Background image
        Image bgImage = prefab.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Create label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(prefab.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.7f, 1);
        labelRect.offsetMin = new Vector2(10, 0);
        labelRect.offsetMax = new Vector2(-10, 0);

        Text labelText = labelObj.AddComponent<Text>();
        labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelText.fontSize = 16;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = Color.white;

        // Create indicator
        GameObject indicatorObj = new GameObject("Indicator");
        indicatorObj.transform.SetParent(prefab.transform, false);

        RectTransform indicatorRect = indicatorObj.AddComponent<RectTransform>();
        indicatorRect.anchorMin = new Vector2(0.7f, 0.2f);
        indicatorRect.anchorMax = new Vector2(0.95f, 0.8f);
        indicatorRect.offsetMin = Vector2.zero;
        indicatorRect.offsetMax = Vector2.zero;

        Image indicatorImage = indicatorObj.AddComponent<Image>();
        indicatorImage.color = Color.red;

        return prefab;
    }
}