using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour {
    public static DebugPanel instance;

    private Text text;
    private string textPerFrame = "[DebugPanel Log]";

    private bool isPanelOn = true;

    private void Awake() {
        if (instance == null || instance != this) instance = this;

        text = GetComponent<Text>();
    }

    public static void Log(string info) {
        if (instance)
            instance.textPerFrame = instance.textPerFrame + "\n" + info;
    }

    private void Update() {
        instance.text.text = textPerFrame;
        textPerFrame = "[DebugPanel Log]";
        if (Input.GetKeyDown(KeyCode.Tab)) {
            isPanelOn = !isPanelOn;
            text.enabled = isPanelOn;
        }
    }
}