using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Component;


namespace Game.Manager;

public partial class GridManager : Node
{
	private const string IS_BUILDABLE = "is_buildable";
	private const string IS_WOOD = "is_wood";

	[Signal]
	public delegate void ResourceTilesUpdatedEventHandler(int collectedTiles);
	
	private HashSet<Vector2I> validBuildableTiles = new HashSet<Vector2I>();
	private HashSet<Vector2I> collectedResourceTiles = new HashSet<Vector2I>();
	[Export] private TileMapLayer highlightTileMapLayer;
	[Export] private TileMapLayer baseTerrainTileMapLayer;

	private List<TileMapLayer> allTilemapLayers = new List<TileMapLayer>();

	public override void _Ready()
	{
		GameEvents.Instance.BuildingPlaced += OnBuildingPlaced;
		allTilemapLayers = GetAllTileMapLayers(baseTerrainTileMapLayer);
	}


	public bool TileHasCustomData(Vector2I tilePosition, string dataName)
	{
		foreach (var layer in allTilemapLayers)
		{
			var customData = layer.GetCellTileData(tilePosition);
			if (customData == null) continue;
			return (bool)customData.GetCustomData(dataName);	
		}

		return false;
	}

	public bool IsTilePositionBuildable(Vector2I tilePosition)
	{
		return validBuildableTiles.Contains(tilePosition);
	}

	public void HighlightBuildableTiles()
	{
		foreach (var tilePosition in validBuildableTiles)
		{
			highlightTileMapLayer.SetCell(tilePosition, 0, Vector2I.Zero);
		}
	}

	public void HighlightExpandedBuildableTile(Vector2I rootCell, int radius)
	{
		HighlightBuildableTiles();
		var validTiles = GetValidTilesRadius(rootCell, radius).ToHashSet();
		var expandedTiles = validTiles.Except(validBuildableTiles).Except(GetOccupiedTiles());
		var atlasCoords = new Vector2I(1, 0);
		foreach (var tilePosition in expandedTiles)
		{
			highlightTileMapLayer.SetCell(tilePosition, 0, atlasCoords);
		}
	}

	public void HighlightResourceTiles(Vector2I rootCell, int radius)
	{
		var resourceTiles = GetResourceTilesInRadius(rootCell, radius);
		var atlasCoords = new Vector2I(1, 0);
		foreach (var tilePosition in resourceTiles)
		{
			highlightTileMapLayer.SetCell(tilePosition, 0, atlasCoords);
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

	public List<TileMapLayer> GetAllTileMapLayers(TileMapLayer baseTileMapLayer)
	{
		var result = new List<TileMapLayer>();
		var reversed = baseTileMapLayer.GetChildren();
		reversed.Reverse();
		foreach (var child in baseTileMapLayer.GetChildren())
		{
			if (child is TileMapLayer childLayer)
			{
				result.AddRange(GetAllTileMapLayers(childLayer));
			}
			
		}
		result.Add(baseTileMapLayer);

		return result;
	}
	
	private void UpdateValidBuildableTiles(BuildingComponent buildingComponent)
	{
		var rootCell = buildingComponent.GetGridCellPosition();
		var validTiles = GetValidTilesRadius(rootCell, buildingComponent.BuildingResource.BuildableRadius);
		validBuildableTiles.UnionWith(validTiles);
		validBuildableTiles.ExceptWith(GetOccupiedTiles());
	}

	private void UpdateCollectedResourceTiles(BuildingComponent buildingComponent)
	{
		var rootCell = buildingComponent.GetGridCellPosition();
		var resourceTiles = GetResourceTilesInRadius(rootCell, buildingComponent.BuildingResource.ResourceRadius);
		
		var oldResourceTileCount = collectedResourceTiles.Count;
		collectedResourceTiles.UnionWith(resourceTiles);

		if (oldResourceTileCount != collectedResourceTiles.Count)
		{
			EmitSignal(SignalName.ResourceTilesUpdated, collectedResourceTiles.Count);	
		}
	}

	private List<Vector2I> GetTilesInRadius(Vector2I rootCell, int radius, Func<Vector2I, bool> filterFn)
	{
		var result = new List<Vector2I>();
		for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
		{
			for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
			{
				var tilePosition = new Vector2I(x, y);
				if(!filterFn(tilePosition)) continue;
				result.Add(tilePosition);	
			}
		}

		return result;
	}
	

	private List<Vector2I> GetValidTilesRadius(Vector2I rootCell, int radius)
	{
		return GetTilesInRadius(rootCell, radius,
			(tilePosition) =>
			{
				return TileHasCustomData(tilePosition, IS_BUILDABLE);
			});	
	}

	private List<Vector2I> GetResourceTilesInRadius(Vector2I rootCell, int radius)
	{
		return GetTilesInRadius(rootCell, radius,
			(tilePosition) =>
			{
				return TileHasCustomData(tilePosition, IS_WOOD);
			});	
	}

	private IEnumerable<Vector2I> GetOccupiedTiles()
	{
		var buildingComponents = GetTree().GetNodesInGroup(nameof(BuildingComponent)).Cast<BuildingComponent>();
		var occupiedTiles = buildingComponents.Select(x => x.GetGridCellPosition());
		return occupiedTiles;
	}

	private void OnBuildingPlaced(BuildingComponent buildingComponent)
	{
		UpdateValidBuildableTiles(buildingComponent);
		UpdateCollectedResourceTiles(buildingComponent);
	}
}
