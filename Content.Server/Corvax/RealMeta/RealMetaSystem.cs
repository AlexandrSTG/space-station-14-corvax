using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Hands;

namespace Content.Server.Corvax.RealMeta.RealMeta;

/*public sealed class RealMetaSystem : EntitySystem
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
*/
public sealed class RealMetaSystem : EntitySystem
{
    [Dependency] private readonly SharedRoleSystem _sharedRole = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RealMetaComponent, ComponentStartup>(StartupComponent);
        SubscribeLocalEvent<RealMetaComponent, ComponentShutdown>(ShutdownComponent);
        SubscribeLocalEvent<RealMetaComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<RealMetaComponent, EquippedHandEvent>(OnPickup);

    }

    private void StartupComponent(EntityUid uid, RealMetaComponent component, ComponentStartup args)

    => SetFakeMeta(uid);
    private void ShutdownComponent(EntityUid uid, RealMetaComponent component, ComponentShutdown args)

    => SetTrueMeta(uid, component);
    private void OnExamined(EntityUid uid, RealMetaComponent component, ExaminedEvent args)

    => SetMeta(uid, component, args.Examiner);
    private void OnPickup(EntityUid uid, RealMetaComponent component, EquippedHandEvent args)

    => SetMeta(uid, component, args.User);

    private void SetMeta(EntityUid uid, RealMetaComponent component, EntityUid user)
    {
        if (HasComp<GhostComponent>(user) || IsTraitor(user))
        {
           SetFakeMeta(uid);
        }
        else
        {
            SetTrueMeta(uid, component);
        }
    }

    private void SetTrueMeta(EntityUid uid,  RealMetaComponent component)
    {
         if (TryComp<MetaDataComponent>(uid, out var meta))
        {
            var proto = meta.EntityPrototype;
        if (proto == null)
            return;
        _metaSystem.SetEntityName(uid, Loc.GetString(component.Name) + '\n' + "[color=#616F71]" + proto.Name);
        _metaSystem.SetEntityDescription(uid, Loc.GetString(component.Desc) +'\n' + "[color=#616F71]" + proto.Description);
        }
    }

    private void SetFakeMeta(EntityUid uid)
    {
        if (TryComp<MetaDataComponent>(uid, out var meta))
        {
            var proto = meta.EntityPrototype;
            if (proto == null)
                 return;

            _metaSystem.SetEntityName(uid, proto.Name);
            _metaSystem.SetEntityDescription(uid, proto.Description);
        }
    }

    private bool IsTraitor(EntityUid user)
    {
        return _mindSystem.TryGetMind(user, out var mindId, out _) && _sharedRole.MindIsAntagonist(mindId);
    }
}
