using Projects.DataManagement.Structure;
using UnityEngine;

namespace Projects.Scripts.Effects.ProviderStrategies
{
    public abstract class BaseAssetProvideStrategy : IAssetProvideStrategy
    {
        public abstract ReceiveHolderType AssetType { get; }

        public abstract int GetCurrentValue(PlayerData data);

        public abstract bool ApplyValue(PlayerData data, int newValue);

        public virtual bool ValidateAmount(int amount) => amount > 0;

        public virtual void OnDeliveryStarted(int amount)
        {
            Log("Delivery started", amount);
        }

        public virtual void OnDeliveryCompleted(int amount)
        {
            Log("Delivery completed", amount);
        }

        public virtual void OnAssetApplied(int finalValue)
        {
            Log("Applied", finalValue);
        }

        private void Log(string message, int value)
        {
#if ACTIONFIT_DEBUG || UNITY_EDITOR
            Debug.Log($"[{AssetType}] {message} : {value}");
#endif
        }
    }
}