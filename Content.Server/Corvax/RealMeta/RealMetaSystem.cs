using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Roles;

namespace Content.Server.Corvax.RealMeta.RealMeta;

public sealed class RealMetaSystem : EntitySystem
{
    [Dependency] private readonly SharedRoleSystem _sharedRole = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RealMetaComponent, ExaminedEvent>(OnExamined);
    }
    private void OnExamined(EntityUid uid, RealMetaComponent component, ExaminedEvent args)
    {
        if (HasComp<GhostComponent>(args.Examiner) || IsTraitor(args.Examiner))
        {
            args.PushMarkup(Loc.GetString("real-meta-name") + Loc.GetString(component.Name));
            args.PushMarkup(Loc.GetString("real-meta-desc") + Loc.GetString(component.Desc));
        }
    }

    private bool IsTraitor(EntityUid user)
    {
        return _mindSystem.TryGetMind(user, out var mindId, out _) && _sharedRole.MindIsAntagonist(mindId);
    }
}
