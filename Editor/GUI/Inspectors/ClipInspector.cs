using UnityEditor;
using UnityEngine;

namespace MMDarkness.Editor
{
    public abstract class ClipInspector<T> : ClipInspector where T : ClipAsset
    {
        protected T Clip => (T)m_target;
    }

    [CustomInspectors(typeof(ClipAsset), true)]
    public class ClipInspector : InspectorsBase
    {
        private ClipAsset m_clip => (ClipAsset)m_target;

        public override void OnInspectorGUI()
        {
            ShowCommonInspector();
        }

        protected void ShowCommonInspector(bool showBaseInspector = true)
        {
            ShowErrors();
            ShowInOutControls();
            ShowBlendingControls();
            if (showBaseInspector)
            {
                base.OnInspectorGUI();
            }
        }

        void ShowErrors()
        {
            if (m_clip.IsValid) return;
            EditorGUILayout.HelpBox("该剪辑无效。 请确保设置了所需的参数。", MessageType.Error);
            GUILayout.Space(5);
        }

        void ShowInOutControls()
        {
            var previousClip = m_clip.GetPreviousSibling();
            var previousTime = previousClip ? previousClip.EndTime : m_clip.Parent.StartTime;
            if (m_clip.CanCrossBlend(previousClip))
            {
                previousTime -= Mathf.Min(m_clip.Length / 2, (previousClip.EndTime - previousClip.StartTime) / 2);
            }

            var nextClip = m_clip.GetNextSibling();
            var nextTime = nextClip != null ? nextClip.StartTime : m_clip.Parent.EndTime;
            if (m_clip.CanCrossBlend(nextClip))
            {
                nextTime += Mathf.Min(m_clip.Length / 2, (nextClip.EndTime - nextClip.StartTime) / 2);
            }

            var canScale = m_clip.CanScale();
            var doFrames = Prefs.timeStepMode == Prefs.TimeStepMode.Frames;

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();

            var _in = m_clip.StartTime;
            var _length = m_clip.Length;
            var _out = m_clip.EndTime;

            if (canScale)
            {
                GUILayout.Label("IN", GUILayout.Width(30));
                if (doFrames)
                {
                    _in *= Prefs.frameRate;
                    _in = EditorGUILayout.DelayedIntField((int)_in, GUILayout.Width(80));
                    _in *= (1f / Prefs.frameRate);
                }
                else
                {
                    _in = EditorGUILayout.DelayedFloatField(_in, GUILayout.Width(80));
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label("◄");
                if (doFrames)
                {
                    _length *= Prefs.frameRate;
                    _length = EditorGUILayout.DelayedIntField((int)_length, GUILayout.Width(80));
                    _length *= (1f / Prefs.frameRate);
                }
                else
                {
                    _length = EditorGUILayout.DelayedFloatField(_length, GUILayout.Width(80));
                }

                GUILayout.Label("►");
                GUILayout.FlexibleSpace();

                GUILayout.Label("OUT", GUILayout.Width(30));
                if (doFrames)
                {
                    _out *= Prefs.frameRate;
                    _out = EditorGUILayout.DelayedIntField((int)_out, GUILayout.Width(80));
                    _out *= (1f / Prefs.frameRate);
                }
                else
                {
                    _out = EditorGUILayout.DelayedFloatField(_out, GUILayout.Width(80));
                }
            }

            GUILayout.EndHorizontal();

            if (canScale)
            {
                if (_in >= m_clip.Parent.StartTime && _out <= m_clip.Parent.EndTime)
                {
                    if (_out > _in)
                    {
                        EditorGUILayout.MinMaxSlider(ref _in, ref _out, previousTime, nextTime);
                    }
                    else
                    {
                        _in = EditorGUILayout.Slider(_in, previousTime, nextTime);
                        _out = _in;
                    }
                }
            }
            else
            {
                GUILayout.Label("IN", GUILayout.Width(30));
                _in = EditorGUILayout.Slider(_in, 0, m_clip.Parent.EndTime);
                _out = _in;
            }


            if (GUI.changed)
            {
                if (_length != m_clip.Length)
                {
                    _out = _in + _length;
                }

                _in = Mathf.Round(_in / Prefs.snapInterval) * Prefs.snapInterval;
                _out = Mathf.Round(_out / Prefs.snapInterval) * Prefs.snapInterval;

                _in = Mathf.Clamp(_in, previousTime, _out);
                _out = Mathf.Clamp(_out, _in, nextClip != null ? nextTime : float.PositiveInfinity);

                m_clip.StartTime = _in;
                m_clip.EndTime = _out;
            }

            if (_in > m_clip.Parent.EndTime)
            {
                EditorGUILayout.HelpBox(Lan.OverflowInvalid, MessageType.Warning);
            }
            else
            {
                if (_out > m_clip.Parent.EndTime)
                {
                    EditorGUILayout.HelpBox(Lan.EndTimeOverflowInvalid, MessageType.Warning);
                }
            }

            if (_out < m_clip.Parent.StartTime)
            {
                EditorGUILayout.HelpBox(Lan.OverflowInvalid, MessageType.Warning);
            }
            else
            {
                if (_in < m_clip.Parent.StartTime)
                {
                    EditorGUILayout.HelpBox(Lan.StartTimeOverflowInvalid, MessageType.Warning);
                }
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// 显示混合输入/输出控件
        /// </summary>
        void ShowBlendingControls()
        {
            var canBlendIn = m_clip.CanBlendIn();
            var canBlendOut = m_clip.CanBlendOut();
            if ((canBlendIn || canBlendOut) && m_clip.Length > 0)
            {
                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                if (canBlendIn)
                {
                    GUILayout.BeginVertical();
                    GUILayout.Label("Blend In");
                    var max = m_clip.Length - m_clip.BlendOut;
                    m_clip.BlendIn = EditorGUILayout.Slider(m_clip.BlendIn, 0, max);
                    m_clip.BlendIn = Mathf.Clamp(m_clip.BlendIn, 0, max);
                    GUILayout.EndVertical();
                }

                if (canBlendOut)
                {
                    GUILayout.BeginVertical();
                    GUILayout.Label("Blend Out");
                    var max = m_clip.Length - m_clip.BlendIn;
                    m_clip.BlendOut = EditorGUILayout.Slider(m_clip.BlendOut, 0, max);
                    m_clip.BlendOut = Mathf.Clamp(m_clip.BlendOut, 0, max);
                    GUILayout.EndVertical();
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }
    }
}