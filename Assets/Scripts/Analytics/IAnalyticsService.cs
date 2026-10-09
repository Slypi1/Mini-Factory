using System.Collections.Generic;

public interface IAnalyticsService
{
    void TrackEvent(
        string eventName,
        Dictionary<string, object> parameters = null);
}
