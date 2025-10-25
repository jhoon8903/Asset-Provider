using Projects.DataManagement.Structure;

namespace Projects.Scripts.Effects.ProviderStrategies
{
    public interface IAssetProvideStrategy
    {
        ReceiveHolderType AssetType { get; }
        int GetCurrentValue(PlayerData data);
        bool ApplyValue(PlayerData data, int newValue);
        bool ValidateAmount(int amount);
        void OnDeliveryStarted(int amount);
        void OnDeliveryCompleted(int amount);
        void OnAssetApplied(int finalValue);
    }
}