using UnityEngine;

#if USE_FIXED_POINT
using CMath = Box2DSharp.Common.FMath;
using CFloat = Box2DSharp.Common.FP;

#else
using CMath = System.Math;
using CFloat = System.Single;
#endif

namespace MMDarkness
{
    public interface IDirectableTimePointer
    {
        CFloat time { get; }
        void TriggerForward(CFloat currentTime, CFloat previousTime);
        void TriggerBackward(CFloat currentTime, CFloat previousTime);
        void Update(CFloat currentTime, CFloat previousTime);
    }

    public struct StartTimePointer : IDirectableTimePointer
    {
        private bool triggered;
        public IDirectable target { get; private set; }
        CFloat IDirectableTimePointer.time => target.StartTime;

        public StartTimePointer(IDirectable target)
        {
            this.target = target;
            triggered = false;
        }
        

        void IDirectableTimePointer.TriggerForward(CFloat currentTime, CFloat previousTime)
        {
            if (currentTime >= target.StartTime)
            {
                if (!triggered)
                {
                    triggered = true;
                    
                    var farmedata = new FrameData()
                    {
                        currentTime = currentTime,
                        previousTime = previousTime,
                        deltaTime = currentTime - previousTime
                    };
                    
                    var LocalTime = CMath.Clamp(currentTime - target.StartTime, 0, target.Length);
                    var innerframedata = new FrameData()
                        { previousTime = 0, currentTime = LocalTime, deltaTime = LocalTime };
                    target.Enter(farmedata,innerframedata);
                    
                    target.Update(farmedata,innerframedata);
                }
            }
        }

        void IDirectableTimePointer.Update(CFloat currentTime, CFloat previousTime)
        {
            if (currentTime >= target.StartTime && currentTime < target.EndTime &&
                currentTime > 0)
            {
                var farmedata = new FrameData()
                {
                    currentTime = currentTime,
                    previousTime = previousTime,
                    deltaTime = CMath.Abs(currentTime-previousTime)
                };
                var localCurrentTime = CMath.Clamp(currentTime - target.StartTime, 0, target.Length);
                var localPreviousTime = CMath.Clamp(previousTime - target.StartTime, 0, target.Length);
                var innerframedata = new FrameData()
                    { previousTime = localPreviousTime, currentTime = localCurrentTime, deltaTime = Mathf.Abs(localCurrentTime-localPreviousTime) };
                target.Update(farmedata,innerframedata);
            }
        }

        void IDirectableTimePointer.TriggerBackward(CFloat currentTime, CFloat previousTime)
        {
            if (currentTime < target.StartTime || currentTime <= 0)
            {
                if (triggered)
                {
                    triggered = false;
                    
                    var farmedata = new FrameData()
                    {
                        currentTime = currentTime,
                        previousTime = previousTime,
                        deltaTime = previousTime - currentTime
                    };
                    
                    var LocalTime = CMath.Clamp(previousTime - target.StartTime, 0, target.Length);
                    var innerframedata = new FrameData()
                        { previousTime = LocalTime, currentTime = 0, deltaTime = LocalTime };
                    target.Update(farmedata,innerframedata);
                    target.Reverse(farmedata,innerframedata);
                }
            }
        }
    }
    
    public struct EndTimePointer : IDirectableTimePointer
    {
        private bool triggered;
        public IDirectable target { get; private set; }
        float IDirectableTimePointer.time => target.EndTime;

        public EndTimePointer(IDirectable target)
        {
            this.target = target;
            triggered = false;
        }

        void IDirectableTimePointer.TriggerForward(CFloat currentTime, CFloat previousTime)
        {
            if (currentTime >= target.EndTime)
            {
                if (!triggered)
                {
                    triggered = true;
                    
                    var farmedata = new FrameData()
                    {
                        currentTime = currentTime,
                        previousTime = previousTime,
                        deltaTime = currentTime - previousTime
                    };
                    var localpreviousTime = CMath.Clamp(previousTime - target.StartTime, 0, target.Length);
                    var innerframedata = new FrameData()
                    {
                        currentTime = target.Length,
                        previousTime = localpreviousTime,
                        deltaTime = currentTime - previousTime,
                    };
                    target.Update(farmedata,innerframedata);
                    target.Exit(farmedata,innerframedata);
                }
            }
        }


        void IDirectableTimePointer.Update(float currentTime, float previousTime)
        {
            
        }


        void IDirectableTimePointer.TriggerBackward(float currentTime, float previousTime)
        {
            if (currentTime < target.EndTime || currentTime <= 0)
            {
                if (triggered)
                {
                    triggered = false;

                    var farmedata = new FrameData()
                    {
                        currentTime = currentTime,
                        previousTime = previousTime,
                        deltaTime = previousTime - currentTime
                    };
                    var localTime = Mathf.Clamp(currentTime - target.StartTime, 0, target.Length);
                    var innerframedata = new FrameData()
                    {
                        currentTime = localTime,
                        previousTime = target.Length,
                        deltaTime = previousTime - currentTime
                    };
                    target.ReverseEnter(farmedata,innerframedata);
                    target.Update(farmedata,innerframedata);
                }
            }
        }
    }
}