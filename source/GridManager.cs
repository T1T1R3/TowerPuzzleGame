using Godot;
using System.Collections.Generic;
using System.Linq;
using Game.Component;


namespace Game.Manager;

public partial class GridManager : Node
{
	private HashSet<Vector2I> occupiedCells = new HashSet<Vector2I>();
	[Export] private TileMapLayer highlightTileMapLayer;
	[Export] private TileMapLayer baseTerrainTileMapLayer;
	

	public bool IsTilePositionValid(Vector2I tilePosition)
	{
		var customData = baseTerrainTileMapLayer.GetCellTileData(tilePosition);
		if (customData == null) return false;
		if (!(bool)customData.GetCustomData("Buildable")) return false;
		return !occupiedCells.Contains(tilePosition);
	}

	public void TurnTileAsOccupied(Vector2I tilePosition)
	{
		occupiedCells.Add(tilePosition);
	}

	public void HighlightBuildableTiles()
	{
		ClearHighlightTiles();
		var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();
		foreach (var buildingComponent in buildingComponents)
		{
			HighlightValidTilesInRadius(buildingComponent.GetGridCellPosition(), buildingComponent.BuildableRadius);
		}
	}

	public void ClearHighlightTiles()
	{
		highlightTileMapLayer.Clear();
	}
	
	public Vector2I GetMouseGridPos()
	{
		var mousePosition = highlightTileMapLayer.GetGlobalMousePosition();
		var gridPosition = mousePosition / 64;
		gridPosition = gridPosition.Floor();
		return new Vector2I((int)gridPosition.X, (int)gridPosition.Y);
	}
	
	private void HighlightValidTilesInRadius(Vector2I rootCell, int radius)
	{
		
		for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
		{
			for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
			{
				var tilePosition = new Vector2I(x, y);
				if(!IsTilePositionValid(tilePosition)) continue;
				highlightTileMapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
			}
		}
	}
}
