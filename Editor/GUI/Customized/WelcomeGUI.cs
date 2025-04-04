﻿using UnityEngine;

namespace MMDarkness.Editor
{
    public class WelcomeGUI : ICustomized
    {
        public virtual void OnGUI()
        {
            var label = $"<size=30><b>{Lan.Title}</b></size>";
            var size = new GUIStyle("label").CalcSize(new GUIContent(label));

            var titleRect = new Rect(0, 0, size.x, size.y)
            {
                center = new Vector2(G.ScreenWidth / 2, G.ScreenHeight / 2 - size.y)
            };

            var iconRect = new Rect(0, 0, 128, 128)
            {
                center = new Vector2(G.ScreenWidth / 2, titleRect.yMin - 70)
            };
            DrawLogo(iconRect);

            GUI.Label(titleRect, label);

            var buttonRect = new Rect(0, 0, size.x, size.y);
            var next = 0;

            GUI.backgroundColor = new Color(0.8f, 0.8f, 1, 1f);
            buttonRect.center = new Vector2(G.ScreenWidth / 2, G.ScreenHeight / 2 + (size.y + 2) * next);
            next++;
            if (GUI.Button(buttonRect, Lan.CreateAsset)) CreateAssetWindow.Show();

            GUI.backgroundColor = Color.white;

            buttonRect.center = new Vector2(G.ScreenWidth / 2, G.ScreenHeight / 2 + (size.y + 2) * next);
            next++;
            if (GUI.Button(buttonRect, Lan.SelectAsset))
                ObjectSelectorWindow.ShowObjectPicker<TimelineGraphAsset>(null, App.OnObjectPickerConfig, "Assets/");

            buttonRect.center = new Vector2(G.ScreenWidth / 2, G.ScreenHeight / 2 + (size.y + 2) * next);
            if (GUI.Button(buttonRect, Lan.Seeting))
                PreferencesWindow.Show(new Rect(G.ScreenWidth - 5 - 400, Styles.ToolbarHeight + 5, 400,
                    G.ScreenHeight - Styles.ToolbarHeight - 50));
        }

        protected virtual void DrawLogo(Rect rect)
        {
            GUI.DrawTexture(rect, Styles.Logo);
        }
    }
}