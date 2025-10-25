using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Projects.DataManagement.Data;
using Projects.DataManagement.Data.Cloud.SaveSystem.Core;
using Projects.DataManagement.Structure;
using UnityEngine;

namespace Projects.Scripts.Effects.ProviderStrategies
{
    public class ProvideSeasonGoods : BaseAssetProvideStrategy
    {
        /// <summary>
        /// 굿즈 타입 (기존 시스템과의 호환성을 위한 임시 구조체)
        /// </summary>
        public struct GoodsType
        {
            public int Type;
            public string Name;
            public int Amount;
        }
        
        public override ReceiveHolderType AssetType => ReceiveHolderType.SeasonPassGoods;

        public override int GetCurrentValue(PlayerData data)
        {
            // 시즌패스 데이터에서 굿즈 포인트 조회
            // data.SeasonPass?.Goods;
            return 0;
        }

        public override bool ApplyValue(PlayerData data, int newValue)
        {
            if (data.SeasonPass == null) return false;
            
            // 시즌 종료 체크
            if (IsSeasonExpired())
            {
                Debug.LogWarning("Season has expired, cannot add goods");
                return false;
            }
            
            //data.SeasonPass.Goods = newValue;
            return true;
        }

        public override bool ValidateAmount(int amount)
        {
            return amount is > 0 and <= 1000 && !IsSeasonExpired();
        }

        public override void OnDeliveryStarted(int amount)
        {
            base.OnDeliveryStarted(amount);
            Debug.Log($"Starting season goods delivery with special effects: {amount}");
        }

        public override async void OnDeliveryCompleted(int amount)
        {
            base.OnDeliveryCompleted(amount);
            await PlaySeasonGoodsEffect(amount);
        }

        public override void OnAssetApplied(int finalValue)
        {
            base.OnAssetApplied(finalValue);
            CheckSeasonPassLevelUp(finalValue);
        }

        private async UniTask PlaySeasonGoodsEffect(int amount)
        {
            try
            {
                // 기존 ReceiveSeasonGoodsEffectiveInternal 로직 적용
                var endPosition = new Vector3(0, -3.8f, 0f);
                var endScale = new Vector3(0.1f, 0.1f, 0.1f);
                const int processDelaySeconds = 500; // 밀리초 단위로 변환

                // 굿즈 타입 가져오기 (임시로 기본값 사용, 실제로는 파라미터나 설정에서)
                var goodsTypes = GetGoodsTypesForAmount(amount);

                for (int idx = goodsTypes.Count - 1; idx >= 0; --idx)
                {
                    bool isPlayOver = false;
                    GameObject seasonGoods = await CreateSeasonGoodsObject(goodsTypes[idx]);
                    if (seasonGoods == null) continue;
                    PlayGoodsParticle(seasonGoods, () => isPlayOver = true);
                    await UniTask.WaitUntil(() => isPlayOver);
                    await PlayActionEffectTween(seasonGoods.transform, endPosition, endScale);
                    await UniTask.Delay(processDelaySeconds);
                }
                
                OpenMask(false);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Season goods effect error: {ex.Message}");
            }
        }

        private List<GoodsType> GetGoodsTypesForAmount(int amount)
        {
            // 실제 구현에서는 MetaConvenience.GetConvertingMetaKey 등을 사용
            // 임시로 기본 굿즈 타입 반환
            var goodsTypes = new List<GoodsType>();
            for (int i = 0; i < amount; i++)
            {
                goodsTypes.Add(new GoodsType { Type = i % 5 }); // 임시 데이터
            }
            return goodsTypes;
        }

        private async UniTask<GameObject> CreateSeasonGoodsObject(GoodsType goodsType)
        {
            // ResourceManager를 통한 오브젝트 생성
            // 실제 구현에서는 PassGoods 프리팹 로드
            try
            {
                // var seasonGoods = await ResourceManager.InstantiateAsync<PassGoods>(seasonPassGoodsPrefab);
                // seasonGoods.SetSpine(goodsType);
                // return seasonGoods.gameObject;
                
                // 임시 구현
                var tempObj = new GameObject("SeasonGoods");
                return tempObj;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create season goods object: {ex.Message}");
                return null;
            }
        }

        private void PlayGoodsParticle(GameObject seasonGoods, Action onComplete)
        {
            // PassGoods의 PlayParticle 메서드 호출
            // seasonGoods.GetComponent<PassGoods>()?.PlayParticle(onComplete);
            
            // 임시 구현
            DOVirtual.DelayedCall(1f, ()=> onComplete?.Invoke());
        }

        private async UniTask PlayActionEffectTween(Transform target, Vector3 endPosition, Vector3 endScale)
        {
            // 기존 ActionEffectiveTween 로직
            var sequence = DOTween.Sequence();
            
            sequence.Append(target.DOMove(endPosition, 0.5f).SetEase(Ease.InQuad));
            sequence.Join(target.DOScale(endScale, 0.5f).SetEase(Ease.InQuad));
            sequence.OnComplete(() =>
            {
                // 사운드 재생
                PlayMetaCompleteSound();
                
                // 오브젝트 제거
                if (target != null)
                {
                    UnityEngine.Object.Destroy(target.gameObject);
                }
            });

            await sequence.AsyncWaitForCompletion();
        }

        private void PlayMetaCompleteSound()
        {
            // Main.Sound.PlaySfx(SoundClip.MetaComplete);
            Debug.Log("Playing meta complete sound");
        }

        private void OpenMask(bool isOpen)
        {
            // Mask.OpenMaskAction(isOpen);
            Debug.Log($"Mask action: {isOpen}");
        }

        private bool IsSeasonExpired()
        {
            // 실제 시즌 종료일 체크 로직
            return false; // 임시
        }

        private void CheckSeasonPassLevelUp(int totalGoods)
        {
            // 시즌패스 레벨업 계산
            Debug.Log($"Checking season pass level up for goods: {totalGoods}");
        }
    }
}