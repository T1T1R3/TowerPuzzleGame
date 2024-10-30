using Godot;
using Game.Manager;
using Game.Resources.Building;

namespace Game;

public partial class Main : Node
{
	private GridManager gridManager;
	private Sprite2D cursor;
	private BuildingResource towerResource;
	private BuildingResource villageResource;
	private Button placeTowerButton;
	private Button placeVillageButton;
	private Vector2I? hoveredGridCell;
	private Node2D ySortRoot;
	private BuildingResource toPlaceBuildingResource;
	
	

	public override void _Ready()
	{
		towerResource = GD.Load<BuildingResource>("res://resources/building/tower.tres");
		villageResource = GD.Load<BuildingResource>("res://resources/building/village.tres");
		gridManager = GetNode<GridManager>("GridManager");
		cursor = GetNode<Sprite2D>("Cursor");
		ySortRoot = GetNode<Node2D>("YSortRoot");
		placeTowerButton = GetNode<Button>("PlaceTowerButton");
		placeVillageButton = GetNode<Button>("PlaceVillageButton");
		
		cursor.Visible = false;
		placeTowerButton.Pressed += OnPlaceTowerButtonPressed;
		placeVillageButton.Pressed += OnPlaceVillageButtonPressed;
		gridManager.ResourceTilesUpdated += OnResourceTileUpdated;
	}

	public override void _UnhandledInput(InputEvent evt)
	{
		if (hoveredGridCell.HasValue && evt.IsActionPressed("left_click") && gridManager.IsTilePositionBuildable(hoveredGridCell.Value))
		{
			PlaceBuildAtHoveredCellPos();
			cursor.Visible = false;
		}
	}

	public override void _Process(double delta)
	{
		var gridPosition = gridManager.GetMouseGridPos();
		cursor.GlobalPosition = gridPosition * 64;
		if (toPlaceBuildingResource != null && cursor.Visible && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
		{
			hoveredGridCell = gridPosition;
			gridManager.ClearHighlightTiles();
			gridManager.HighlightExpandedBuildableTile(hoveredGridCell.Value, toPlaceBuildingResource.BuildableRadius);
			gridManager.HighlightResourceTiles(hoveredGridCell.Value, toPlaceBuildingResource.ResourceRadius);
		}
		
	}

	private void PlaceBuildAtHoveredCellPos()
	{
		if (!hoveredGridCell.HasValue) return;
		
		var building = toPlaceBuildingResource.BuildingScene.Instantiate<Node2D>();
		ySortRoot.AddChild(building);
		
		building.GlobalPosition = hoveredGridCell.Value * 64;
		
		hoveredGridCell = null;
		gridManager.ClearHighlightTiles();
	} 
	

	private void OnPlaceTowerButtonPressed()
	{
		toPlaceBuildingResource = towerResource;
		if (cursor.Visible)
		{
			cursor.Visible = false;
		hoveredGridCell = null;
		gridManager.ClearHighlightTiles();
		}
		else
			cursor.Visible = true;
		gridManager.HighlightBuildableTiles();
	}
	
	private void OnPlaceVillageButtonPressed()
	{
		toPlaceBuildingResource = villageResource;
		if (cursor.Visible)
		{
			cursor.Visible = false;
			hoveredGridCell = null;
			gridManager.ClearHighlightTiles();
		}
		else
			cursor.Visible = true;
		gridManager.HighlightBuildableTiles();
	}

	private void OnResourceTileUpdated(int resourceCount)
	{
		GD.Print(resourceCount);
	}
	
}
