using _Plugins__Runtime.UIModule.Animation;
using _Plugins__Runtime.UIModule.UINavigation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JSAM;
using Projects.DataManagement.Data;
using Projects.Scripts.Effects;
using Projects.Scripts.Scene.Lobby.Button;
using Projects.Scripts.View.Store;
using TMPro;
using UnityEngine;

namespace Projects.Scripts.Elements
{
    public class CoinDisplay : PropsComponents
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text coinValueText;
        
        [Header("Animation Settings")]
        [SerializeField] private float countUpDuration = 0.8f;
        [SerializeField] private bool useScaleEffect = true;
        [SerializeField] private bool usePunchEffect;
        
        private int _lastDisplayedValue = -1;
        private int _pendingValue = -1;
        private Sequence _currentCountUpTweener;
        private bool _isAnimating;
        private NumberCountUpAnimation _animation;

        protected override void Awake()
        {
            base.Awake();
            _animation = new NumberCountUpAnimation();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _lastDisplayedValue = InGameDB.Data.Mileage;
            UpdateCoinDisplayInstant();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (coinValueText != null)
            {
                coinValueText.transform.DOKill();
                coinValueText.transform.localScale = Vector3.one;
            }
            NumberCountUpAnimation.SafeKill(_currentCountUpTweener, coinValueText?.transform);
        }
        
        private void UpdateCoinDisplayInstant()
        {
            if (coinValueText == null) return;
            coinValueText.text = _animation.FormatCurrency(InGameDB.Data.Mileage);
            _lastDisplayedValue = InGameDB.Data.Mileage;
        }
        
        private void StartCountUpAnimation(int fromValue, int toValue)
        {
            if (coinValueText == null) return;
            _isAnimating = true;
            NumberCountUpAnimation.SafeKill(_currentCountUpTweener, coinValueText.transform);
            
            // ★ 증가할 때만 사운드 재생 (차감 시에는 재생 안함)
            if (toValue > fromValue) AudioManager.PlaySound(LibSounds.PickUp);
            
            if (useScaleEffect)
            {
                Sequence sequence = _animation.CountUpWithScale(
                    coinValueText,
                    fromValue,
                    toValue,
                    countUpDuration,
                    1.15f,
                    _animation.FormatCurrency,
                    OnCountUpComplete
                );
                _currentCountUpTweener = sequence;
            }
            else if (usePunchEffect)
            {
                Sequence sequence = _animation.CountUpWithPunch(
                    coinValueText,
                    fromValue,
                    toValue,
                    countUpDuration,
                    _animation.FormatCurrency,
                    OnCountUpComplete
                );
                _currentCountUpTweener = sequence;
            }
            
            _lastDisplayedValue = toValue;
        }
        
        private void OnCountUpComplete()
        {
            _isAnimating = false;
            if (_pendingValue != -1 && _pendingValue != _lastDisplayedValue)
            {
                int nextValue = _pendingValue;
                _pendingValue = -1;
                StartCountUpAnimation(_lastDisplayedValue, nextValue);
                return;
            }
            int currentMileage = InGameDB.Data.Mileage;
            if (currentMileage != _lastDisplayedValue)  StartCountUpAnimation(_lastDisplayedValue, currentMileage);
        }
        
        // ★ 수정: Meta 구매(차감) 시에도 호출됨
        protected override void DeliveryStart(UIUpdatePendingData updateData)
        {
            if (updateData.AssetType != ReceiveHolderType.Coin) return;
            
            // ★ 데이터가 이미 변경된 상태이므로 즉시 업데이트
            UpdateCoinValue();
        }

        protected override void DeliveryCompleted(ReceiveHolderType receiveItemType)
        {
            if (receiveItemType != ReceiveHolderType.Coin)  UpdateCoinValue();
        }

        protected override void AssetParticleArrived()
        {
            if (coinValueText == null) return;
            UpdateCoinValue();
        }
        
        public void UpdateCoinValue()
        {
            int targetValue = InGameDB.Data.Mileage;
            if (_isAnimating) _pendingValue = targetValue;
            else if (targetValue != _lastDisplayedValue) StartCountUpAnimation(_lastDisplayedValue, targetValue);
        }

        public void PurchaseCoin() => Purchase().Forget();

        private async UniTask Purchase()
        {
            LobbyBtnHelper.SelectBtnSetting(ButtonType.Store);
            StoreView view = await UINavigation.Instance.GetViewWithoutShow<StoreView>();
            if (UINavigation.Instance.GetTopView().isActiveAndEnabled != view) await view.ShowAsync();
            await view.ScrollToPoint(StoreOpenPoint.Mileage);
        }
    }
}