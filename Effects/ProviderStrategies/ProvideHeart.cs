using System;
using System.Globalization;
using JSAM;
using Projects.DataManagement.Data;
using Projects.DataManagement.Data.Cloud.SaveSystem.Core;
using Projects.DataManagement.Structure;
using Projects.Heart;
using UnityEngine;

namespace Projects.Scripts.Effects.ProviderStrategies
{
    public class ProvideHeart : BaseAssetProvideStrategy
    {
        public override ReceiveHolderType AssetType => ReceiveHolderType.Heart;

        public override int GetCurrentValue(PlayerData data) => data.Heart;

        public override bool ApplyValue(PlayerData data, int newValue)
        {
            HeartSystem.AddHearts(newValue);
            return true;
        }
        
        public override bool ValidateAmount(int amount) => amount is > 0 and <= 5;

        public override void OnDeliveryStarted(int amount)
        {
            base.OnDeliveryStarted(amount);
            AudioManager.PlaySound(LibSounds.Click);
        }

        public override void OnAssetApplied(int finalValue)
        {
            base.OnAssetApplied(finalValue);
            
            if (finalValue >= 5)
            {
                // ShowNotification("하트가 가득 찼습니다!");
            }
        }
    }
    
    public class ProvideUnlimitedHeart : BaseAssetProvideStrategy
    {
        public override ReceiveHolderType AssetType => ReceiveHolderType.UnlimitedHeart;
    
        public override int GetCurrentValue(PlayerData data)
        {
            if (DateTime.TryParse(data.EndPackageHeartTime, out var endTime)) 
                return endTime > DateTime.Now ? 1 : 0;
            return 0;
        }
    
        public override bool ApplyValue(PlayerData data, int newValue)
        {
            try
            {
                DateTime newEndTime;
            
                if (DateTime.TryParse(data.EndPackageHeartTime, out var currentEndTime))
                {
                    newEndTime = currentEndTime > DateTime.Now 
                        ? currentEndTime.AddMinutes(newValue) 
                        : DateTime.Now.AddMinutes(newValue);
                }
                else newEndTime = DateTime.Now.AddMinutes(newValue);
                data.EndPackageHeartTime = newEndTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                HeartSystem.NotifyHeartChanged();
            
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to apply unlimited heart: {ex.Message}");
                return false;
            }
        }
    
        public override bool ValidateAmount(int amount) => amount > 0 && amount <= 1440; // 최대 24시간
    
        public override void OnAssetApplied(int finalValue)
        {
            base.OnAssetApplied(finalValue);
            Debug.Log($"Unlimited heart applied for {finalValue} hours");
        }
    }
}