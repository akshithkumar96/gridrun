using UnityEngine;

namespace GridRun.Content
{
    [CreateAssetMenu(menuName = "GridRun/Battle Pass Definition")]
    public sealed class BattlePassDefinition : ScriptableObject
    {
        [SerializeField] private string _seasonId;
        [SerializeField] private string _displayName;
        [SerializeField] private int[] _rewardCreditAmounts;

        public string SeasonId
        {
            get { return _seasonId; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public int[] RewardCreditAmounts
        {
            get { return _rewardCreditAmounts; }
        }
    }
}
