using System.Globalization;
using TMPro;
using UnityEngine;

public class DrivingDebugHUD : MonoBehaviour
{
    [Header("Data References")]
    [SerializeField] private CarController carController;
    [SerializeField] private CarSensors carSensors;

    [Header("UI Text References")]
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private TMP_Text steerText;
    [SerializeField] private TMP_Text throttleText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text sensorsText;

    [Header("Options")]
    [SerializeField] private bool hidePanelWhenMissingReferences = true;
    [SerializeField] private GameObject panelRoot;

    [Header("Optional Debug Logs")]
    [SerializeField] private bool enablePeriodicDebugLog;
    [SerializeField] private float debugLogIntervalSeconds = 1f;

    private float nextDebugLogTime;

    private void Update()
    {
        bool hasReferences = carController != null && carSensors != null;

        if (panelRoot != null && hidePanelWhenMissingReferences)
        {
            panelRoot.SetActive(hasReferences);
        }

        if (!hasReferences)
        {
            return;
        }

        UpdateUiTexts();
        MaybeWritePeriodicLog();
    }

    private void UpdateUiTexts()
    {
        if (modeText != null)
        {
            modeText.text = "Mode: " + carController.CurrentControlMode;
        }

        if (steerText != null)
        {
            steerText.text = "Steer: " + carController.CurrentSteeringInput.ToString("F3", CultureInfo.InvariantCulture);
        }

        if (throttleText != null)
        {
            throttleText.text = "Throttle: " + carController.CurrentThrottleInput.ToString("F3", CultureInfo.InvariantCulture);
        }

        if (speedText != null)
        {
            speedText.text = "Speed: " + carController.CurrentSpeed.ToString("F3", CultureInfo.InvariantCulture);
        }

        if (sensorsText != null)
        {
            sensorsText.text = string.Format(
                CultureInfo.InvariantCulture,
                "Sensors F/FL/L/FR/R: {0:F3}, {1:F3}, {2:F3}, {3:F3}, {4:F3}",
                carSensors.front,
                carSensors.frontLeft,
                carSensors.left,
                carSensors.frontRight,
                carSensors.right);
        }
    }

    private void MaybeWritePeriodicLog()
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

        Debug.Log(string.Format(
            CultureInfo.InvariantCulture,
            "[HUD] mode={0}, steer={1:F3}, throttle={2:F3}, speed={3:F3}, sensors={4:F3}|{5:F3}|{6:F3}|{7:F3}|{8:F3}",
            carController.CurrentControlMode,
            carController.CurrentSteeringInput,
            carController.CurrentThrottleInput,
            carController.CurrentSpeed,
            carSensors.front,
            carSensors.frontLeft,
            carSensors.left,
            carSensors.frontRight,
            carSensors.right));
    }
}