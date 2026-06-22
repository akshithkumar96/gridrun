using GridRun.Core;
using UnityEngine;

namespace GridRun.Content
{
    [CreateAssetMenu(menuName = "GridRun/Node Definition")]
    public sealed class NodeDefinition : ScriptableObject
    {
        [SerializeField] private NodeKind _kind;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _hudColor = Color.white;

        public NodeKind Kind
        {
            get { return _kind; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public Sprite Icon
        {
            get { return _icon; }
        }

        public Color HudColor
        {
            get { return _hudColor; }
        }
    }
}
