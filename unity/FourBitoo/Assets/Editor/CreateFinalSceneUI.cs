using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class CreateFinalSceneUI : EditorWindow
{
    [MenuItem("Tools/Crear UI Final Scene")]
    public static void CreateUI()
    {
        // Asegurarse de que tenemos un canvas
        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        // Crear un panel para organizar los elementos
        GameObject panelObject = new GameObject("ResultsPanel");
        panelObject.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // Configurar el panel
        panelRect.anchorMin = new Vector2(0.2f, 0.2f);
        panelRect.anchorMax = new Vector2(0.8f, 0.8f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Crear título
        GameObject titleObject = CreateTextObject("Título", "RESULTADOS DEL PARTIDO", 36, TextAlignmentOptions.Center);
        titleObject.transform.SetParent(panelRect, false);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.8f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = new Vector2(20, 0);
        titleRect.offsetMax = new Vector2(-20, -20);

        // Crear Team1ScoreText
        GameObject team1TextObject = CreateTextObject("Team1ScoreText", "0", 30, TextAlignmentOptions.Center);
        team1TextObject.transform.SetParent(panelRect, false);
        RectTransform team1Rect = team1TextObject.GetComponent<RectTransform>();
        team1Rect.anchorMin = new Vector2(0, 0.5f);
        team1Rect.anchorMax = new Vector2(0.5f, 0.7f);
        team1Rect.offsetMin = new Vector2(50, 0);
        team1Rect.offsetMax = new Vector2(-50, 0);

        // Crear etiqueta para Team1
        GameObject team1LabelObject = CreateTextObject("Team1Label", "EQUIPO 1", 24, TextAlignmentOptions.Center);
        team1LabelObject.transform.SetParent(panelRect, false);
        RectTransform team1LabelRect = team1LabelObject.GetComponent<RectTransform>();
        team1LabelRect.anchorMin = new Vector2(0, 0.7f);
        team1LabelRect.anchorMax = new Vector2(0.5f, 0.8f);
        team1LabelRect.offsetMin = new Vector2(50, 0);
        team1LabelRect.offsetMax = new Vector2(-50, 0);

        // Crear Team2ScoreText
        GameObject team2TextObject = CreateTextObject("Team2ScoreText", "0", 30, TextAlignmentOptions.Center);
        team2TextObject.transform.SetParent(panelRect, false);
        RectTransform team2Rect = team2TextObject.GetComponent<RectTransform>();
        team2Rect.anchorMin = new Vector2(0.5f, 0.5f);
        team2Rect.anchorMax = new Vector2(1, 0.7f);
        team2Rect.offsetMin = new Vector2(50, 0);
        team2Rect.offsetMax = new Vector2(-50, 0);

        // Crear etiqueta para Team2
        GameObject team2LabelObject = CreateTextObject("Team2Label", "EQUIPO 2", 24, TextAlignmentOptions.Center);
        team2LabelObject.transform.SetParent(panelRect, false);
        RectTransform team2LabelRect = team2LabelObject.GetComponent<RectTransform>();
        team2LabelRect.anchorMin = new Vector2(0.5f, 0.7f);
        team2LabelRect.anchorMax = new Vector2(1, 0.8f);
        team2LabelRect.offsetMin = new Vector2(50, 0);
        team2LabelRect.offsetMax = new Vector2(-50, 0);

        // Crear WinnerText
        GameObject winnerTextObject = CreateTextObject("WinnerText", "¡El Equipo X ha ganado!", 32, TextAlignmentOptions.Center);
        winnerTextObject.transform.SetParent(panelRect, false);
        RectTransform winnerRect = winnerTextObject.GetComponent<RectTransform>();
        winnerRect.anchorMin = new Vector2(0, 0.2f);
        winnerRect.anchorMax = new Vector2(1, 0.4f);
        winnerRect.offsetMin = new Vector2(20, 0);
        winnerRect.offsetMax = new Vector2(-20, 0);

        // Crear objeto ResultsDisplay y añadir componente
        GameObject resultsObject = new GameObject("ResultsDisplay");
        FinalSceneResultsDisplay resultsDisplay = resultsObject.AddComponent<FinalSceneResultsDisplay>();
        
        // Asignar las referencias usando SerializedObject para acceder a las propiedades serializadas
        SerializedObject serializedDisplay = new SerializedObject(resultsDisplay);
        SerializedProperty team1ScoreTextProp = serializedDisplay.FindProperty("team1ScoreText");
        SerializedProperty team2ScoreTextProp = serializedDisplay.FindProperty("team2ScoreText");
        SerializedProperty winnerTextProp = serializedDisplay.FindProperty("winnerText");
        
        team1ScoreTextProp.objectReferenceValue = team1TextObject.GetComponent<TextMeshProUGUI>();
        team2ScoreTextProp.objectReferenceValue = team2TextObject.GetComponent<TextMeshProUGUI>();
        winnerTextProp.objectReferenceValue = winnerTextObject.GetComponent<TextMeshProUGUI>();
        
        serializedDisplay.ApplyModifiedProperties();

        // Notificar al usuario
        Debug.Log("✅ UI para la escena final creada con éxito.");
        EditorUtility.DisplayDialog("Éxito", "La UI para mostrar resultados ha sido creada correctamente.", "OK");
    }

    private static GameObject CreateTextObject(string name, string defaultText, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name);
        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = defaultText;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        return textObject;
    }
}
