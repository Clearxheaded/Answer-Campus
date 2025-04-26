using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IntPath
{
    public List<int> values;
}
[CreateAssetMenu(fileName = "PathGenerator", menuName = "Game/PathGenerator")]

public class PathGenerator : ScriptableObject
{
    public List<IntPath> presetPaths;

    public int[] GetRandomPath()
    {
        if (presetPaths == null || presetPaths.Count == 0)
            return new int[0];

        var selected = presetPaths[Random.Range(0, presetPaths.Count)];
        return selected.values.ToArray();
    }
}