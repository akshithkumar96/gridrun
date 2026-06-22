using System.Collections.Generic;
using GridRun.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GridRun.Runtime
{
    public sealed class GridRunGame : MonoBehaviour
    {
        private const int BoardSize = 8;

        private readonly Dictionary<BoardPosition, CellView> _cellViews = new Dictionary<BoardPosition, CellView>();
        private readonly Dictionary<string, Text> _skillLabels = new Dictionary<string, Text>();

        private HackRunController _controller;
        private CanvasScaler _canvasScaler;
        private RectTransform _hudPanel;
        private RectTransform _boardPanel;
        private RectTransform _skillPanel;
        private RectTransform _movesPanel;
        private RectTransform _statusPanel;
        private GridLayoutGroup _boardGrid;
        private GridLayoutGroup _skillGrid;
        private Text _titleText;
        private Text _hpText;
        private Text _cpuText;
        private Text _currencyText;
        private Text _enemyText;
        private Text _movesText;
        private Text _statusText;
        private RectTransform _hpFill;
        private RectTransform _cpuFill;
        private RectTransform _enemyFill;
        private Font _font;
        private BoardPosition? _selected;
        private string _pendingSkillId;
        private int _lastWidth;
        private int _lastHeight;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null)
            {
                _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            EnsureCamera();
            EnsureEventSystem();
            _controller = new HackRunController(new SystemRandomSource());
            _controller.StartDefaultRun();
            BuildInterface();
            ApplyResponsiveLayout(true);
            RefreshAll();
        }

        private void Update()
        {
            ApplyResponsiveLayout(false);
            RefreshAll();
        }

        private void EnsureCamera()
        {
            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = new Color(0.02f, 0.04f, 0.07f);
                return;
            }

            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.04f, 0.07f);
            camera.orthographic = true;
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        private void BuildInterface()
        {
            GameObject canvasObject = new GameObject("GridRun Canvas", typeof(RectTransform));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            _canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvasScaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            CreateBackground(canvasRect);
            BuildHud(canvasRect);
            BuildBoard(canvasRect);
            BuildMovesBadge(canvasRect);
            BuildSkillPanel(canvasRect);
            BuildStatusPanel(canvasRect);
        }

        private void CreateBackground(RectTransform parent)
        {
            RectTransform background = CreatePanel("Background", parent, new Color(0.02f, 0.04f, 0.07f, 1f));
            background.anchorMin = Vector2.zero;
            background.anchorMax = Vector2.one;
            background.offsetMin = Vector2.zero;
            background.offsetMax = Vector2.zero;

            for (int i = 0; i < 10; i++)
            {
                RectTransform line = CreatePanel("Data Trace " + i, background, i % 2 == 0
                    ? new Color(0.0f, 0.85f, 1.0f, 0.08f)
                    : new Color(0.9f, 0.0f, 1.0f, 0.07f));
                line.anchorMin = new Vector2(0f, 0.05f + i * 0.09f);
                line.anchorMax = new Vector2(1f, 0.05f + i * 0.09f);
                line.pivot = new Vector2(0.5f, 0.5f);
                line.sizeDelta = new Vector2(0f, 2f + i % 3);
                line.anchoredPosition = Vector2.zero;
            }
        }

        private void BuildHud(RectTransform parent)
        {
            _hudPanel = CreatePanel("Decker HUD", parent, new Color(0.03f, 0.08f, 0.12f, 0.93f));
            AddOutline(_hudPanel.gameObject, new Color(0f, 0.75f, 1f, 0.65f), new Vector2(2f, -2f));

            _titleText = CreateText("Title", _hudPanel, "DECKER HUD", 28, TextAnchor.UpperLeft, new Color(0.28f, 0.92f, 1f));
            SetStretch(_titleText.rectTransform, 24, 12, 24, 80);

            _hpText = CreateText("HP Text", _hudPanel, string.Empty, 20, TextAnchor.MiddleLeft, Color.white);
            SetRect(_hpText.rectTransform, new Vector2(0f, 0.55f), new Vector2(0f, 0.55f), new Vector2(230f, 28f), new Vector2(24f, -4f));
            RectTransform hpBar = CreateBar("HP Bar", _hudPanel, new Color(0.08f, 0.15f, 0.2f), new Color(0.1f, 0.7f, 1f), out _hpFill);
            SetRect(hpBar, new Vector2(0f, 0.55f), new Vector2(0f, 0.55f), new Vector2(380f, 18f), new Vector2(250f, -4f));

            _cpuText = CreateText("CPU Text", _hudPanel, string.Empty, 20, TextAnchor.MiddleLeft, Color.white);
            SetRect(_cpuText.rectTransform, new Vector2(0f, 0.3f), new Vector2(0f, 0.3f), new Vector2(230f, 28f), new Vector2(24f, -4f));
            RectTransform cpuBar = CreateBar("CPU Bar", _hudPanel, new Color(0.08f, 0.15f, 0.12f), new Color(0.1f, 0.9f, 0.35f), out _cpuFill);
            SetRect(cpuBar, new Vector2(0f, 0.3f), new Vector2(0f, 0.3f), new Vector2(380f, 18f), new Vector2(250f, -4f));

            _currencyText = CreateText("Currency Text", _hudPanel, string.Empty, 22, TextAnchor.MiddleRight, new Color(1f, 0.82f, 0.28f));
            SetRect(_currencyText.rectTransform, new Vector2(1f, 0.64f), new Vector2(1f, 0.64f), new Vector2(260f, 38f), new Vector2(-24f, -2f));

            _enemyText = CreateText("Enemy Text", _hudPanel, string.Empty, 18, TextAnchor.MiddleRight, new Color(1f, 0.35f, 0.44f));
            SetRect(_enemyText.rectTransform, new Vector2(1f, 0.28f), new Vector2(1f, 0.28f), new Vector2(260f, 30f), new Vector2(-24f, -2f));
            RectTransform enemyBar = CreateBar("Enemy Bar", _hudPanel, new Color(0.18f, 0.06f, 0.08f), new Color(1f, 0.12f, 0.22f), out _enemyFill);
            SetRect(enemyBar, new Vector2(1f, 0.12f), new Vector2(1f, 0.12f), new Vector2(260f, 14f), new Vector2(-24f, 0f));
        }

        private void BuildBoard(RectTransform parent)
        {
            _boardPanel = CreatePanel("Data Node Grid", parent, new Color(0.02f, 0.07f, 0.1f, 0.94f));
            AddOutline(_boardPanel.gameObject, new Color(0f, 0.8f, 1f, 0.9f), new Vector2(3f, -3f));
            _boardGrid = _boardPanel.gameObject.AddComponent<GridLayoutGroup>();
            _boardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _boardGrid.constraintCount = BoardSize;
            _boardGrid.spacing = new Vector2(6f, 6f);
            _boardGrid.padding = new RectOffset(12, 12, 12, 12);

            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    BoardPosition position = new BoardPosition(x, y);
                    CellView view = CreateCell(position);
                    _cellViews[position] = view;
                }
            }
        }

        private void BuildMovesBadge(RectTransform parent)
        {
            _movesPanel = CreatePanel("Moves Badge", parent, new Color(0.04f, 0.09f, 0.14f, 0.94f));
            AddOutline(_movesPanel.gameObject, new Color(0.48f, 0.86f, 1f, 0.55f), new Vector2(2f, -2f));
            _movesText = CreateText("Moves Text", _movesPanel, string.Empty, 34, TextAnchor.MiddleCenter, Color.white);
            SetStretch(_movesText.rectTransform, 6, 6, 6, 6);
        }

        private void BuildSkillPanel(RectTransform parent)
        {
            _skillPanel = CreatePanel("Software Rail", parent, new Color(0.03f, 0.08f, 0.12f, 0.93f));
            AddOutline(_skillPanel.gameObject, new Color(0f, 0.75f, 1f, 0.55f), new Vector2(2f, -2f));
            _skillGrid = _skillPanel.gameObject.AddComponent<GridLayoutGroup>();
            _skillGrid.padding = new RectOffset(12, 12, 12, 12);
            _skillGrid.spacing = new Vector2(12f, 12f);
            _skillGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _skillGrid.constraintCount = 1;

            foreach (RuntimeSkillDefinition skill in _controller.State.Skills)
            {
                CreateSkillButton(skill);
            }
        }

        private void BuildStatusPanel(RectTransform parent)
        {
            _statusPanel = CreatePanel("Status Panel", parent, new Color(0.03f, 0.08f, 0.12f, 0.9f));
            AddOutline(_statusPanel.gameObject, new Color(0.32f, 1f, 0.78f, 0.4f), new Vector2(1f, -1f));
            _statusText = CreateText("Status Text", _statusPanel, string.Empty, 22, TextAnchor.MiddleCenter, new Color(0.75f, 1f, 0.95f));
            SetStretch(_statusText.rectTransform, 12, 12, 8, 8);
        }

        private CellView CreateCell(BoardPosition position)
        {
            GameObject cellObject = new GameObject("Node " + position.X + "," + position.Y, typeof(RectTransform));
            cellObject.transform.SetParent(_boardPanel, false);
            Image image = cellObject.AddComponent<Image>();
            image.color = Color.white;
            Button button = cellObject.AddComponent<Button>();
            Outline outline = cellObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.9f);
            outline.effectDistance = new Vector2(0f, 0f);

            Text label = CreateText("Label", cellObject.GetComponent<RectTransform>(), string.Empty, 18, TextAnchor.MiddleCenter, Color.white);
            SetStretch(label.rectTransform, 4, 4, 4, 4);

            BoardPosition captured = position;
            button.onClick.AddListener(delegate { OnTileClicked(captured); });
            return new CellView(button, image, label, outline);
        }

        private void CreateSkillButton(RuntimeSkillDefinition skill)
        {
            GameObject buttonObject = new GameObject(skill.Id, typeof(RectTransform));
            buttonObject.transform.SetParent(_skillPanel, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = skill.Unlocked ? new Color(0.08f, 0.18f, 0.23f, 0.95f) : new Color(0.04f, 0.06f, 0.08f, 0.8f);
            Button button = buttonObject.AddComponent<Button>();
            button.interactable = skill.Unlocked;
            LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
            layout.preferredWidth = 136f;
            layout.preferredHeight = 96f;

            Text label = CreateText("Label", buttonObject.GetComponent<RectTransform>(), string.Empty, 17, TextAnchor.MiddleCenter, Color.white);
            SetStretch(label.rectTransform, 6, 6, 6, 6);
            _skillLabels[skill.Id] = label;

            string captured = skill.Id;
            button.onClick.AddListener(delegate { OnSkillClicked(captured); });
        }

        private void OnTileClicked(BoardPosition position)
        {
            if (_pendingSkillId != null)
            {
                _controller.ActivateSkill(_pendingSkillId, position);
                _pendingSkillId = null;
                _selected = null;
                RefreshAll();
                return;
            }

            if (!_selected.HasValue)
            {
                _selected = position;
                RefreshAll();
                return;
            }

            if (_selected.Value == position)
            {
                _selected = null;
                RefreshAll();
                return;
            }

            if (_selected.Value.IsAdjacentTo(position))
            {
                _controller.TrySwap(_selected.Value, position);
                _selected = null;
            }
            else
            {
                _selected = position;
            }

            RefreshAll();
        }

        private void OnSkillClicked(string skillId)
        {
            RuntimeSkillDefinition skill = _controller.State.Skills.Find(candidate => candidate.Id == skillId);
            if (skill == null || !skill.Unlocked)
            {
                return;
            }

            if (skill.TargetingMode == SkillTargetingMode.Tile)
            {
                if (!_controller.CanUseSkill(skill))
                {
                    _controller.ActivateSkill(skillId, null);
                    RefreshAll();
                    return;
                }

                _pendingSkillId = skillId;
                _selected = null;
                _controller.State.Status = "Select target for " + skill.DisplayName + ".";
                RefreshAll();
                return;
            }

            _controller.ActivateSkill(skillId, null);
            RefreshAll();
        }

        private void ApplyResponsiveLayout(bool force)
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            bool portrait = Screen.height > Screen.width;

            if (portrait)
            {
                _canvasScaler.referenceResolution = new Vector2(1080f, 1920f);
                SetRect(_hudPanel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(960f, 180f), new Vector2(0f, -28f));
                SetRect(_boardPanel, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(900f, 900f), new Vector2(0f, -40f));
                SetRect(_movesPanel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(150f, 110f), new Vector2(-70f, -245f));
                SetRect(_skillPanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(940f, 132f), new Vector2(0f, 42f));
                SetRect(_statusPanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(900f, 78f), new Vector2(0f, 196f));
                _skillGrid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                _skillGrid.constraintCount = 1;
                _skillGrid.cellSize = new Vector2(220f, 108f);
                SetBoardCellSize(900f);
            }
            else
            {
                _canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
                SetRect(_hudPanel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(960f, 144f), new Vector2(150f, -24f));
                SetRect(_boardPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(720f, 720f), new Vector2(150f, -36f));
                SetRect(_movesPanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(112f, 112f), new Vector2(-304f, 268f));
                SetRect(_skillPanel, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(178f, 560f), new Vector2(42f, 0f));
                SetRect(_statusPanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(760f, 62f), new Vector2(150f, 34f));
                _skillGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                _skillGrid.constraintCount = 1;
                _skillGrid.cellSize = new Vector2(154f, 125f);
                SetBoardCellSize(720f);
            }

            if (force)
            {
                Canvas.ForceUpdateCanvases();
            }
        }

        private void SetBoardCellSize(float boardSize)
        {
            float inner = boardSize - _boardGrid.padding.left - _boardGrid.padding.right - _boardGrid.spacing.x * (BoardSize - 1);
            float cell = inner / BoardSize;
            _boardGrid.cellSize = new Vector2(cell, cell);
        }

        private void RefreshAll()
        {
            RefreshHud();
            RefreshTiles();
            RefreshSkills();
        }

        private void RefreshHud()
        {
            HackRunState state = _controller.State;
            _hpText.text = "HP: " + state.PlayerHp + "/" + state.PlayerMaxHp;
            _cpuText.text = "CPU CYCLES: " + state.Cpu + "/" + state.CpuMax;
            _currencyText.text = "CURRENCY: " + state.Credits + " Cr";
            _enemyText.text = "FIREWALL: " + state.EnemyHp + "/" + state.EnemyMaxHp;
            _movesText.text = "Moves\nLeft\n" + state.MovesRemaining;
            _statusText.text = state.Status;
            SetFill(_hpFill, state.PlayerHp, state.PlayerMaxHp);
            SetFill(_cpuFill, state.Cpu, state.CpuMax);
            SetFill(_enemyFill, state.EnemyHp, state.EnemyMaxHp);
        }

        private void RefreshTiles()
        {
            foreach (KeyValuePair<BoardPosition, CellView> pair in _cellViews)
            {
                TileState tile = _controller.State.Board.GetTile(pair.Key);
                CellView view = pair.Value;
                view.Image.color = TileColor(tile);
                view.Label.text = TileLabel(tile);
                view.Button.interactable = tile != null && !tile.IsFrozen && !_controller.State.Won && !_controller.State.Lost;
                bool selected = _selected.HasValue && _selected.Value == pair.Key;
                view.Outline.effectDistance = selected ? new Vector2(4f, -4f) : new Vector2(0f, 0f);
            }
        }

        private void RefreshSkills()
        {
            foreach (RuntimeSkillDefinition skill in _controller.State.Skills)
            {
                Text label;
                if (!_skillLabels.TryGetValue(skill.Id, out label))
                {
                    continue;
                }

                if (!skill.Unlocked)
                {
                    label.text = "LOCKED";
                    label.color = new Color(0.55f, 0.65f, 0.75f);
                    continue;
                }

                string cost = skill.CpuCost > 0 ? "\nCPU " + skill.CpuCost : string.Empty;
                label.text = skill.DisplayName + cost;
                label.color = _controller.CanUseSkill(skill) ? Color.white : new Color(0.62f, 0.66f, 0.7f);
            }
        }

        private static void SetFill(RectTransform fill, int value, int max)
        {
            float percent = max <= 0 ? 0f : Mathf.Clamp01(value / (float)max);
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(percent, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
        }

        private string TileLabel(TileState tile)
        {
            if (tile == null)
            {
                return string.Empty;
            }

            string label;
            switch (tile.PowerUp)
            {
                case PowerUpKind.LaserDrill:
                    label = tile.Axis == PowerUpAxis.Column ? "LAS\nCOL" : "LAS\nROW";
                    break;
                case PowerUpKind.EmpBomb:
                    label = "EMP";
                    break;
                case PowerUpKind.RootAccessKey:
                    label = "ROOT\nKEY";
                    break;
                default:
                    label = NodeLabel(tile.Kind);
                    break;
            }

            if (tile.Kind == NodeKind.Glitch)
            {
                label += "\nT-" + tile.GlitchTurns;
            }

            if (tile.IsFrozen)
            {
                label += "\nFRZ";
            }

            return label;
        }

        private static string NodeLabel(NodeKind kind)
        {
            switch (kind)
            {
                case NodeKind.Attack:
                    return "ATK";
                case NodeKind.Defense:
                    return "DEF";
                case NodeKind.Cpu:
                    return "CPU";
                case NodeKind.Credits:
                    return "CR";
                case NodeKind.Glitch:
                    return "GL";
                default:
                    return "?";
            }
        }

        private static Color TileColor(TileState tile)
        {
            if (tile == null)
            {
                return Color.black;
            }

            Color color;
            switch (tile.Kind)
            {
                case NodeKind.Attack:
                    color = new Color(0.95f, 0.12f, 0.18f, 0.95f);
                    break;
                case NodeKind.Defense:
                    color = new Color(0.1f, 0.68f, 1f, 0.95f);
                    break;
                case NodeKind.Cpu:
                    color = new Color(0.12f, 0.88f, 0.32f, 0.95f);
                    break;
                case NodeKind.Credits:
                    color = new Color(1f, 0.72f, 0.18f, 0.95f);
                    break;
                case NodeKind.Glitch:
                    color = new Color(0.72f, 0.08f, 1f, 0.95f);
                    break;
                default:
                    color = Color.white;
                    break;
            }

            if (tile.PowerUp != PowerUpKind.None)
            {
                color = Color.Lerp(color, Color.white, 0.25f);
            }

            if (tile.IsFrozen)
            {
                color = Color.Lerp(color, new Color(0.42f, 0.48f, 0.56f), 0.55f);
            }

            return color;
        }

        private RectTransform CreatePanel(string name, RectTransform parent, Color color)
        {
            GameObject panelObject = new GameObject(name, typeof(RectTransform));
            panelObject.transform.SetParent(parent, false);
            Image image = panelObject.AddComponent<Image>();
            image.color = color;
            return panelObject.GetComponent<RectTransform>();
        }

        private Text CreateText(string name, RectTransform parent, string text, int size, TextAnchor anchor, Color color)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            Text label = textObject.AddComponent<Text>();
            label.font = _font;
            label.text = text;
            label.fontSize = size;
            label.alignment = anchor;
            label.color = color;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 10;
            label.resizeTextMaxSize = size;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private RectTransform CreateBar(string name, RectTransform parent, Color backgroundColor, Color fillColor, out RectTransform fill)
        {
            RectTransform bar = CreatePanel(name, parent, backgroundColor);
            RectTransform fillRect = CreatePanel(name + " Fill", bar, fillColor);
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fill = fillRect;
            return bar;
        }

        private static void AddOutline(GameObject target, Color color, Vector2 distance)
        {
            Outline outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 anchoredPosition)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
        }

        private static void SetStretch(RectTransform rect, float left, float right, float top, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private sealed class CellView
        {
            public CellView(Button button, Image image, Text label, Outline outline)
            {
                Button = button;
                Image = image;
                Label = label;
                Outline = outline;
            }

            public Button Button { get; }
            public Image Image { get; }
            public Text Label { get; }
            public Outline Outline { get; }
        }
    }
}
