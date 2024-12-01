using UnityEngine;

namespace MMDarkness.Editor
{
    [CustomPreview(typeof(VFXClipProcessor))]
    public class VFXPreview : PreviewLogic
    {
        private VFXClipProcessor _processor;
        private ParticleSystem vfx;
/// <summary>
/// 重写编辑器Preview逻辑，编辑器运行时分开两套逻辑
/// </summary>
        public override void Init()
        {
            base.Init();
            _processor = Processor as VFXClipProcessor;
            vfx = _processor.vfx;
            vfx.randomSeed = 1;
            foreach (var particle in vfx.GetComponentsInChildren<ParticleSystem>())
            {
                particle.randomSeed = 1;
            }
        }

        public override void Enter(FrameData frameData, FrameData innerFrameData)
        {
            if(!_processor.vfx)
                return;
            vfx.gameObject.SetActive(true);
        }
        public override void ReverseEnter(FrameData frameData, FrameData innerFrameData)
        {
            if(!_processor.vfx)
                return;
            vfx.gameObject.SetActive(true);
        }
        

        public override void Exit(FrameData frameData, FrameData innerFrameData)
        {
            if(!_processor.vfx)
                return;
            vfx.gameObject.SetActive(false);
        }

        public override void Reverse(FrameData frameData, FrameData innerFrameData)
        {
            if(!_processor.vfx)
                return;
            vfx.gameObject.SetActive(false);
        }
        
        public override void OnEditorStop(FrameData frameData, FrameData innerFrameData)
        {
            if(vfx)
                Object.DestroyImmediate(vfx.gameObject);
        }
    }
}