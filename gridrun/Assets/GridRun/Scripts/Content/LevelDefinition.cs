using GridRun.Core;
using UnityEngine;

namespace GridRun.Content
{
    [CreateAssetMenu(menuName = "GridRun/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private ObjectiveKind _objectiveKind = ObjectiveKind.DefeatIce;
        [SerializeField] private int _moveLimit = 14;
        [SerializeField] private int _enemyHp = 720;

        public string Id
        {
            get { return _id; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public ObjectiveKind ObjectiveKind
        {
            get { return _objectiveKind; }
        }

        public int MoveLimit
        {
            get { return _moveLimit; }
        }

        public int EnemyHp
        {
            get { return _enemyHp; }
        }
    }
}
