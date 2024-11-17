using UnityEngine;

public static class Util
{
    public static void SetActiveness(this CanvasGroup cg, bool value)
    {
        cg.alpha = value ? 1f : 0f;
        cg.blocksRaycasts = value;
    }
}
