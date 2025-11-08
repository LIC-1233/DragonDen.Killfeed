using Comfort.Common;
using EFT;
using UnityEngine;

namespace DragonDen.Killfeed.Utilities;

public static class State
{
    public static GameWorld World => Singleton<GameWorld>.Instance;
    public static Player LocalPlayer => World?.MainPlayer;
}