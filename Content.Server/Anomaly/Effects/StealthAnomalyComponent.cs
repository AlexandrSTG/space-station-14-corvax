using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Anomaly.Components;
using Content.Shared.Anomaly.Effects.Components;
using Robust.Shared.Map;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Server.Stealth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Robust.Shared.Collections;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Utility;
using Robust.Shared.GameObjects;
using Content.Shared.Damage;
using Content.Shared.Item;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Content.Shared.Eye.Blinding.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Flash;
using Content.Shared.Flash;
using Robust.Shared.GameStates;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.Anomaly.Effects;

/// <summary>
/// This handles <see cref="StealthAnomalySystem"/> and the events from <seealso cref="AnomalySystem"/>
/// </summary>
public sealed class StealthAnomalySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    // [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly SharedStealthSystem _stealthable = default!;
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly FlashSystem _flashSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<StealthAnomalyComponent, AnomalyPulseEvent>(OnPulse);
        SubscribeLocalEvent<StealthAnomalyComponent, AnomalySupercriticalEvent>(OnSupercritical);
        // SubscribeLocalEvent<StealthAnomalyComponent, OnSpawnAn>(OnSupercritical);
    }

    private void OnPulse(EntityUid uid, StealthAnomalyComponent component, ref AnomalyPulseEvent args)
    {
        var xform = Transform(uid);
        var stealthingRadius = component.maximumStealthingRadius * args.Stability;
        StealthNearBy(uid, xform.Coordinates, args.Severity, stealthingRadius);
    }



    private void OnSupercritical(EntityUid uid, StealthAnomalyComponent component, ref AnomalySupercriticalEvent args)
    {
        var xform = Transform(uid);
        StealthNearBy(uid, xform.Coordinates, 1, component.maximumStealthingRadius * 2);

        return;
    }

    private void StealthNearBy(EntityUid uid, EntityCoordinates coordinates, float severity, float range)
    {

        var targetEntities = new HashSet<EntityUid>();
        targetEntities = _entityLookup.GetEntitiesInRange(coordinates, range * (severity / 100f + 1f), LookupFlags.Uncontained);
        // var StealthedEntities = new HashSet<EntityUid<StealthComponent>>();
        // StealthedEntities = _entityLookup.GetEntitiesInRange(coordinates, 15, LookupFlags.Uncontained);
        var targetEntitiesStructures = new HashSet<EntityUid>();
        foreach (var targetEnt in targetEntities)
        {
            if (_entities.HasComponent<StealthComponent>(targetEnt))
                continue;

            _entities.AddComponent<StealthComponent>(targetEnt);
            _entities.AddComponent<StealthOnMoveComponent>(targetEnt);

            // _stealthable.SetEnabled(targetEnt, true, null);
            // если стена видима, то нужно ей изменить параметры
            // if (_entities.HasComponent<Structable>)
            {
                // targetEntitiesStructures.Add(targetEnt);
                // функцияПоменятьРендерНаПрозрачный!
            }

            // зафлешить всех
            // TODO сделать ЗАВИСИМОСТЬ ОТ ПРОЦЕНТА МАССЫ АНОМЫ!
            if (_entities.HasComponent<FlashableComponent>(targetEnt))
                _flashSystem.Flash(targetEnt, uid, uid, 2000f * severity, 0.8f * (1f / severity), false, _entities.GetComponent<FlashableComponent>(targetEnt), false);
        }

        // убираем со всех компонентов стелс со стелса и структур
        // await Task.Delay(10);
        Timer.Spawn(TimeSpan.FromSeconds(30), () => OnTimeOutDeleteComponent(uid, ref targetEntities, ref targetEntitiesStructures), default);

        return;
    }

    public void OnTimeOutDeleteComponent(EntityUid uid, ref HashSet<EntityUid> entities, ref HashSet<EntityUid> entitiesStructures)
    {

        // делаем затухание:

        // TimerExtensions.AddTimer(uid, timer, default);
        // делаем стены снова просвечиваевыемися
        // OnTimeOutChangeComponent(targetEntitiesStructures, Time)            // _stealthable.OnGetVisibilityModifiers();
        foreach (var targetEnt in entities)
        {
            if (_entities.HasComponent<StealthComponent>(targetEnt))
            {
                _entities.RemoveComponent<StealthOnMoveComponent>(targetEnt);

                _entities.RemoveComponent<StealthComponent>(targetEnt);
            }

        }
        return;
    }
}
