using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MMDarkness
{
    internal static class CreateUtilities
    {
        public static void SaveAssetIntoObject(Object childAsset, Object masterAsset)
        {
            if (childAsset == null || masterAsset == null)
                return;

            if ((masterAsset.hideFlags & HideFlags.DontSave) != 0)
            {
                childAsset.hideFlags |= HideFlags.DontSave;
            }
            else
            {
                childAsset.hideFlags |= HideFlags.HideInHierarchy;
#if UNITY_EDITOR
                if (!AssetDatabase.Contains(childAsset) && AssetDatabase.Contains(masterAsset))
                    AssetDatabase.AddObjectToAsset(childAsset, masterAsset);
#endif
            }
        }
    }
}