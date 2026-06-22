using GridRun.Core;
using UnityEngine;

namespace GridRun.Content
{
    [CreateAssetMenu(menuName = "GridRun/Upgrade Definition")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private UpgradeKind _kind;
        [SerializeField] private int _cost;
        [SerializeField] private int _value;

        public string Id
        {
            get { return _id; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public UpgradeKind Kind
        {
            get { return _kind; }
        }

        public int Cost
        {
            get { return _cost; }
        }

        public int Value
        {
            get { return _value; }
        }
    }
}
