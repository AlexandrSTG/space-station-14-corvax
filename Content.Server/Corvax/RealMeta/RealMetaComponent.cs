namespace Content.Server.Corvax.RealMeta.RealMeta;

/// <summary>
/// Set Real meta for antags
/// </summary>
[RegisterComponent]
[Access(typeof(RealMetaSystem))]
public sealed partial class RealMetaComponent : Component
{
    /// <summary>
    /// Name to be shown when examined.
    /// </summary>
    [DataField("name", required: true), ViewVariables(VVAccess.ReadWrite)]
    public string Name = string.Empty;

    /// <summary>
    /// Desc to be shown when examined.
    /// </summary>
    [DataField("desc", required: true), ViewVariables(VVAccess.ReadWrite)]
    public string Desc = string.Empty;
}
