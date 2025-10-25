using Projects.DataManagement.Data;
using Projects.DataManagement.Structure;
using UnityEngine;

namespace Projects.Scripts.Effects.ProviderStrategies
{
    public class ProvideItem : BaseAssetProvideStrategy
    {
        private readonly ReceiveHolderType _itemType;
        private readonly int _maxStack;

        public ProvideItem(ReceiveHolderType itemType, int maxStack = 999)
        {
            _itemType = itemType;
            _maxStack = maxStack;
        }

        public override ReceiveHolderType AssetType => _itemType;

        public override int GetCurrentValue(PlayerData data)
        {
            return _itemType switch
            {
                ReceiveHolderType.Item1 => data.Item1Value,
                ReceiveHolderType.Item2 => data.Item2Value,
                ReceiveHolderType.Item3 => data.Item3Value,
                ReceiveHolderType.Item4 => data.Item7Value,
                ReceiveHolderType.Boost1 => data.Item4Value,
                ReceiveHolderType.Boost2 => data.Item5Value,
                ReceiveHolderType.Boost3 => data.Item6Value,
                _ => 0
            };
        }

        public override bool ApplyValue(PlayerData data, int newValue)
        {
            newValue = Mathf.Min(newValue, _maxStack);
            
            switch (_itemType)
            {
                case ReceiveHolderType.Item1:
                    data.Item1Value = newValue;
                    break;
                case ReceiveHolderType.Item2:
                    data.Item2Value = newValue;
                    break;
                case ReceiveHolderType.Item3:
                    data.Item3Value = newValue;
                    break;
                case ReceiveHolderType.Item4:
                    data.Item7Value = newValue;
                    break;
                case ReceiveHolderType.Boost1:
                    data.Item4Value = newValue;
                    break;
                case ReceiveHolderType.Boost2:
                    Debug.Log($"Currency Value : {data.Item5Value} | NewValue : {newValue}");
                    data.Item5Value = newValue;
                    Debug.Log($"Update Value : {data.Item5Value}");
                    break;
                case ReceiveHolderType.Boost3:
                    Debug.Log($"Currency Value : {data.Item6Value} | NewValue : {newValue}");
                    data.Item6Value = newValue;
                    Debug.Log($"Update Value : {data.Item6Value}");
                    break;
                default:
                    return false;
            }
            return true;
        }

        public override bool ValidateAmount(int amount) => amount > 0 && amount <= _maxStack;

        public override void OnAssetApplied(int finalValue)
        {
            base.OnAssetApplied(finalValue);
  
            if (finalValue >= _maxStack)
            {
                Debug.Log($"Item {_itemType} stack is full!");
            }
        }
    }
}