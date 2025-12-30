#nullable enable
using System;
using UnityEngine.LowLevel;

public static class PlayerLoopExtensions
{
    /// <summary>
    /// Appends the given <paramref name="function"/> as a subsystem to the provided <paramref name="system"/>.
    /// </summary>
    public static int AddCallback(this ref PlayerLoopSystem system, Action function)
    {
        int index = system.subSystemList.Length;
        PlayerLoopSystem[] subsystemList = new PlayerLoopSystem[index + 1];
        Array.Copy(system.subSystemList, subsystemList, system.subSystemList.Length);
        ref PlayerLoopSystem newCallbackSystem = ref subsystemList[index];
        newCallbackSystem.updateDelegate = new(function);
        system.subSystemList = subsystemList;
        return index;
    }

    /// <summary>
    /// Removes a subsystem at the given <paramref name="index"/> from the provided <paramref name="system"/>.
    /// </summary>
    public static void RemoveCallback(this ref PlayerLoopSystem system, int index)
    {
        if (index < 0 || index >= system.subSystemList.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        
        PlayerLoopSystem[] subsystemList = new PlayerLoopSystem[system.subSystemList.Length - 1];
        if (index > 0)
        {
            Array.Copy(system.subSystemList, 0, subsystemList, 0, index);
        }

        if (index < system.subSystemList.Length - 1)
        {
            Array.Copy(system.subSystemList, index + 1, subsystemList, index, system.subSystemList.Length - index - 1);
        }

        system.subSystemList = subsystemList;
    }
}