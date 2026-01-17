using UnityEngine;

namespace AIOHHF.Mono;

public static class Extentions
{
    /// <summary>
    /// Copies a transform positional data to another transform
    /// </summary>
    /// <param name="source">The transform to be copied</param>
    /// <param name="target">The transform to copy to</param>
    public static void CopyTransformPosition(this Transform source, Transform target)
    {
        target.position = source.position;
        target.rotation = source.rotation;
        target.eulerAngles = source.eulerAngles;
    }
    /// <summary>
    /// Copies a transform's local positional data to another transform
    /// </summary>
    /// <param name="source">The transform to be copied</param>
    /// <param name="target">The transform to copy to</param>
    public static void CopyTransformLocalPosition(this Transform source, Transform target)
    {
        target.localPosition = source.localPosition;
        target.localRotation = source.localRotation;
        target.localEulerAngles = source.localEulerAngles;
    }
    public static void CopyTransformToLocalPosition(this Transform source, Transform target)
    {
        target.localPosition = source.position;
        target.localRotation = source.rotation;
        target.localEulerAngles = source.eulerAngles;
    }
}