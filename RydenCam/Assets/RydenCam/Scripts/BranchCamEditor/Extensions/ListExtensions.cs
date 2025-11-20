using System.Collections.Generic;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Extensions
{
    public static class ListExtensions
    {
        public static bool TryGet<T>(List<T> list, int index, out T value)
        {
            if (index >= 0 && index < list.Count)
            {
                value = list[index];
                return true;
            }

            value = default;
            return false;
        }
    }
}