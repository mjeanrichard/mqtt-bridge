using System.Text;

namespace MqttBridge.Models;

public class Metric
{
    public Metric(string name)
    {
        Name = name;
        Tags = new Dictionary<string, string>();
    }

    public string Name { get; set; }

    public double Value { get; set; }

    public long Timestamp { get; set; }

    public IDictionary<string, string> Tags { get; set; }

    public Metric SetTag(string tag, string value)
    {
        Tags[tag] = value;
        return this;
    }

    public string ToPrometheus()
    {
        StringBuilder builder = new();
        builder.Append(Name);

        if (Tags.Any())
        {
            builder.Append('{');
            builder.Append(string.Join(',', Tags.Select(t => $@"{t.Key}=""{t.Value}""")));
            builder.Append('}');
        }

        builder.Append(" ");
        builder.Append(Value);
        builder.Append(" ");
        builder.Append(Timestamp);

        return builder.ToString();
    }
}