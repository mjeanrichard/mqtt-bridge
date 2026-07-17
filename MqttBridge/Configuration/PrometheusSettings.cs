namespace MqttBridge.Configuration;

public class PrometheusSettings
{
    public const string Name = "Prometheus";

    /// <summary>
    /// Base URL for writing metrics (vminsert in a VictoriaMetrics cluster). Should end with a trailing slash and
    /// include any cluster prefix, e.g. <c>http://vminsert:8480/insert/0/prometheus/</c>.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for deleting series (vmselect in a VictoriaMetrics cluster). Should end with a trailing slash and
    /// include any cluster prefix, e.g. <c>http://vmselect:8481/delete/0/prometheus/</c>. When empty, <see cref="Url"/> is used.
    /// </summary>
    public string DeleteUrl { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}