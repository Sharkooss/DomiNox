using DomiNox.Core;
using DomiNox.Dominoes;
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

        public bool TryGetCellAtScreenPosition(Vector2 screenPosition, out int x, out int y)
        {
            for (var cellY = 0; cellY < GameConstants.GridHeight; cellY++)
            {
                for (var cellX = 0; cellX < GameConstants.GridWidth; cellX++)
                {
                    var rect = (RectTransform)cells[cellX, cellY].transform;
                    if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition))
                    {
                        x = cellX;
                        y = cellY;
                        return true;
                    }
                }
            }

            x = -1;
            y = -1;
            return false;
        }

        public void RenderPreview(GridState grid, DominoInstance domino, GridPosition position, DominoOrientation orientation, bool valid)
        {
            Render(grid);
            var index = 0;
            foreach (var cell in GridState.GetCells(position, orientation))
            {
                if (cell.X >= 0 && cell.X < GameConstants.GridWidth && cell.Y >= 0 && cell.Y < GameConstants.GridHeight)
                {
                    var value = index == 0 ? domino.Definition.Left : domino.Definition.Right;
                    cells[cell.X, cell.Y].SetPreview(value.ToString(), valid);
                }

                index++;
            }
        }
    }
}
