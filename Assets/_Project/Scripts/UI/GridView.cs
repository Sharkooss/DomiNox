using DomiNox.Core;
using DomiNox.Grid;
using DomiNox.Run;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class GridView : MonoBehaviour
    {
        private readonly GridCellView[,] cells = new GridCellView[GameConstants.GridWidth, GameConstants.GridHeight];
        private GameFlowController controller;

        public void Initialize(GameFlowController flowController)
        {
            controller = flowController;
            var layout = gameObject.AddComponent<GridLayoutGroup>();
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = GameConstants.GridWidth;
            layout.cellSize = new Vector2(64f, 64f);
            layout.spacing = new Vector2(4f, 4f);

            for (var y = 0; y < GameConstants.GridHeight; y++)
            {
                for (var x = 0; x < GameConstants.GridWidth; x++)
                {
                    var go = new GameObject($"Cell_{x}_{y}", typeof(RectTransform));
                    go.transform.SetParent(transform, false);
                    var cell = go.AddComponent<GridCellView>();
                    cell.Initialize(x, y, controller.TryPlaceSelected);
                    cells[x, y] = cell;
                }
            }
        }

        public void Render(GridState grid)
        {
            for (var y = 0; y < GameConstants.GridHeight; y++)
            {
                for (var x = 0; x < GameConstants.GridWidth; x++)
                {
                    var placed = grid.GetAt(new GridPosition(x, y));
                    cells[x, y].SetContent(placed?.Domino.ToString() ?? string.Empty, placed != null);
                }
            }
        }
    }
}
