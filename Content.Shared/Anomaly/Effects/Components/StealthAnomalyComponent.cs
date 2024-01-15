using Content.Shared.Stealth.Components;

namespace Content.Shared.Anomaly.Effects.Components;

[RegisterComponent]
public sealed partial class StealthAnomalyComponent : Component
{
    /// <summary>
    /// The maximum distance from which you can be ignited by the anomaly.
    /// </summary>
    [DataField("maximumStealthingRadius")]
    public float maximumStealthingRadius= 5f;
}
