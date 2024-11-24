namespace MMDarkness.Editor
{
    public struct StartTimePreviewPointer : IDirectableTimePointer
    {
        private bool triggered;
        private float lastTargetStartTime;
        public PreviewLogic target { get; }
        float IDirectableTimePointer.time => target.Directable.StartTime;

        public StartTimePreviewPointer(PreviewLogic target)
        {
            this.target = target;
            triggered = false;
            lastTargetStartTime = target.Directable.StartTime;
        }


        void IDirectableTimePointer.TriggerForward(float currentTime, float previousTime)
        {
            if (!target.Directable.IsActive) return;
            if (currentTime >= target.Directable.StartTime)
                if (!triggered)
                {
                    triggered = true;
                    var farmedata = new FrameData
                    {
                        currentTime = currentTime,
                        previousTime = previousTime,
                        deltaTime = currentTime - previousTime
                    };

                    var LocalTime = target.Directable.ToLocalTime(currentTime);
                    var innerframedata = new FrameData
                        { previousTime = 0, currentTime = LocalTime, deltaTime = LocalTime };
                    target.Enter(farmedata, innerframedata);

                    target.Update(farmedata, innerframedata);
                }
        }

        void IDirectableTimePointer.Update(float currentTime, float previousTime)
        {
            if (!target.Directable.IsActive) return;
            if (currentTime >= target.Directable.StartTime && currentTime < target.Directable.EndTime &&
                currentTime > 0)
            {
                var farmedata = new FrameData
                {
                    currentTime = currentTime,
                    previousTime = previousTime,
                    deltaTime = currentTime - previousTime
                };
                var deltaMoveClip = target.Directable.StartTime - lastTargetStartTime;
                var localCurrentTime = target.Directable.ToLocalTime(currentTime);
                var localPreviousTime = target.Directable.ToLocalTime(previousTime + deltaMoveClip);

                var innerframedata = new FrameData
                {
                    previousTime = localPreviousTime, currentTime = localCurrentTime,
                    deltaTime = localCurrentTime - localPreviousTime
                };
                target.Update(farmedata, innerframedata);
                lastTargetStartTime = target.Directable.StartTime;
            }
        }

        void IDirectableTimePointer.TriggerBackward(float currentTime, float previousTime)
        {
            if (!target.Directable.IsActive) return;
            if (currentTime < target.Directable.StartTime || currentTime <= 0)
                if (triggered)
                {
                    triggered = false;

                    var farmedata = new FrameData
                    {
                        currentTime = currentTime,
                        previousTime = previousTime,
                        deltaTime = previousTime - currentTime
                    };
                    var LocalTime = target.Directable.ToLocalTime(previousTime);
                    var innerframedata = new FrameData
                        { previousTime = LocalTime, currentTime = 0, deltaTime = LocalTime };
                    target.Update(farmedata, innerframedata);
                    target.Reverse(farmedata, innerframedata);
                }
        }
    }

    public struct EndTimePreviewPointer : IDirectableTimePointer
    {
        private bool triggered;
        public PreviewLogic target { get; }
        float IDirectableTimePointer.time => target.Directable.EndTime;

        public EndTimePreviewPointer(PreviewLogic target)
        {
            this.target = target;
            triggered = false;
        }

        void IDirectableTimePointer.TriggerForward(float currentTime, float previousTime)
        {
            if (!target.Directable.IsActive) return;
            if (currentTime >= target.Directable.EndTime)
                if (!triggered)
                {
                    triggered = true;
                    var farmedata = new FrameData
                    {
                        currentTime = currentTime,
                        previousTime = previousTime,
                        deltaTime = currentTime - previousTime
                    };
                    var localpreviousTime = target.Directable.ToLocalTime(previousTime);
                    var innerframedata = new FrameData
                    {
                        currentTime = target.Directable.GetLength(),
                        previousTime = localpreviousTime,
                        deltaTime = currentTime - previousTime
                    };
                    target.Update(farmedata, innerframedata);
                    target.Exit(farmedata, innerframedata);
                }
        }


        void IDirectableTimePointer.Update(float currentTime, float previousTime)
        {
        }


        void IDirectableTimePointer.TriggerBackward(float currentTime, float previousTime)
        {
            if (!target.Directable.IsActive) return;
            if (currentTime < target.Directable.EndTime || currentTime <= 0)
                if (triggered)
                {
                    triggered = false;
                    var farmedata = new FrameData
                    {
                        currentTime = currentTime,
                        previousTime = previousTime,
                        deltaTime = previousTime - currentTime
                    };
                    var localTime = target.Directable.ToLocalTime(currentTime);
                    var innerframedata = new FrameData
                    {
                        currentTime = localTime,
                        previousTime = target.Directable.GetLength(),
                        deltaTime = previousTime - currentTime
                    };
                    target.ReverseEnter(farmedata, innerframedata);
                    target.Update(farmedata, innerframedata);
                }
        }
    }
}