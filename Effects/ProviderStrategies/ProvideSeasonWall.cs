using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Projects.DataManagement.Data;
using Projects.DataManagement.Data.Cloud.SaveSystem.Core;
using Projects.DataManagement.Structure;
using Projects.Scripts.Core.Resource;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Projects.Scripts.Effects.ProviderStrategies
{
    public class ProvideSeasonWall : BaseAssetProvideStrategy
    {
        public override ReceiveHolderType AssetType => ReceiveHolderType.SeasonPassWall;

        public override int GetCurrentValue(PlayerData data)
        {
            return 0;
                //data.SeasonPass?.Wall ?? 0;
        }

        public override bool ApplyValue(PlayerData data, int newValue)
        {
            if (data.SeasonPass == null) return false;
            const int maxWalls = 1;
            //data.SeasonPass.Wall = Mathf.Min(newValue, maxWalls);
            return true;
        }

        public override bool ValidateAmount(int amount)
        {
            return amount is > 0 and <= 1;
        }

        public override void OnDeliveryStarted(int amount)
        {
            base.OnDeliveryStarted(amount);
            Debug.Log($"Starting season wall delivery with special effects: {amount}");
        }

        public override async void OnDeliveryCompleted(int amount)
        {
            base.OnDeliveryCompleted(1);
            await PlaySeasonWallEffect();
        }

        public override void OnAssetApplied(int finalValue)
        {
            base.OnAssetApplied(finalValue);
        }

        private async UniTask PlaySeasonWallEffect()
        {
            try
            {
                // 기존 ReceiveSeasonWallAndEffective 로직 적용
                var endPosition = new Vector3(0, -3.78f, 0f);
                var maxScale = new Vector3(0.35f, 0.35f, 0.35f);
                var endScale = new Vector3(0.02f, 0.02f, 0.02f);

                // 월 오브젝트 생성
                var wallObj = await CreateSeasonWallObject();
                if (wallObj == null) return;

                // DOTween 시퀀스 생성
                var sequence = DOTween.Sequence();

                // 확대 애니메이션 (OutBack 이징)
                sequence.Append(wallObj.transform.DOScale(maxScale, 1f).SetEase(Ease.OutBack).SetUpdate(true));
                
                // 축소 애니메이션 (InQuad 이징)
                sequence.Append(wallObj.transform.DOScale(endScale, 0.5f).SetEase(Ease.InQuad).SetUpdate(true));
                
                // 이동 애니메이션 (InQuad 이징)
                sequence.Join(wallObj.transform.DOMove(endPosition, 0.5f).SetEase(Ease.InQuad).SetUpdate(true));
                
                // 완료 시 처리
                sequence.OnComplete(() =>
                {
                    // 메타 완료 사운드 재생
                    PlayMetaCompleteSound();
                    
                    // 마스크 해제
                    OpenMask(false);
                    
                    // 오브젝트 제거
                    if (wallObj != null)
                    {
                        UnityEngine.Object.Destroy(wallObj);
                    }
                }).SetUpdate(true);

                // 애니메이션 완료 대기
                await sequence.AsyncWaitForCompletion();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Season wall effect error: {ex.Message}");
            }
        }

        private async UniTask<GameObject> CreateSeasonWallObject()
        {
            // string caseName = NowPassProduct switch
            // {
            //     IAP_Product_ID.season_2024_06 => "Loca_Complete",
            //     IAP_Product_ID.season_2024_12 => "Winter2024",
            //     IAP_Product_ID.season_2025_03 => "Spring2025",
            //     IAP_Product_ID.season_2025_06 => "Summer2025",
            //     IAP_Product_ID.season_2025_09 => "Fall2025",
            //     IAP_Product_ID.season_2025_12 => "Winter2025",
            //     _ => "Winter2024"
            // };
            AssetReference wallReference = null;
            throw new NotImplementedException("Season Pass Wall Not Implemented");
            try
            {
                GameObject wallObj = await ResourceManager.InstantiateAsync(wallReference);
                return wallObj;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create season wall object: {ex.Message}");
                return null;
            }
        }

        private void PlayMetaCompleteSound()
        {
            // Main.Sound.PlaySfx(SoundClip.MetaComplete);
            Debug.Log("Playing meta complete sound for wall");
        }

        private void OpenMask(bool isOpen)
        {
            // Mask.OpenMaskAction(isOpen);
            Debug.Log($"Mask action for wall: {isOpen}");
        }
    }
}