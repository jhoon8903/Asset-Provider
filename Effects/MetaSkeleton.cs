using Spine.Unity;
using UnityEngine;

namespace Projects.Scripts.Effects
{
    public class MetaSkeleton : MonoBehaviour
    {
        private SkeletonGraphic _spineGraphic;

        private void Awake()
        {
            _spineGraphic = GetComponentInChildren<SkeletonGraphic>();
          
        }

        private void Start()
        {
            PlayIdle2Animation();
        }

        private void PlayIdle2Animation()
        {
            _spineGraphic.AnimationState.SetAnimation(0, "idle2", true);
            Debug.Log("idle2");
        }
    
    }
}