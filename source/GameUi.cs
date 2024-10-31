using Godot;
using Game.Resources.Building;
using System;

namespace Game.UI;
public partial class GameUi : MarginContainer
{
	[Signal]
	public delegate void BuildingResourceSelectedEventHandler(BuildingResource buildingResource);

	[Export] private BuildingResource[] buildingResources;

	private HBoxContainer HBoxCont;

	public override void _Ready()
	{
		HBoxCont = GetNode<HBoxContainer>("HBoxContainer");
		CreatingBuildingButtons();
	}

	private void CreatingBuildingButtons()
	{
		foreach (var buildingResource in buildingResources)
		{
			
			var buildingButton = new Button();
			buildingButton.Text = $"Place {buildingResource.DisplayName}";
			HBoxCont.AddChild(buildingButton);

			buildingButton.Pressed += () =>
			{
				EmitSignal(SignalName.BuildingResourceSelected, buildingResource);
			};
		}
	}

}
