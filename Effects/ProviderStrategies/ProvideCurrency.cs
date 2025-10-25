using Projects.DataManagement.Structure;
using Projects.Scripts.Core.LevelOP;
using Projects.Scripts.Scene.InGame;
using UnityEngine;

namespace Projects.Scripts.Effects.ProviderStrategies
{
    public class ProvideCurrency : BaseAssetProvideStrategy
    {
        private readonly ReceiveHolderType _currencyType;
        private readonly int _maxAmount;
        public override ReceiveHolderType AssetType => _currencyType;

        public ProvideCurrency(ReceiveHolderType currencyType, int maxAmount = int.MaxValue)
        {
            _currencyType = currencyType;
            _maxAmount = maxAmount;
        }
       
        public override int GetCurrentValue(PlayerData data)
        {
            switch (_currencyType)
            {
                case ReceiveHolderType.Coin:
                    return data.Mileage;
                case ReceiveHolderType.Ticket:
                    return data.Ticket;
                case ReceiveHolderType.Star:
                    return data.Star;
                case ReceiveHolderType.InGameStar:
                    return GameScene.GamePlayData.totalStarCount;
                default:
                    return 0;
            }
        }

        public override bool ApplyValue(PlayerData data, int newValue)
        {
            newValue = Mathf.Min(newValue, _maxAmount);
            
            switch (_currencyType)
            {
                case ReceiveHolderType.Coin:
                    data.Mileage = newValue;
                    break;
                case ReceiveHolderType.Ticket:
                    data.Ticket = newValue;
                    break;
                case ReceiveHolderType.Star:
                    data.Star = newValue;
                    break;
                case ReceiveHolderType.InGameStar:
                    Debug.Log($"New VALUE: {newValue}");
                    GameScene.GamePlayData.totalStarCount = newValue;
                    Debug.Log($"UPDATE IN GAME STAR COUNT: {GameScene.GamePlayData.totalStarCount}");
                    break;
                default:
                    return false;
            }
            return true;
        }
         
        public override bool ValidateAmount(int amount) => amount > 0 && amount <= _maxAmount;

        public override void OnAssetApplied(int finalValue)
        {
            base.OnAssetApplied(finalValue);
            
            // 일일 퀘스트?
            switch (_currencyType)
            {
                case ReceiveHolderType.Coin:
                    CheckCoinAchievements(finalValue);
                    break;
                case ReceiveHolderType.Star:
                    CheckStarMilestones(finalValue);
                    break;
            }
        }

        private void CheckCoinAchievements(int totalCoins)
        {
            // 코인 업적 체크
            Debug.Log($"Checking coin achievements for: {totalCoins}");
        }

        private void CheckStarMilestones(int totalStars)
        {
            // 별 마일스톤 체크
            Debug.Log($"Checking star milestones for: {totalStars}");
        }
    }
}