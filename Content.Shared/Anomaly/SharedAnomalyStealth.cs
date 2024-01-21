using Content.Shared.Anomaly.Components;
using Content.Shared.Anomaly.Effects.Components;
using Robust.Shared.Map;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Shared.Flash;
using Robust.Shared.GameStates;
using Timer = Robust.Shared.Timing.Timer;
using Content.Shared.Eye.Blinding;
using Content.Shared.Anomaly.Effects;
// using Robust.Shared.GameObjects;
using Robust.Shared.GameObjects;
using System.Xml;
using Content.Shared.Doors.Components;
using System.Data;

namespace Content.Server.Anomaly.Effects;

/// <summary>
/// This handles <see cref="StealthAnomalySystem"/> and the events from <seealso cref="AnomalySystem"/>
/// </summary>
public sealed class StealthAnomalySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly OccluderSystem _occluderSystem = default!;
    [Dependency] private readonly SharedStealthSystem _stealthable = default!;
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly SharedFlashSystem _flashSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<StealthAnomalyComponent, AnomalyPulseEvent>(OnPulse);
        SubscribeLocalEvent<StealthAnomalyComponent, AnomalySupercriticalEvent>(OnSupercritical);
        // SubscribeLocalEvent<StealthAnomalyComponent, OnSpawnAn>(OnSupercritical);
        SubscribeLocalEvent<StealthAnomalyComponent, MapInitEvent>(OnSpawnAnomaly);
    }

    /// <summary>
    /// This handles <see cref="AnomalyStabilityChangedEvent"/> from <seealso cref="AnomalySystem"/>.
    /// Means it activates when anomaly created
    /// </summary>
    private void OnSpawnAnomaly(EntityUid uid, StealthAnomalyComponent component, MapInitEvent args)
    {
        // checking because it can be null in other prototipes for some reason
        if (!_entities.TryGetComponent<AnomalyComponent>(uid, out var anomComp))
            return;
        var xform = Transform(uid);
        StealthNearBy(uid, xform.Coordinates, component.maximumStealthingRadius * anomComp.Severity, component.maximumStealthingRadius / 2f);
    }

    /// <summary>
    /// This handles <see cref="AnomalyPulseEvent"/> from <seealso cref="AnomalySystem"/>.
    /// Using private method StealthNearBy()
    /// </summary>
    private void OnPulse(EntityUid uid, StealthAnomalyComponent component, ref AnomalyPulseEvent args)
    {
        var xform = Transform(uid);
        var stealthingRadius = component.maximumStealthingRadius * args.Stability;
        StealthNearBy(uid, xform.Coordinates, args.Severity, stealthingRadius);
    }
    /// <summary>
    /// This handles <see cref="AnomalyPulseEvent"/> from <seealso cref="AnomalySystem"/>.
    /// Using private method StealthNearBy()
    /// </summary>
    private void OnSupercritical(EntityUid uid, StealthAnomalyComponent component, ref AnomalySupercriticalEvent args)
    {
        var xform = Transform(uid);
        StealthNearBy(uid, xform.Coordinates, 1f, component.maximumStealthingRadius * 2f);
    }

    // Добавляем всем сущностям StealthComponent, StealthOnMoveComponent, если без компонента StealthComponent
    // Так же флешими их
    /// <summary>
    /// This handles <see cref="StealthAnomalySystem"/> from <seealso cref="AnomalySystem"/>.
    /// Tis method adds <see cref="StealthComponent"/> <see cref="StealthOnMoveComponent"/>
    /// </summary>
    private void StealthNearBy(EntityUid uid, EntityCoordinates coordinates, float severity, float range)
    {

        var targetEntities = new HashSet<EntityUid>();
        var stealthedEntities = new HashSet<EntityUid>();
        var stealthedEntitiesStructures = new HashSet<EntityUid>();
        // Находит все сущности, которые не хранятся или не одеты.
        // targetEntities = _entityLookup.GetEntitiesInRange(coordinates, range * (severity + 1f), LookupFlags.Uncontained);
        targetEntities = _entityLookup.GetEntitiesInRange(coordinates, 5f, LookupFlags.Uncontained);
        // Добавляем всем сущностям StealthComponent, StealthOnMoveComponent, если без компонента StealthComponent
        // Так же флешими их
        foreach (var targetEnt in targetEntities)
        {
            if (_entities.HasComponent<StealthComponent>(targetEnt))
                continue;
            // TODO make a new tryAddStealthComponent Method in StealthShared
            _entities.AddComponent<StealthComponent>(targetEnt);
            _entities.AddComponent<StealthOnMoveComponent>(targetEnt);
            stealthedEntities.Add(targetEnt);
            if (_entities.TryGetComponent<OccluderComponent>(targetEnt, out var occComp))
            {
                // SetOccluder(targetEnt, occComp);
                // Do walls transperent. Walls that are invisible are transperent by default
                _occluderSystem.SetEnabled(targetEnt, false, occComp);

            }
            // if(_entities.HasComponent<OccluderComponent>(targetEnt))
            // {
            //     stealthedEntities.Add(targetEnt, _entities.GetComponent<OccluderComponent>(targetEnt));
            // }
            // _stealthable.SetEnabled(targetEnt, true, null);
            // если стена видима, то нужно ей изменить параметры
            // if (_entities.HasComponent<Structable>)
                // targetEntitiesStructures.Add(targetEnt);
                // функцияПоменятьРендерНаПрозрачный!

            // зафлешить всех
            // TODO сделать ЗАВИСИМОСТЬ ОТ ПРОЦЕНТА МАССЫ АНОМЫ!
            // if (_entities.HasComponent<FlashableComponent>(targetEnt))
            //     _flashSystem.Flash(targetEnt, uid, uid, 2000f * (1f + severity) * (1f + severity), 0.8f * severity, false, _entities.GetComponent<FlashableComponent>(targetEnt), false);
        }

        // убираем со всех компонентов стелс со стелса и структур
        // TODO: время невидимости от критической массы
        Timer.Spawn(TimeSpan.FromSeconds(20 * (severity + 2) * (severity + 2)), () => OnTimeOutDeleteComponent(uid, stealthedEntities, stealthedEntitiesStructures), default);

        return;
    }

    // private async void SetOccluder(EntityUid uid, OccluderComponent occComp)
    // {
    //     _occluderSystem.SetEnabled(uid, false, occComp);
    // }

    private void OnTimeOutDeleteComponent(EntityUid uid, HashSet<EntityUid> entities, HashSet<EntityUid> entitiesStructures)
    {

        foreach (var targetEnt in entities)
        {
            if (TryComp<StealthComponent>(targetEnt, out var stealthComp))
            {
                if (!TryComp<StealthOnMoveComponent>(targetEnt, out var stealthOnMoveComp))
                    return;
                else
                {
                    _stealthable.ModifyStealthOnMove(targetEnt, 0.25f, 0.1f, stealthComp, stealthOnMoveComp);
                    // GetVisibility, не ебу почему не так для остального делает событие, которое обновляет StealthOnMove иначе ничего не будет потому что потому сука.
                    // _stealthable.GetVisibility(targetEnt, stealthComp);
                    if (_entities.TryGetComponent<OccluderComponent>(targetEnt, out var occComp))
                        if (_entities.TryGetComponent<DoorComponent>(targetEnt, out var doorComp))
                            if (doorComp.State == DoorState.Open)
                                _occluderSystem.SetEnabled(targetEnt, false);
                            else
                            {
                                _occluderSystem.SetEnabled(targetEnt, true);
                            }
                    // идут для ивента RequestChangeStealthEvent
                    OnTimeOutDeleteStealthComponent(targetEnt);

                }

                // var c = _entities.GetComponent<StealthComponent>(targetEnt);
                // c.HadOutline = true;
                // c.LastVisibility = c.LastVisibility - 0.3f;
            }

        }
        // TODO сделать ивент, который бы менял энтити прозрачность.
        // RaiseNetworkEvent(new RequestChangeStealthEvent(GetNetEntity(uid), netEntities));
    }

    private void OnTimeOutDeleteStealthComponent(EntityUid targetEnt)
    {
        if (_entities.HasComponent<StealthComponent>(targetEnt) && !_entities.HasComponent<StealthAnomalyComponent>(targetEnt))
        {
            _entities.RemoveComponent<StealthOnMoveComponent>(targetEnt);

            _entities.RemoveComponent<StealthComponent>(targetEnt);
        }
    }
}
