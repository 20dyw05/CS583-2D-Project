# Elemental Incremental

CS 583 Individual 2D Game Project: a 2D idle/incremental factory game made in Unity 6 (6000.6.2f1), inspired by the Roblox tycoon Miner's Haven.

Changes from the one-pager and notes on AI use are in [Design Changes and AI Notes.txt](Design%20Changes%20and%20AI%20Notes.txt).

## Running the game

1. Open this folder as a project in Unity Hub, with Unity 6000.6.2f1.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Set the Game view to a 16:9 resolution (1920x1080) and press Play.

## How to play

1. Open the Inventory (**I**) and place the Condenser, some Conveyors and the Transmutator so that the Elements the Condenser drops ride the belts into the Transmutator. Elements that fall off the board are lost.
2. Every Element that reaches a Transmutator is turned into mana.
3. Buy Catalysts in the Shop (**P**) and add them to the line to raise each Element's value before it's sold. Hover over any item to see what it does and its limits.
4. Click a placed item to Upgrade, Move or Stash it.
5. Once you can afford it, click **Rebirth**: the board resets, your income goes up for good, and you win random reward items (Common to Legendary) that you keep forever.

## Controls

| Input | Action |
| --- | --- |
| **I** / **P** | Open the Inventory / the Shop |
| Left-click an empty tile | Place the item picked in the Inventory |
| Left-click an item | Open its menu (Upgrade, Move, Stash) |
| **R** | Turn what you're placing, or move the selected item |
| **Z** / right-click an item | Stash it back into the Inventory |
| **Esc** | Stop placing or close a window |
| **G** | God Mode: everything is free (for demos) |
