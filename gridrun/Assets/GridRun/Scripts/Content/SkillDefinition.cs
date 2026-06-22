using GridRun.Core;
using UnityEngine;

namespace GridRun.Content
{
    [CreateAssetMenu(menuName = "GridRun/Skill Definition")]
    public sealed class SkillDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private int _cpuCost;
        [SerializeField] private SkillTargetingMode _targetingMode;
        [SerializeField] private Sprite _icon;

        public string Id
        {
            get { return _id; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public int CpuCost
        {
            get { return _cpuCost; }
        }

        public SkillTargetingMode TargetingMode
        {
            get { return _targetingMode; }
        }

        public Sprite Icon
        {
            get { return _icon; }
        }
    }
}
