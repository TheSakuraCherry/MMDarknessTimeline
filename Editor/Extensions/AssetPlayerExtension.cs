using UnityEngine;

namespace MMDarkness.Editor
{
    public static class AssetPlayerExtension
    {
        public static Color GetScriberColor(this TimelineGraphPreviewProcessor player)
        {
            return player.IsActive ? Color.yellow : new Color(1, 0.3f, 0.3f);
        }
    }
}