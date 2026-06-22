using GridRun.Core;
using UnityEngine;

namespace GridRun.Content
{
    [CreateAssetMenu(menuName = "GridRun/ICE Definition")]
    public sealed class IceDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private int _maxHp = 720;
        [SerializeField] private int _turnInterval = 3;
        [SerializeField] private CountermeasureKind[] _countermeasures;

        public string Id
        {
            get { return _id; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public int MaxHp
        {
            get { return _maxHp; }
        }

        public int TurnInterval
        {
            get { return _turnInterval; }
        }

        public CountermeasureKind[] Countermeasures
        {
            get { return _countermeasures; }
        }
    }
}
