using System;
using Unity.VisualScripting;
using UnityEngine;

namespace MMDarkness.Editor
{
    /// <summary>
    /// 重写预览类，预览类默认使用Processor原来的方法，如果需要编辑器预览与运行时不同逻辑
    /// 则继承预览类，并且在类上方使用[CustomPreview(typeof(重写Processor的类))]，
    /// 并且重写生命周期，然后去掉base.生命周期
    /// </summary>
    public class PreviewLogic
    {
        public DirectableAsset Directable;
        public IDirectable Processor;

        public void SetTarget(IDirectable t,DirectableAsset directable)
        {
            Directable = directable;
            Processor = t;
        }

        public virtual void Enter(FrameData frameData,FrameData innerFrameData)
        {
            Processor.Enter(frameData, innerFrameData);
        }

        public virtual void Exit(FrameData frameData,FrameData innerFrameData)
        {
            Processor.Exit(frameData, innerFrameData);
        }

        public virtual void ReverseEnter(FrameData frameData,FrameData innerFrameData)
        {
            Processor.ReverseEnter(frameData, innerFrameData);
        }

        public virtual void Reverse(FrameData frameData,FrameData innerFrameData)
        {
            Processor.Reverse(frameData, innerFrameData);
        }


        public virtual void Update(FrameData frameData, FrameData innerFrameData)
        {
            Processor.Update(frameData, innerFrameData);
        }
    }
}
