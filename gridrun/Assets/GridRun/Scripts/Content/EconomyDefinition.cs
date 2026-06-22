using UnityEngine;

namespace GridRun.Content
{
    [CreateAssetMenu(menuName = "GridRun/Economy Definition")]
    public sealed class EconomyDefinition : ScriptableObject
    {
        [SerializeField] private int _startingCredits = 3500;
        [SerializeField] private int _maxBattery = 5;
        [SerializeField] private int _batteryRechargeMinutes = 20;

        public int StartingCredits
        {
            get { return _startingCredits; }
        }

        public int MaxBattery
        {
            get { return _maxBattery; }
        }

        public int BatteryRechargeMinutes
        {
            get { return _batteryRechargeMinutes; }
        }
    }
}
