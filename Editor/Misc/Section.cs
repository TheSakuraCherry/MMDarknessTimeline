﻿using UnityEngine;

namespace MMDarkness.Editor
{
    public class Section
    {
        public static readonly Color DEFAULT_COLOR = Color.black.WithAlpha(0.3f);

        [SerializeField] private Color _color = DEFAULT_COLOR;

        [SerializeField] private float _time;


        public float time
        {
            get => _time;
            set => _time = value;
        }


        public Color color
        {
            get => _color.a > 0.1f ? _color : DEFAULT_COLOR;
            set => _color = value;
        }
    }
}