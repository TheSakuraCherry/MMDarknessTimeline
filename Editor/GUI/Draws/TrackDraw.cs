using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MMDarkness.Editor.Draws
{
    public static class TrackDraw
    {
        private const float BOX_WIDTH = 30f;

        public static void Draw(TrackAsset trackAsset, Rect trackRect)
        {
            var e = Event.current;

            DoDefaultInfoGUI(trackAsset, e, trackRect);

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
        }

        public static void DoDefaultInfoGUI(TrackAsset trackAsset, Event e, Rect trackRect)
        {
            var dopeRect = new Rect(0, 0, 4f, trackAsset.ShowHeight);
            GUI.color = trackAsset.GetColor();
            GUI.Box(dopeRect, string.Empty, Styles.HeaderBoxStyle);
            GUI.color = Color.white;

            var iconBGRect = new Rect(4f, 0, BOX_WIDTH, trackAsset.ShowHeight);
            iconBGRect = iconBGRect.ExpandBy(-1);
            var textInfoRect = Rect.MinMaxRect(iconBGRect.xMax + 2, 0, trackRect.width - BOX_WIDTH - 2,
                trackAsset.ShowHeight);
            var curveButtonRect = new Rect(trackRect.width - BOX_WIDTH, 0, BOX_WIDTH, trackAsset.ShowHeight);

            GUI.color = Color.black.WithAlpha(0.1f);
            GUI.DrawTexture(iconBGRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            var icon = trackAsset.GetIcon();
            if (icon != null)
            {
                var iconRect = new Rect(0, 0, 16, 16);
                iconRect.center = iconBGRect.center;
                GUI.color = ReferenceEquals(DirectorUtility.SelectedObject, trackAsset)
                    ? Color.white
                    : new Color(1, 1, 1, 0.8f);
                GUI.DrawTexture(iconRect, icon);
                GUI.color = Color.white;
            }

            var nameString = $"<size=11>{trackAsset.Name}</size>";
            var infoString = $"<size=9><color=#707070>{trackAsset.info}</color></size>";
            GUI.color = trackAsset.IsActive ? Color.white : Color.grey;
            GUI.Label(textInfoRect, $"{nameString}\n{infoString}");
            GUI.color = Color.white;


            var wasEnable = GUI.enabled;
            GUI.enabled = true;

            GUI.color = Color.grey;
            if (!trackAsset.IsActive)
            {
                var hiddenRect = new Rect(0, 0, 16, 16)
                {
                    center = curveButtonRect.center
                };
                if (GUI.Button(hiddenRect, Styles.HiddenIcon, GUIStyle.none))
                    trackAsset.IsActive = !trackAsset.IsActive;
            }

            if (trackAsset.IsLocked)
            {
                var lockRect = new Rect(0, 0, 16, 16)
                {
                    center = curveButtonRect.center
                };
                if (!trackAsset.IsActive) lockRect.center -= new Vector2(16, 0);

                if (GUI.Button(lockRect, Styles.LockIcon, GUIStyle.none)) trackAsset.IsLocked = !trackAsset.IsLocked;
            }

            GUI.color = Color.white;
            GUI.enabled = wasEnable;
        }

        public static void DrawTrackContextMenu(TrackAsset trackAsset, Event e, Rect posRect, float cursorTime)
        {
            var clipsPosRect = Rect.MinMaxRect(posRect.xMin, posRect.yMin, posRect.xMax,
                posRect.yMin + trackAsset.ShowHeight);
            if (e.type == EventType.ContextClick && clipsPosRect.Contains(e.mousePosition))
            {
                var attachableTypeInfos = new List<EditorTools.TypeMetaInfo>();

                var existing = trackAsset.Clips.FirstOrDefault();
                var existingCatAtt =
                    existing?.GetType().GetCustomAttributes(typeof(CategoryAttribute), true).FirstOrDefault() as
                        CategoryAttribute;

                foreach (var clip in EditorTools.GetTypeMetaDerivedFrom(typeof(Clip)))
                {
                    if (!clip.AttachableTypes?.Contains(trackAsset.trackModel.GetType()) ?? true) continue;

                    if (existingCatAtt != null)
                    {
                        if (existingCatAtt.Category == clip.Category) attachableTypeInfos.Add(clip);
                    }
                    else
                    {
                        attachableTypeInfos.Add(clip);
                    }
                }

                if (attachableTypeInfos.Count > 0)
                {
                    var menu = new GenericMenu();
                    foreach (var metaInfo in attachableTypeInfos)
                    {
                        var info = metaInfo;
                        var category = string.IsNullOrEmpty(info.Category) ? string.Empty : info.Category + "/";
                        var tName = info.Name;
                        menu.AddItem(new GUIContent(category + tName), false,
                            () => { trackAsset.AddClip(info.Type, cursorTime); });
                    }

                    var copyType = DirectorUtility.GetCopyType();
                    if (copyType != null && attachableTypeInfos.Select(i => i.Type).Contains(copyType))
                    {
                        menu.AddSeparator("/");
                        menu.AddItem(new GUIContent(string.Format(Lan.ClipPaste, copyType.Name)), false,
                            () => { trackAsset.PasteClip(DirectorUtility.CopyClipAsset, cursorTime); });
                    }

                    menu.ShowAsContext();
                    e.Use();
                }
            }
        }

        #region Icons

        private static readonly Dictionary<Type, Texture> _iconDictionary = new();
        private static readonly Dictionary<Type, Color> _colorDictionary = new();

        private static Texture GetIcon(this TrackAsset trackAsset)
        {
            var type = trackAsset.GetType();
            if (_iconDictionary.TryGetValue(type, out var icon)) return icon;

            var att = trackAsset.GetType().RTGetAttribute<ShowIconAttribute>(true);

            if (att != null)
            {
                if (att.Texture != null)
                {
                    icon = att.Texture;
                }
                else if (!string.IsNullOrEmpty(att.IconPath))
                {
                    if (att.IconPath.StartsWith("Assets/"))
                        icon = AssetDatabase.LoadAssetAtPath<Texture>(att.IconPath);
                    else
                        icon = Resources.Load(att.IconPath) as Texture;
                }

                if (icon == null && !string.IsNullOrEmpty(att.IconPath))
                    icon = EditorGUIUtility.FindTexture(att.IconPath);

                if (icon == null && att.FromType != null) icon = AssetPreview.GetMiniTypeThumbnail(att.FromType);
            }

            if (icon != null) _iconDictionary[type] = icon;

            return icon;
        }

        private static Color GetColor(this TrackAsset trackAsset)
        {
            var type = trackAsset.GetType();
            if (_colorDictionary.TryGetValue(type, out var icon)) return icon;

            var colorAttribute = trackAsset.GetType().GetCustomAttribute<ColorAttribute>();
            if (colorAttribute != null)
                _colorDictionary[type] = colorAttribute.Color;
            else
                _colorDictionary[type] = Color.gray;

            return _colorDictionary[type];
        }

        #endregion
    }
}