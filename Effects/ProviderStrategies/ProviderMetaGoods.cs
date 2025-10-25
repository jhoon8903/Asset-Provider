using Projects.DataManagement.Structure;
using UnityEngine;

namespace Projects.Scripts.Effects.ProviderStrategies
{
    /// <summary>
    /// 메타 굿즈 진행도 제공 전략
    /// 실제 데이터는 MetaDataManager에서 관리하므로 이 클래스는 검증과 이벤트만 처리
    /// </summary>
    public class ProviderMetaGoods : IAssetProvideStrategy
    {
        public ReceiveHolderType AssetType => ReceiveHolderType.MetaGoods;

        /// <summary>
        /// PlayerData에서는 값을 가져오지 않음 (MetaDataManager에서 관리)
        /// AssetProvider 구조상 필요하므로 더미 값 반환
        /// </summary>
        public int GetCurrentValue(PlayerData data)
        {
            return 0;
        }

        /// <summary>
        /// PlayerData에 직접 적용하지 않음
        /// 실제 진행도 적용은 MetaPurchaseController에서 MetaDataManager를 통해 처리
        /// </summary>
        public bool ApplyValue(PlayerData data, int newValue)
        {
            // MetaPurchaseController에서 이미 처리했으므로 성공으로 간주
            return true;
        }

        /// <summary>
        /// 파티클로 전송될 코인 금액 검증
        /// </summary>
        public bool ValidateAmount(int amount)
        {
            return amount > 0;
        }

        /// <summary>
        /// 파티클 전송 시작 시 호출
        /// </summary>
        public void OnDeliveryStarted(int amount)
        {
            Debug.Log($"[ProviderMetaGoods] 파티클 전송 시작: {amount} 코인");
        }

        /// <summary>
        /// 파티클이 굿즈에 도착했을 때 호출
        /// </summary>
        public void OnDeliveryCompleted(int amount)
        {
            Debug.Log($"[ProviderMetaGoods] 파티클 전송 완료: {amount} 코인");
        }

        /// <summary>
        /// 진행도가 UI에 적용되었을 때 호출
        /// </summary>
        public void OnAssetApplied(int finalValue)
        {
            Debug.Log($"[ProviderMetaGoods] UI 적용 완료: {finalValue}");
        }
    }
}