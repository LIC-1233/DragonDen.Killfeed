using System;
using System.Reflection;
using Comfort.Common;
using DragonDen.Killfeed.Models;
using DragonDen.Killfeed.Utilities;
using EFT;
using EFT.InventoryLogic;
using EFT.HealthSystem;
using SPT.Reflection.Patching;
using UnityEngine;

namespace DragonDen.Killfeed.Patches;

public class ActiveHealthApplyDamagePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ActiveHealthController).GetMethod(
            "ApplyDamage",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(EBodyPart), typeof(float), typeof(DamageInfoStruct) },
            null
        );
    }

    static string ResolveWeaponLabel(Item weaponItem)
    {
        if (weaponItem is Weapon w)
        {
            var factory = Singleton<ItemFactoryClass>.Instance;
            return factory.BriefItemName(w, w.ShortName.Localized());
        }

        return string.Empty;
    }

    static string ResolveAmmoName(string ammoTpl)
    {
        if (string.IsNullOrEmpty(ammoTpl)) return string.Empty;
        var db = Singleton<ItemFactoryClass>.Instance.ItemTemplates;
        if (db.TryGetValue((MongoID)ammoTpl, out var tpl))
            return tpl.ShortNameLocalizationKey.Localized();
        return ammoTpl;
    }

    [PatchPostfix]
    static void Postfix(ActiveHealthController __instance, EBodyPart __0, float __1, DamageInfoStruct __2)
    {
        var bodyPart = __0;
        var damageArg = __1;
        var di = __2;

        var victim = __instance.Player;
        if (victim == null) return;

        var attacker = di.Player != null ? di.Player.iPlayer as Player : null;

        float didBody = Mathf.Max(0f, di.DidBodyDamage);
        float didArmor = Mathf.Max(0f, di.DidArmorDamage);
        if (didBody <= 0.0001f && damageArg > 0f) didBody = damageArg;

        bool blocked = di.BlockedBy.HasValue;
        bool deflected = di.DeflectedBy.HasValue;

        bool armorOnly = (didArmor > 0.01f && didBody <= 0.01f) || blocked || deflected;

        string weaponLabel = di.Weapon != null ? ResolveWeaponLabel(di.Weapon) : string.Empty;
        string ammoName = ResolveAmmoName(di.SourceId);

        float dist = 0f;
        try
        {
            Vector3 hit = di.HitPoint;
            if (attacker != null && hit != default)
                dist = Vector3.Distance(attacker.Transform.position, hit);
            else if (attacker != null && victim != null)
                dist = Vector3.Distance(attacker.Transform.position, victim.Transform.position);
        }
        catch
        {
            dist = 0f;
        }

        var e = new DamageEvent
        {
            AttackerId = attacker?.Profile?.Id,
            AttackerName = attacker?.Profile?.Nickname ?? "Unknown",
            AttackerSide = attacker != null ? attacker.Side : EPlayerSide.Savage,
            VictimId = victim.Profile?.Id,
            VictimName = victim.Profile?.Nickname ?? "Unknown",
            BodyPart = bodyPart.ToString(),
            DamageAmount = damageArg,
            BodyDamage = didBody,
            ArmorDamage = didArmor,
            IsLocalAttacker = attacker != null && ReferenceEquals(attacker, State.LocalPlayer),
            IsLocalVictim = ReferenceEquals(victim, State.LocalPlayer),
            IsHeadshot = bodyPart == EBodyPart.Head,
            VictimIsDead = victim.HealthController != null && victim.HealthController.IsAlive == false,
            WorldPos = di.HitPoint,
            WeaponLabel = weaponLabel,
            AmmoName = ammoName,
            IsArmorHit = armorOnly,
            Ricochet = deflected,
            Blocked = blocked,
            DistanceMeters = dist
        };

        if (e.VictimIsDead) EventBus.RaiseKill(e);
    }
}