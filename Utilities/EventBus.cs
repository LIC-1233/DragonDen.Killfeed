using System;
using DragonDen.Killfeed.Models;
using UnityEngine;

namespace DragonDen.Killfeed.Utilities;

public static class EventBus
{
    public static event Action<DamageEvent> OnKill;

    public static void RaiseKill(DamageEvent e)
    {
        e.Time = Time.unscaledTime;
        OnKill?.Invoke(e);
    }
}