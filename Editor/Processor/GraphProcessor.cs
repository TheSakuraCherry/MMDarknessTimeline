using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MMDarkness.Editor
{
    public class GraphProcessor
    {
        private static GraphProcessor m_instance;
        public static GraphProcessor Instance => m_instance ??= new GraphProcessor();
        private float m_playTimeMin;
        private float m_playTimeMax;
        private float m_currentTime;
        private bool m_preInitialized;
        private List<IDirectableTimePointer> m_timePointers;
        private List<IDirectableTimePointer> m_unsortedStartTimePointers; //预览器

        public float PreviousTime { get; private set; }
        public TimelineGraph CurrentGraph;

        public bool IsActive { get; set; }

        public bool IsPaused { get; set; }

        public float Length => CurrentGraph?.length ?? 0;

        public float CurrentTime
        {
            get => m_currentTime;
            set => m_currentTime = Mathf.Clamp(value, 0, Length);
        }

        public float PlayTimeMin
        {
            get => m_playTimeMin;
            set => m_playTimeMin = Mathf.Clamp(value, 0, PlayTimeMax);
        }

        public float PlayTimeMax
        {
            get => m_playTimeMax;
            set => m_playTimeMax = Mathf.Clamp(value, PlayTimeMin, Length);
        }

        public void Sample()
        {
            Sample(m_currentTime);
        }

        public void Sample(float time)
        {
            CurrentTime = time;
            if ((m_currentTime == 0 || m_currentTime == Length) && PreviousTime == m_currentTime)
            {
                return;
            }

            if (!m_preInitialized && m_currentTime > 0 && PreviousTime == 0)
            {
                InitializePreviewPointers();
            }


            if (m_timePointers != null)
            {
                InternalSamplePointers(m_currentTime, PreviousTime);
            }

            PreviousTime = m_currentTime;
        }

        void InternalSamplePointers(float currentTime, float previousTime)
        {
            if (!Application.isPlaying || currentTime > previousTime)
            {
                foreach (var t in m_timePointers)
                {
                    try
                    {
                        t.TriggerForward(currentTime, previousTime);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }


            if (!Application.isPlaying || currentTime < previousTime)
            {
                for (var i = m_timePointers.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        m_timePointers[i].TriggerBackward(currentTime, previousTime);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            if (m_unsortedStartTimePointers != null)
            {
                foreach (var t in m_unsortedStartTimePointers)
                {
                    try
                    {
                        t.Update(currentTime, previousTime);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        /// <summary>
        /// 初始化时间指针预览器
        /// </summary>
        public void InitializePreviewPointers()
        {
            m_timePointers = new List<IDirectableTimePointer>();
            m_unsortedStartTimePointers = new List<IDirectableTimePointer>();

            Dictionary<Type, Type> typeDic = new Dictionary<Type, Type>();
            var childs = EditorTools.GetTypeMetaDerivedFrom(typeof(PreviewLogic));
            foreach (var t in childs)
            {
                var arrs = t.Type.GetCustomAttributes(typeof(CustomPreviewAttribute), true);
                foreach (var arr in arrs)
                {
                    if (arr is CustomPreviewAttribute c)
                    {
                        var bindT = c.PreviewType;
                        var iT = t.Type;
                        if (!typeDic.ContainsKey(bindT))
                        {
                            if (!iT.IsAbstract) typeDic[bindT] = iT;
                        }
                        else
                        {
                            var old = typeDic[bindT];
                            //如果不是抽象类，且是子类就更新
                            if (!iT.IsAbstract && iT.IsSubclassOf(old))
                            {
                                typeDic[bindT] = iT;
                            }
                        }
                    }
                }
            }

            /*foreach (var group in CurrentGraph.groups.AsEnumerable().Reverse())
            {
                if (!group.IsActive) continue;
                foreach (var track in group.Tracks.AsEnumerable().Reverse())
                {
                    if (!track.IsActive) continue;
                    var tType = track.GetType();
                    if (typeDic.TryGetValue(tType, out var t1))
                    {
                        if (Activator.CreateInstance(t1) is PreviewLogic preview)
                        {
                            preview.SetTarget(track);
                            var p3 = new StartTimePointer(preview);
                            m_timePointers.Add(p3);

                            m_unsortedStartTimePointers.Add(p3);
                            m_timePointers.Add(new EndTimePointer(preview));
                        }
                    }

                    foreach (var clip in track.Clips)
                    {
                        var cType = clip.GetType();
                        if (typeDic.TryGetValue(cType, out var t))
                        {
                            if (Activator.CreateInstance(t) is PreviewLogic preview)
                            {
                                preview.SetTarget(clip);
                                var p3 = new StartTimePointer(preview);
                                m_timePointers.Add(p3);

                                m_unsortedStartTimePointers.Add(p3);
                                m_timePointers.Add(new EndTimePointer(preview));
                            }
                        }
                    }
                }
            }*/

            m_preInitialized = true;
        }
    }
}
