using UnityEngine;

namespace MMDarkness
{
    public abstract class DirectableAsset : ScriptableObject, IData
    {
        [SerializeField] [HideInInspector] internal DirectableAsset m_parent;

        public abstract TimelineGraphAsset Root { get; set; }
        public abstract DirectableAsset Parent { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual bool IsLocked { get; set; }
        public virtual bool IsCollapsed { get; set; }
        public virtual float StartTime { get; set; }
        public virtual float EndTime { get; set; }
        public virtual float BlendIn { get; set; }
        public virtual float BlendOut { get; set; }
        public virtual bool CanCrossBlend { get; set; }
    }
}