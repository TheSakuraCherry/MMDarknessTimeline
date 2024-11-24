#if USE_FIXED_POINT
using CMath = Box2DSharp.Common.FMath;
using CFloat = Box2DSharp.Common.FP;
#else
using CMath = System.Math;
using CFloat = System.Single;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MMDarkness
{
    [ViewModel(typeof(TimelineGraph))]
    public class TimelineGraphProcessor : ITimelineGraph
    {
        private BlackboardProcessor<string> context;

        private CFloat currentTime;
        private TimelineGraph data;
        private CFloat endTime;
        private Events<string> events;
        private readonly List<GroupProcessor> groups;

        private bool m_preInitialized;

        private List<IDirectableTimePointer> m_timePointers;
        private List<IDirectableTimePointer> m_unsortedStartTimePointers;
        private Action onStop;

        private CFloat startTime;

        public TimelineGraphProcessor(TimelineGraph data)
        {
            this.data = data;
            if (data.groups != null)
            {
                groups = new List<GroupProcessor>(data.groups.Count);
                foreach (var group in data.groups)
                {
                    var groupProcessorType = ViewModelFactory.GetViewModelType(group.GetType());
                    if (ObjectPools.Instance.Spawn(groupProcessorType) is GroupProcessor groupProcessor)
                    {
                        groupProcessor.SetUp(group, this);
                        groups.Add(groupProcessor);
                    }
                }
            }
        }

        public WarpCategory PlayingWarpMode => data.warpCategory;

        public CFloat StartTime
        {
            get => startTime;
            private set => SetStartTime(value);
        }

        public CFloat EndTime
        {
            get => endTime;
            private set => SetEndTime(value);
        }

        /// <summary>
        ///     当前时间
        /// </summary>
        public CFloat CurrentTime
        {
            get => currentTime;
            set => SetCurrentTime(value);
        }

        public IEnumerable<IDirectable> Children => groups != null ? groups : Array.Empty<IDirectable>();

        public BlackboardProcessor<string> Context
        {
            get
            {
                if (context == null) context = new BlackboardProcessor<string>(new Blackboard<string>(), Events);

                return context;
            }
        }

        public Events<string> Events
        {
            get
            {
                if (events == null) events = new Events<string>();

                return events;
            }
        }

        public bool Active { get; private set; }
        public CFloat Length => data.length;
        public CFloat PreviousTime { get; private set; }

        public GameObject Owner { get; set; }


        public void Reset()
        {
            if (!Active) return;

            foreach (var child in Children) child.Reset();

            context?.Clear();
            events?.Clear();
        }


        public void Dispose()
        {
            Stop();
            foreach (var child in Children) child.Dispose();

            data = null;
            groups.Clear();
            context?.Clear();
            events?.Clear();
        }

        private void SetStartTime(CFloat value)
        {
            startTime = CMath.Clamp(value, 0, Length);
        }

        private void SetEndTime(CFloat value)
        {
            endTime = CMath.Clamp(value, 0, Length);
        }

        private void SetCurrentTime(CFloat value)
        {
            currentTime = CMath.Clamp(value, 0, Length);
        }


        public void Play()
        {
            Play(0, data.length, null);
        }

        public void Play(CFloat start)
        {
            Play(start, data.length, null);
        }

        public void Play(CFloat start, Action stopCallback)
        {
            Play(start, data.length, stopCallback);
        }

        public void Play(CFloat start, CFloat end, Action stopCallback)
        {
            if (Active) return;

            if (start > end) return;

            StartTime = start;
            EndTime = end;

            Active = true;
            StartTime = start;
            EndTime = end;
            PreviousTime = CMath.Clamp(start, start, end);
            CurrentTime = CMath.Clamp(start, start, end);
            onStop = stopCallback;

            Sample(currentTime);
        }


        public void Sample(CFloat time)
        {
            CurrentTime = time;
            if ((currentTime == 0 || Mathf.Approximately(currentTime, Length)) &&
                Mathf.Approximately(PreviousTime, currentTime)) return;

            if (!m_preInitialized && currentTime > 0 && PreviousTime == 0) InitializePreviewPointers();


            if (m_timePointers != null) InternalSamplePointers(currentTime, PreviousTime);

            switch (PlayingWarpMode)
            {
                case WarpCategory.Once:

                    if (CurrentTime < EndTime)
                        PreviousTime = CurrentTime;
                    else
                        Stop();

                    break;
                case WarpCategory.Loop:
                    if (CurrentTime < EndTime)
                    {
                        PreviousTime = CurrentTime;
                    }
                    else
                    {
                        PreviousTime = StartTime;
                        CurrentTime = StartTime;
                    }

                    break;
            }
        }

        private void InternalSamplePointers(CFloat currentTime, CFloat previousTime)
        {
            if (currentTime > previousTime)
                foreach (var t in m_timePointers)
                    try
                    {
                        t.TriggerForward(currentTime, previousTime);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }


            if (currentTime < previousTime)
                for (var i = m_timePointers.Count - 1; i >= 0; i--)
                    try
                    {
                        m_timePointers[i].TriggerBackward(currentTime, previousTime);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

            if (m_unsortedStartTimePointers != null)
                foreach (var t in m_unsortedStartTimePointers)
                    try
                    {
                        t.Update(currentTime, previousTime);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
        }

        /// <summary>
        ///     初始化时间指针
        /// </summary>
        public void InitializePreviewPointers()
        {
            m_timePointers = new List<IDirectableTimePointer>();
            m_unsortedStartTimePointers = new List<IDirectableTimePointer>();

            foreach (var group in groups.AsEnumerable().Reverse())
            foreach (var track in group.Children.AsEnumerable().Reverse())
            {
                var trackp3 = new StartTimePointer(track);
                m_timePointers.Add(trackp3);
                m_unsortedStartTimePointers.Add(trackp3);
                m_timePointers.Add(new EndTimePointer(track));

                foreach (var clip in track.Children)
                {
                    var clipp3 = new StartTimePointer(clip);
                    m_timePointers.Add(clipp3);

                    m_unsortedStartTimePointers.Add(clipp3);
                    m_timePointers.Add(new EndTimePointer(clip));
                }
            }

            m_preInitialized = true;
        }

        public void Stop(StopMode stopMode = StopMode.Exit)
        {
            if (!Active)
                return;

            Active = false;

            switch (stopMode)
            {
                case StopMode.Exit:
                {
                    foreach (var group in groups)
                    {
                        if (!group.IsTriggered)
                            continue;

                        var frameData = new FrameData
                        {
                            currentTime = CurrentTime,
                            previousTime = PreviousTime,
                            deltaTime = CurrentTime - PreviousTime
                        };

                        var localCurrentTime = CMath.Clamp(CurrentTime - group.StartTime, 0, group.Length);
                        var localPreviousTime = CMath.Clamp(PreviousTime - group.StartTime, 0, group.Length);

                        var innerframedata = new FrameData
                        {
                            previousTime = localPreviousTime, currentTime = localCurrentTime,
                            deltaTime = localCurrentTime - localPreviousTime
                        };
                        group.Exit(frameData, innerframedata);
                    }

                    break;
                }
                case StopMode.Skip:
                {
                    Sample(endTime);
                    break;
                }
            }

            if (onStop != null)
            {
                onStop.Invoke();
                onStop = null;
            }

            CurrentTime = 0;
            PreviousTime = 0;
        }
    }
}