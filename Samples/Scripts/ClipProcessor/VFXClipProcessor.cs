using UnityEditor;
using UnityEngine;

namespace MMDarkness
{
    [ViewModel(typeof(VFXClip))]
    public class VFXClipProcessor : ClipProcessor<VFXClip>
    {
        public ParticleSystem vfx;
        protected override void OnInit()
        {
            //这里简单示例，就直接实例化了，到具体项目用项目内的对象池做
            var vfxresource = AssetDatabase.LoadAssetAtPath<GameObject>(TData.VFXPath);
            var vfxobj = Object.Instantiate(vfxresource, Owner.transform);
            vfxobj.transform.localPosition = TData.PositionOffset;
            vfxobj.transform.localRotation = Quaternion.Euler(TData.RotationOffset);
            if (vfxobj.TryGetComponent(out ParticleSystem p))
            {
                vfx = p;
            }

            if (vfx)
            {
                vfx.Stop();
                vfx.gameObject.SetActive(false);
            }
        }

        protected override void OnEnter(FrameData frameData, FrameData innerFrameData)
        {
            if(!vfx)
                return;
            vfx.gameObject.SetActive(true);
            vfx.Play();
        }

        protected override void OnUpdate(FrameData frameData, FrameData innerFrameData)
        {
            if(!vfx)
                return;
            vfx.Simulate(innerFrameData.currentTime);
        }

        protected override void OnExit(FrameData frameData, FrameData innerFrameData)
        {
            if(!vfx)
                return;
            Object.Destroy(vfx.gameObject);
        }
    }
}