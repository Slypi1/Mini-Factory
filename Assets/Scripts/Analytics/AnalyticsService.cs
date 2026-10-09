using UnityEngine;
using System.Collections.Generic;

public class AnalyticsService : IAnalyticsService
{
    public void TrackEvent(
        string eventName,
        Dictionary<string, object> parameters = null)
    {
        string details = "";

        if (parameters != null)
        {
            foreach (var parameter in parameters)
            {
                details += $" {parameter.Key}={parameter.Value}";
            }
        }

        Debug.Log($"[Analytics] {eventName}{details}");
    }
}
