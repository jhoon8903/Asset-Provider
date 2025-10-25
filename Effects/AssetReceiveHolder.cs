using System;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;

namespace Projects.Scripts.Effects
{
    [RequireComponent(typeof(UIParticleAttractor))]
    public class AssetReceiveHolder : MonoBehaviour
    {
        [Header("Holder Asset Type")]
        [SerializeField] private ReceiveHolderType receiveHolderType = ReceiveHolderType.None;
        public ReceiveHolderType ReceiveHolderType => receiveHolderType;
        
        [Header("Particle Settings")]
        [SerializeField] private int particlesPerUnit = 5;
        
        [Header("FeedBack Tween")]
        [SerializeField] private float feedBackDuration = 0.2f;
        [SerializeField] private float feedBackScale = 1.3f;
        [SerializeField] private Ease feedBackEase = Ease.OutBounce;
        
        public event Action OnAssetArrived;
        private Tweener _feedBackTweener;
        public UIParticleAttractor Attractor { get; private set; }
        private Canvas _parentCanvas;

        private void Awake()
        {
            _parentCanvas = GetComponentInParent<Canvas>();
            Attractor = GetComponent<UIParticleAttractor>();
            Attractor ??= gameObject.AddComponent<UIParticleAttractor>();
            Attractor.onAttracted.AddListener(OnParticleAttracted);
        }

        private void OnEnable() => AssetProvider.RegisterHolder(this);

        private void OnDisable()
        {
            _feedBackTweener?.Kill();
            transform.localScale = Vector3.one;
            AssetProvider.UnregisterHolder(this);
        }

        public bool ProcessDeliveryEffect(DeliveryRequest deliveryRequest)
        {
            if (_parentCanvas == null) return false;

            try
            {
                int totalParticles = Mathf.Max(1, deliveryRequest.Amount / particlesPerUnit);
                CreateDeliveryParticle(deliveryRequest.StartScreenPosition, totalParticles);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void CreateDeliveryParticle(Vector2 startLocalPos, int emitCount)
        {
            ParticleSystem particleInstance = Attractor.particleSystems[0];
            if (particleInstance == null) return;
            if (particleInstance.isPlaying)
            {
                particleInstance.Stop(true);
                particleInstance.Clear();
            }
            particleInstance.transform.localPosition = startLocalPos;
            particleInstance.Emit(emitCount);
        }
        
        private void OnParticleAttracted()
        {
            FeedBackAction();
            OnAssetArrived?.Invoke();
        }

        private void FeedBackAction()
        {
            transform.localScale = Vector3.one;
            _feedBackTweener?.Kill(true);
            _feedBackTweener = transform
                .DOScale(feedBackScale, feedBackDuration)
                .SetEase(feedBackEase)
                .OnComplete(()=>transform.localScale = Vector3.one);
        }
    }
}