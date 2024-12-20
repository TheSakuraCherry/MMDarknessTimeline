﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MMDarkness.Editor
{
    public static class DrawUtils
    {
        #region 可定制GUI

        /// <summary>
        ///     类型映射
        /// </summary>
        private static readonly Dictionary<Type, Type> s_customizedTypeDic = new();

        /// <summary>
        ///     类型实例映射
        /// </summary>
        private static readonly Dictionary<Type, ICustomized> s_customizedInstDic = new();

        public static void Draw<T>() where T : ICustomized
        {
            var type = typeof(T);
            if (s_customizedInstDic.TryGetValue(type, out var customized) && customized != null)
            {
                customized.OnGUI();
                return;
            }

            var subType = GetSubclassType(type);
            if (subType == null) return;
            s_customizedTypeDic[type] = subType;
            if (Activator.CreateInstance(subType) is ICustomized custom)
            {
                s_customizedInstDic[type] = custom;
                custom.OnGUI();
            }
        }

        private static Type GetSubclassType(Type type)
        {
            if (s_customizedTypeDic.TryGetValue(type, out var t)) return t;

            EditorTools.GetTypeLastSubclass(type, s_customizedTypeDic);
            if (s_customizedTypeDic.TryGetValue(type, out t)) return t;

            return null;
        }

        #endregion

        #region 绘制贴图

        private static readonly Dictionary<AudioClip, Texture2D> m_audioTextures = new();

        public static Texture2D GetAudioClipTexture(AudioClip clip, int width, int height)
        {
            if (clip == null) return null;

            width = 8192;

            if (m_audioTextures.TryGetValue(clip, out var texture))
                if (texture != null)
                    return texture;

            if (clip.loadType != AudioClipLoadType.DecompressOnLoad)
            {
                m_audioTextures[clip] = Styles.WhiteTexture;
                return null;
            }

            texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var samples = new float[clip.samples * clip.channels];
            var step = Mathf.CeilToInt(clip.samples * clip.channels / width);
            clip.GetData(samples, 0);
            var xy = new Color[width * height];
            for (var x = 0; x < width * height; x++) xy[x] = new Color(0, 0, 0, 0);

            texture.SetPixels(xy);

            var i = 0;
            while (i < width)
            {
                var barHeight = Mathf.CeilToInt(Mathf.Clamp(Mathf.Abs(samples[i * step]) * height, 0, height));
                var add = samples[i * step] > 0 ? 1 : -1;
                for (var j = 0; j < barHeight; j++)
                    texture.SetPixel(i,
                        Mathf.FloorToInt(height / 2) - Mathf.FloorToInt(barHeight / 2) * add + j * add,
                        Color.white);

                ++i;
            }

            texture.Apply();
            m_audioTextures[clip] = texture;
            return texture;
        }

        /// <summary>
        ///     绘制循环音频剪辑纹理
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="audioClip"></param>
        /// <param name="maxLength"></param>
        /// <param name="offset"></param>
        public static void DrawLoopedAudioTexture(Rect rect, AudioClip audioClip, float maxLength, float offset)
        {
            if (audioClip == null) return;

            var audioRect = rect;
            audioRect.width = audioClip.length / maxLength * rect.width;
            var t = GetAudioClipTexture(audioClip, (int)audioRect.width, (int)audioRect.height);
            if (t != null)
            {
                Handles.color = new Color(0, 0, 0, 0.2f);
                GUI.color = new Color(0.4f, 0.435f, 0.576f);
                audioRect.yMin += 2;
                audioRect.yMax -= 2;
                for (var f = offset; f < maxLength; f += audioClip.length)
                {
                    audioRect.x = f / maxLength * rect.width;
                    rect.x = audioRect.x;
                    GUI.DrawTexture(audioRect, t);
                }

                Handles.color = Color.white;
                GUI.color = Color.white;
            }
            else
            {
                Debug.Log("texture is null");
            }
        }

        /// <summary>
        ///     在 Rect 内绘制环形垂直线，并提供最大长度（带可选偏移量）
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="length"></param>
        /// <param name="maxLength"></param>
        /// <param name="offset"></param>
        public static void DrawLoopedLines(Rect rect, float length, float maxLength, float offset)
        {
            if (length != 0 && maxLength != 0)
            {
                length = Mathf.Abs(length);
                maxLength = Mathf.Abs(maxLength);
                Handles.color = new Color(0, 0, 0, 0.2f);
                for (var f = offset; f < maxLength; f += length)
                {
                    var posX = f / maxLength * rect.width;
                    Handles.DrawLine(new Vector2(posX, 0), new Vector2(posX, rect.height));
                }

                Handles.color = Color.white;
            }
        }

        #endregion
    }
}