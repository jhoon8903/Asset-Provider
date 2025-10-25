using Projects.Scripts.Effects;
using UnityEngine;

namespace Projects.Scripts.Elements
{
    public abstract class PropsComponents : MonoBehaviour
    {
        private AssetReceiveHolder _assetReceiveHolder;

        protected virtual void Awake()
        {
            _assetReceiveHolder = GetComponentInChildren<AssetReceiveHolder>();
        }

        protected virtual void OnEnable()
        {
            if (_assetReceiveHolder) _assetReceiveHolder.OnAssetArrived += AssetParticleArrived;
            AssetProvider.OnAssetDeliveryCompleted += DeliveryCompleted;
            AssetProvider.OnAssetDeliveryStarted += DeliveryStart;
        }

        protected virtual void OnDisable()
        {
            if (_assetReceiveHolder) _assetReceiveHolder.OnAssetArrived -= AssetParticleArrived;
            AssetProvider.OnAssetDeliveryStarted -= DeliveryStart;
            AssetProvider.OnAssetDeliveryCompleted -= DeliveryCompleted;
        }
        
        /// <summary>
        /// 데이터 저장 후 실행
        /// </summary>
        /// <param name="updateData">에셋 프로바이딩 데이터</param>>
        /// <param name="updateData.AssetType">에셋 타입</param>
        /// <param name="updateData.OldDisplayValue">업데이트 전 Value</param>
        /// <param name="updateData.NewDisplayValue">업데이트 후 Value</param>
        /// <param name="updateData.ChangeAmount">업데이트 되는 사잇 값</param>
        protected abstract void DeliveryStart(UIUpdatePendingData updateData);
        
        /// <summary>
        /// Particle Emit 시점
        /// </summary>
        /// <param name="receiveItemType"></param>
        protected abstract void DeliveryCompleted(ReceiveHolderType receiveItemType);
        
        /// <summary>
        /// 파티클 도착시 마다 호출
        /// Emit 된 파티클이 5개 라면 총 5회 호출 됨
        /// </summary>
        protected abstract void AssetParticleArrived();
    }
}