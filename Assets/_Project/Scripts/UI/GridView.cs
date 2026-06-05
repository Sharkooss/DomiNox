using System.Collections.Generic;
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
        private static readonly Color[] DominoBorderPalette =
        {
            new Color(0.95f, 0.73f, 0.22f),
            new Color(0.32f, 0.72f, 1f),
            new Color(0.75f, 0.45f, 1f),
            new Color(0.28f, 0.9f, 0.55f),
            new Color(1f, 0.48f, 0.45f)
        };
        private GameFlowController controller;

        public void Initialize(GameFlowController flowController)
        {
            controller = flowController;
            var layout = gameObject.AddComponent<GridLayoutGroup>();
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = GameConstants.GridWidth;
            layout.cellSize = new Vector2(56f, 56f);
            layout.spacing = new Vector2(3f, 3f);

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
            var borderColors = BuildPlacedDominoBorderColors(grid);
            for (var y = 0; y < GameConstants.GridHeight; y++)
            {
                for (var x = 0; x < GameConstants.GridWidth; x++)
                {
                    var position = new GridPosition(x, y);
                    var placed = grid.GetAt(position);
                    if (placed == null)
                    {
                        cells[x, y].SetContent(string.Empty, false, Color.clear);
                        continue;
                    }

                    grid.TryGetCellValue(position, out var value);
                    cells[x, y].SetContent(value.ToString(), true, borderColors[placed]);
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
                    var value = GridState.GetCellValue(domino, orientation, index);
                    cells[cell.X, cell.Y].SetPreview(value.ToString(), valid);
                }

                index++;
            }
        }

        private static Dictionary<PlacedDomino, Color> BuildPlacedDominoBorderColors(GridState grid)
        {
            var colors = new Dictionary<PlacedDomino, Color>();
            var placedDominoes = grid.GetPlacedDominoes();
            for (var i = 0; i < placedDominoes.Count; i++)
            {
                colors[placedDominoes[i]] = DominoBorderPalette[i % DominoBorderPalette.Length];
            }

            return colors;
        }
    }
}
