using Godot;
using System;

namespace Game.UI;
public partial class GameUi : MarginContainer
{
	[Signal] public delegate void PlaceTowerPressedEventHandler();
	[Signal] public delegate void PlaceVillagePressedEventHandler();
	
	private Button placeTowerButton;
	private Button placeVillageButton;
	
	public override void _Ready()
	{
		placeTowerButton = GetNode<Button>("%PlaceTowerButton");
		placeVillageButton = GetNode<Button>("%PlaceVillageButton");

		placeTowerButton.Pressed += OnPlaceTowerButtonPressed;
		placeVillageButton.Pressed += OnPlaceVillageButtonPressed;
	}

	private void OnPlaceTowerButtonPressed()
	{
		EmitSignal(SignalName.PlaceTowerPressed);
	}

	private void OnPlaceVillageButtonPressed()
	{
		EmitSignal(SignalName.PlaceVillagePressed);
	}
}
