using System.Collections.Generic;
using UnityEngine;

public class CheerInputBridge : MonoBehaviour
{
    public static CheerInputBridge Instance { get; private set; }

    private Queue<CheerDirection> inputQueue = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnPressUp()    => EnqueueDirection(CheerDirection.Up);
    public void OnPressDown()  => EnqueueDirection(CheerDirection.Down);
    public void OnPressLeft()  => EnqueueDirection(CheerDirection.Left);
    public void OnPressRight() => EnqueueDirection(CheerDirection.Right);

    private void EnqueueDirection(CheerDirection dir)
    {
        Debug.Log($"[DEBUG] Enqueued {dir} at DSP: {AudioSettings.dspTime:F2}");

        inputQueue.Enqueue(dir);
        Debug.Log($"[INPUT] Queued: {dir}");
    }

    public bool TryGetNextDirection(out CheerDirection dir)
    {
        if (inputQueue.Count > 0)
        {
            dir = inputQueue.Dequeue();
            return true;
        }

        dir = default;
        return false;
    }

    public void Clear()
    {
        inputQueue.Clear();
    }
}