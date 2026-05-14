using System.Globalization;
using UnityEngine;

public class DrivingDebugHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarController carController;
    [SerializeField] private CarSensors carSensors;

    [Header("HUD")]
    [SerializeField] private bool showHud = true;
    [SerializeField] private Vector2 hudPosition = new Vector2(12f, 12f);
    [SerializeField] private Vector2 hudSize = new Vector2(420f, 170f);

    [Header("Debug Logging")]
    [SerializeField] private bool enablePeriodicDebugLog = true;
    [SerializeField] private float debugLogIntervalSeconds = 1f;

    private GUIStyle boxStyle;
    private GUIStyle textStyle;
    private float nextDebugLogTime;

    private void Awake()
    {
        boxStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.UpperLeft,
            padding = new RectOffset(10, 10, 10, 10),
            fontSize = 12
        };

        textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            richText = true
        };
    }

    private void Update()
    {
        if (!enablePeriodicDebugLog)
        {
            return;
        }

        if (Time.time < nextDebugLogTime)
        {
            return;
        }

        nextDebugLogTime = Time.time + Mathf.Max(0.1f, debugLogIntervalSeconds);
        Debug.Log($"[HUD] {BuildStateLine()} | sensors={BuildSensorLine()}");
    }

    private void OnGUI()
    {
        if (!showHud)
        {
            return;
        }

        Rect panelRect = new Rect(hudPosition.x, hudPosition.y, hudSize.x, hudSize.y);
        GUILayout.BeginArea(panelRect, boxStyle);

        GUILayout.Label("<b>Driving Debug HUD</b>", textStyle);
        GUILayout.Space(4f);

        GUILayout.Label(BuildStateLine(), textStyle);
        GUILayout.Space(2f);
        GUILayout.Label("Sensors (F, FL, L, FR, R): " + BuildSensorLine(), textStyle);

        GUILayout.EndArea();
    }

    private string BuildStateLine()
    {
        if (carController == null)
        {
            return "Controller missing";
        }

        return string.Format(
            CultureInfo.InvariantCulture,
            "Mode: <b>{0}</b> | Steer: {1:F3} | Throttle: {2:F3} | Speed: {3:F3}",
            carController.CurrentControlMode,
            carController.CurrentSteeringInput,
            carController.CurrentThrottleInput,
            carController.CurrentSpeed);
    }

    private string BuildSensorLine()
    {
        if (carSensors == null)
        {
            return "(missing)";
        }

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0:F3}, {1:F3}, {2:F3}, {3:F3}, {4:F3}",
            carSensors.front,
            carSensors.frontLeft,
            carSensors.left,
            carSensors.frontRight,
            carSensors.right);
    }
}
