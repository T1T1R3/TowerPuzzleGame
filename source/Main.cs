using Godot;
using Game.Manager;

namespace Game;

public partial class Main : Node
{
	private GridManager gridManager;
	private Sprite2D cursor;
	private PackedScene buildingScene;
	private Button placeBuildingButton;
	private Vector2I? hoveredGridCell;
	public override void _Ready()
	{
		buildingScene = GD.Load<PackedScene>("res://scenes/building/building.tscn");
		gridManager = GetNode<GridManager>("GridManager");
		cursor = GetNode<Sprite2D>("Cursor");
		placeBuildingButton = GetNode<Button>("PlaceBuildingButton");
		cursor.Visible = false;
		placeBuildingButton.Pressed += buttonPressed;
		
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
		if (cursor.Visible && (!hoveredGridCell.HasValue || hoveredGridCell.Value != gridPosition))
		{
			hoveredGridCell = gridPosition;
			gridManager.HighlightExpandedBuildableTile(hoveredGridCell.Value, 3);
		}
	}

	private void PlaceBuildAtHoveredCellPos()
	{
		if (!hoveredGridCell.HasValue) return;
		
		var building = buildingScene.Instantiate<Node2D>();
		AddChild(building);
		
		building.GlobalPosition = hoveredGridCell.Value * 64;
		
		hoveredGridCell = null;
		gridManager.ClearHighlightTiles();
	} 
	

	private void buttonPressed()
	{
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
	
}
