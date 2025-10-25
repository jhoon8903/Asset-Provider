using UnityEngine;

namespace Projects.Scripts.Effects
{
    public enum ReceiveHolderType
    {
        None = 0,
        Heart = 1, UnlimitedHeart = 2, Coin = 3, Ticket = 4, Star = 5, InGameStar = 6, 
        Item1 = 7, Item2 = 8, Item3 = 9,
        Boost1 = 10, Boost2 = 11, Boost3 = 12,
        SeasonPassGoods = 13, SeasonPassWall = 14,
        Random = 15, Item4 = 16,
        Ads = 17,
        MetaGoods = 18,
    }
    
    public struct DeliveryRequest
    {
        public ReceiveHolderType TargetType;
        public Vector2 StartScreenPosition;
        public int Amount;
        public float DeliveryDuration;
    }
    
    public struct UIUpdatePendingData
    {
        public ReceiveHolderType AssetType;
        public int OldDisplayValue;
        public int NewDisplayValue;
        public int ChangeAmount;
    }
}