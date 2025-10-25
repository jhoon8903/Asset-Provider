using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using Projects.DataManagement.Data;
using Projects.Scripts.Effects.ProviderStrategies;
using UnityEngine;

namespace Projects.Scripts.Effects
{
    public partial class AssetProvider : MonoBehaviour
    {
        [Header("Default Settings")] 
        [SerializeField] private float defaultDeliveryDuration = 1.0f;
        [SerializeField] private bool autoApplyOnDelveryComplete = true;
        [SerializeField] private float forceApplyTimeout = 3f;

        private static AssetProvider _instance;
        private static readonly Dictionary<ReceiveHolderType, AssetReceiveHolder> ActiveHolders = new();
        private readonly Dictionary<string, UniTaskCompletionSource<bool>> _deliveryTasks = new();
        private static readonly Dictionary<ReceiveHolderType, IAssetProvideStrategy> ProvideStrategies = new();
        
        public static Action<UIUpdatePendingData> OnAssetDeliveryStarted;
        public static Action<ReceiveHolderType> OnAssetDeliveryCompleted;

        private static AssetProvider Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindAnyObjectByType<AssetProvider>();
                _instance?.InitializeStrategies();
                return _instance;
            }
        }
        
        [SerializeField] private SerializedDictionary<ReceiveHolderType, ParticleSystem> particleSystems = new();
        
        private void InitializeStrategies()
        {
            if (ProvideStrategies.Count > 0) return;
            ProvideStrategies[ReceiveHolderType.Heart] = new ProvideHeart();
            ProvideStrategies[ReceiveHolderType.UnlimitedHeart] = new ProvideUnlimitedHeart();
            ProvideStrategies[ReceiveHolderType.Coin] = new ProvideCurrency(ReceiveHolderType.Coin);
            ProvideStrategies[ReceiveHolderType.Ticket] = new ProvideCurrency(ReceiveHolderType.Ticket);
            ProvideStrategies[ReceiveHolderType.Star] = new ProvideCurrency(ReceiveHolderType.Star);
            ProvideStrategies[ReceiveHolderType.InGameStar] = new ProvideCurrency(ReceiveHolderType.InGameStar);
            ProvideStrategies[ReceiveHolderType.Item1] = new ProvideItem(ReceiveHolderType.Item1);
            ProvideStrategies[ReceiveHolderType.Item2] = new ProvideItem(ReceiveHolderType.Item2);
            ProvideStrategies[ReceiveHolderType.Item3] = new ProvideItem(ReceiveHolderType.Item3);
            ProvideStrategies[ReceiveHolderType.Item4] = new ProvideItem(ReceiveHolderType.Item4);
            ProvideStrategies[ReceiveHolderType.Boost1] = new ProvideItem(ReceiveHolderType.Boost1);
            ProvideStrategies[ReceiveHolderType.Boost2] = new ProvideItem(ReceiveHolderType.Boost2);
            ProvideStrategies[ReceiveHolderType.Boost3] = new ProvideItem(ReceiveHolderType.Boost3);
            ProvideStrategies[ReceiveHolderType.Ads] = new ProvideCurrency(ReceiveHolderType.Ads);
            ProvideStrategies[ReceiveHolderType.SeasonPassGoods] = new ProvideSeasonGoods();
            ProvideStrategies[ReceiveHolderType.SeasonPassWall] = new ProvideSeasonWall();
            ProvideStrategies[ReceiveHolderType.MetaGoods] = new ProviderMetaGoods();
        }

        #region Holder Registration
        
        private void OnDestroy()
        {
            ActiveHolders.Clear();
            _deliveryTasks.Clear();
            ProvideStrategies.Clear();
            _instance = null;
        }

        public static void RegisterHolder(AssetReceiveHolder holder)
        {
            if (holder == null) return;
            UnregisterHolder(holder);
            if (Instance == null) return;
            if (Instance.particleSystems == null) return;
            if (ActiveHolders == null) return;
            ActiveHolders[holder.ReceiveHolderType] = holder;
            if (Instance.particleSystems.TryGetValue(holder.ReceiveHolderType, out ParticleSystem particleSystem)) 
                holder.Attractor.AddParticleSystem(particleSystem);
        }

        public static void UnregisterHolder(AssetReceiveHolder holder)
        {
            if (holder == null) return;
            UnregisterHolder(holder.ReceiveHolderType);
        }

        public static void UnregisterHolder(ReceiveHolderType holderType)
        {
            if (Instance == null) return;
            if (Instance.particleSystems == null) return;
            if (ActiveHolders == null) return;
            if (!ActiveHolders.TryGetValue(holderType, out AssetReceiveHolder holder)) return;
            if (!Instance.particleSystems.TryGetValue(holderType, out ParticleSystem particleSystem)) return;
            holder.Attractor.RemoveParticleSystem(particleSystem);
            ActiveHolders.Remove(holderType);
        }

        #endregion

        #region Asset Delivery Methods

        public static bool AssetProviding(
            ReceiveHolderType receiveHolderType,
            Transform startUITransform,
            int amount = 0,
            float duration = 1f,
            bool useDelivery = true) =>
            StartAssetDelivery(receiveHolderType, amount, duration, useDelivery);
        
        public static bool StartAssetDelivery(
            ReceiveHolderType receiveHolderType,
            int amount,
            float duration = -1f,
            bool useDelivery = true)
        {
            AssetProvider instance = Instance;
            if (instance == null) return false;
            if (duration < 0) duration = instance.defaultDeliveryDuration;
            if (!ProvideStrategies.TryGetValue(receiveHolderType, out var strategy))
            {
                Debug.LogWarning($"[AssetProvider] No strategy for {receiveHolderType}");
                return false;
            }
            
            if (!strategy.ValidateAmount(amount))
            {
                Debug.LogWarning($"[AssetProvider] ValidateAmount failed for {receiveHolderType}, amount={amount}");
                return false;
            }
            if (!ActiveHolders.TryGetValue(receiveHolderType, out var targetHolder))
            {
                Debug.LogWarning($"[AssetProvider] No ActiveHolder for {receiveHolderType}");
                return false;
            }
            if (targetHolder == null || !targetHolder.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[AssetProvider] Holder inactive for {receiveHolderType}. Unregister and abort.");
                UnregisterHolder(targetHolder);
                return false;
            }
            int currentValue = strategy.GetCurrentValue(InGameDB.Data);
            int newValue = currentValue + amount;
            if (useDelivery)
            {
                bool dataSaved = strategy.ApplyValue(InGameDB.Data, newValue);
                if (!dataSaved) return false;
            }
            
            UIUpdatePendingData uiUpdateData = new UIUpdatePendingData
            {
                AssetType = receiveHolderType,
                OldDisplayValue = currentValue,
                NewDisplayValue = newValue,
                ChangeAmount = amount,
            };
            
            strategy.OnDeliveryStarted(amount);
            OnAssetDeliveryStarted?.Invoke(uiUpdateData);
            
            string taskKey = $"{receiveHolderType}_{Time.time}";
            UniTaskCompletionSource<bool> taskCompletion = new UniTaskCompletionSource<bool>();
            instance._deliveryTasks[taskKey] = taskCompletion;
            
            Vector2 startScreenPos = GetScreenPosition();
            DeliveryRequest deliveryRequest = new DeliveryRequest
            {
                TargetType = receiveHolderType,
                StartScreenPosition = startScreenPos,
                Amount = amount,
                DeliveryDuration = duration
            };

            bool deliverySuccess = targetHolder.ProcessDeliveryEffect(deliveryRequest);
            if (deliverySuccess) strategy.OnDeliveryCompleted(amount);
            else
            {
                if (useDelivery) strategy.ApplyValue(InGameDB.Data, currentValue);
            }
            
            OnAssetDeliveryCompleted?.Invoke(receiveHolderType);
            
            if (!instance._deliveryTasks.TryGetValue(taskKey, out var completion)) return deliverySuccess;
            completion.TrySetResult(deliverySuccess);
            instance._deliveryTasks.Remove(taskKey);
            return deliverySuccess;
        }
        
        public static void ForceUpdateUI(ReceiveHolderType type)
        {
            var data = new UIUpdatePendingData();
            data.AssetType = type;
            OnAssetDeliveryStarted?.Invoke(data);
            OnAssetDeliveryCompleted?.Invoke(type);
        }

        #endregion

        #region Dynamic Particle Sprite (Meta Goods)
        
        private static void UpdateParticleSprite(ReceiveHolderType holderType, Sprite sprite)
        {
            if (Instance == null) return;
            if (!Instance.particleSystems.TryGetValue(holderType, out ParticleSystem ps)) return;
            if (ps == null || sprite == null) return;
            ParticleSystem.TextureSheetAnimationModule textureSheet = ps.textureSheetAnimation;
            if (!textureSheet.enabled) return;
            if (textureSheet.mode != ParticleSystemAnimationMode.Sprites) return;
            textureSheet.SetSprite(0,null);
            textureSheet.SetSprite(0, sprite);
        }
        
        public static void DeliverToMetaGoods(Transform targetTransform, Sprite goodsSprite)
        {
            if (Instance == null || targetTransform == null) return;
            if (goodsSprite != null) UpdateParticleSprite(ReceiveHolderType.MetaGoods, goodsSprite);
            if (!Instance.particleSystems.TryGetValue(ReceiveHolderType.MetaGoods, out ParticleSystem ps)) return;
            UIParticleAttractor attractor = targetTransform.GetComponentInChildren<UIParticleAttractor>();
            if (attractor == null) return;
            
            attractor.AddParticleSystem(ps);
            
            var parentRect = ps.transform.parent as RectTransform;
            if (parentRect == null)
            {
                var canvas = ps.GetComponentInParent<Canvas>();
                if (canvas == null) return;
                Vector3 worldPos = ScreenToWorldPointOnCanvas(canvas, GetPointerScreenPos());
                ps.transform.position = worldPos;
            }
            else
            {
                Vector2 localPos = ScreenToLocalIn(parentRect, GetPointerScreenPos());
                var psRect = ps.transform as RectTransform;
                if (psRect != null) psRect.anchoredPosition = localPos;
                else ps.transform.localPosition = localPos;
            }
            
            ps.Emit(1);
        }

        #endregion
    }
}