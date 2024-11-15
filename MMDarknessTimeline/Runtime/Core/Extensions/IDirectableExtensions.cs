using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MMDarkness
{
    public static class DirectableExtensions
    {
        public static float GetLength(this DirectableAsset directable)
        {
            return directable.EndTime - directable.StartTime;
        }

        public static float ToLocalTime(this DirectableAsset directable, float time)
        {
            return Mathf.Clamp(time - directable.StartTime, 0, directable.GetLength());
        }

        public static float ToLocalTimeUnClamped(this DirectableAsset directable, float time)
        {
            return time - directable.StartTime;
        }

        public static bool CanCrossBlend(this ClipAsset directable, ClipAsset other)
        {
            if (directable == null || other == null)
            {
                return false;
            }

            if ((directable.CanCrossBlend || other.CanCrossBlend) && directable.GetType() == other.GetType())
            {
                return true;
            }

            return false;
        }

        public static bool CanBlendIn(this ClipAsset directable)
        {
            var blendInProp = directable.GetType().GetProperty("BlendIn", BindingFlags.Instance | BindingFlags.Public);
            return blendInProp != null && blendInProp.CanWrite && Math.Abs(directable.BlendIn - (-1)) > 0.0001f && blendInProp.DeclaringType != typeof(DirectableAsset);
        }

        public static bool CanBlendOut(this ClipAsset directable)
        {
            var blendOutProp = directable.GetType().GetProperty("BlendOut", BindingFlags.Instance | BindingFlags.Public);
            return blendOutProp != null && blendOutProp.CanWrite && Math.Abs(directable.BlendOut - (-1)) > 0.0001f && blendOutProp.DeclaringType != typeof(DirectableAsset);
        }

        public static bool CanScale(this ClipAsset directable)
        {
#if false
            var lengthProp = directable.GetType().GetProperty("Length", BindingFlags.Instance | BindingFlags.Public);
            return lengthProp != null && lengthProp.CanWrite && lengthProp.DeclaringType != typeof(ClipAsset);
#endif
            return true;
        }

        public static ClipAsset GetPreviousSibling(this ClipAsset directable)
        {
            if (directable.Parent is TrackAsset trackAsset)
            {
                return trackAsset.Clips.LastOrDefault(d => d != directable && d.StartTime < directable.StartTime);
            }

            return null;
        }

        /// <summary>
        /// 返回父对象的下个同级
        /// </summary>
        /// <param name="directable"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetNextSibling<T>(this ClipAsset directable) where T : ClipAsset
        {
            return (T)GetNextSibling(directable);
        }

        public static ClipAsset GetNextSibling(this ClipAsset directable)
        {
            if (directable.Parent is TrackAsset trackAsset)
            {
                return trackAsset.Clips.FirstOrDefault(d => d != directable && d.StartTime > directable.StartTime);
            }

            return null;
        }


        ///The weight at specified local time based on its blend properties.
        public static float GetWeight(this ClipAsset directable, float time)
        {
            return GetWeight(directable, time, directable.BlendIn, directable.BlendOut);
        }

        ///The weight at specified local time based on provided override blend in/out properties
        public static float GetWeight(this ClipAsset directable, float time, float blendInOut)
        {
            return GetWeight(directable, time, blendInOut, blendInOut);
        }

        public static float GetWeight(this ClipAsset directable, float time, float blendIn, float blendOut)
        {
            var length = GetLength(directable);
            if (time <= 0)
            {
                return blendIn <= 0 ? 1 : 0;
            }

            if (time >= length)
            {
                return blendOut <= 0 ? 1 : 0;
            }

            if (time < blendIn)
            {
                return time / blendIn;
            }

            if (time > length - blendOut)
            {
                return (length - time) / blendOut;
            }

            return 1;
        }

        /// <summary>
        /// 返回剪辑的上一个循环长度
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static float GetPreviousLoopLocalTime(this ISubClipContainable clip)
        {
            float clipLength = 0;
            if (clip is DirectableAsset directableAsset)
            {
                clipLength = directableAsset.GetLength();
            }

            var loopLength = clip.SubClipLength / clip.SubClipSpeed;
            if (clipLength > loopLength)
            {
                var mod = (clipLength - clip.SubClipOffset) % loopLength;
                var aproxZero = Mathf.Abs(mod) < 0.01f;
                return clipLength - (aproxZero ? loopLength : mod);
            }

            return clipLength;
        }


        /// <summary>
        /// 返回剪辑的下一个循环长度
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public static float GetNextLoopLocalTime(this ISubClipContainable clip)
        {
            float clipLength = 0;
            if (clip is DirectableAsset directableAsset)
            {
                clipLength = directableAsset.GetLength();
            }

            var loopLength = clip.SubClipLength / clip.SubClipSpeed;
            var mod = (clipLength - clip.SubClipOffset) % loopLength;
            var aproxZero = Mathf.Abs(mod) < 0.01f || Mathf.Abs(loopLength - mod) < 0.01f;
            return clipLength + (aproxZero ? loopLength : (loopLength - mod));
        }
    }
}
