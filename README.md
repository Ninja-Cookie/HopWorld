# <p align="center">![hopworld-icon]</p>
# <p align="center">HopWorld - Big Hops Opened Up</p>

&nbsp;

## About
**HopWorld** is a **[Big Hops](https://store.steampowered.com/app/1221480/Big_Hops/)** plugin, through **[BepInEx](https://github.com/BepInEx/BepInEx)**, which opens up the full explorable world from a New Game save file, with nothing collected.

HopWorld also allows you to play Bingo using [BingoSync](https://bingosync.com/), using the in-game `F3` button menu!

NOTES:
- HopWorld disables achievements
- HopWorld should have no effect on any existing Save File, other than access to skin options in the closet, but caution is advised, just in case.

## Installation
#### (If BepInEx is installed, you can skip to Step 4)
1. Go to **[BepInEx's Releases](https://github.com/BepInEx/BepInEx/releases)** and get the relevent BepInEx ZIP file.
   ##### (This will likely be BepInEx_win_x64, even on Linux, if the game runs through Proton)
2. Locate your games files, usually found in `/Steam/steamapps/common/Big Hops/`
   ##### (`Steam Library > Right-Click Big Hops > Selecting Properties... > Selecting Installed Files > Selecting Browse...`)
3. Open the BepInEx ZIP file, and extract the contents to the games folder.
   ##### (The BepInEx folder and other contents should now be in the same directory as `Big Hops.exe`)
4. Go to **[HopWorld's Releases](https://github.com/Ninja-Cookie/HopWorld/releases)** and get the latest ZIP/7z file containing the plugin.
   ##### (May require [7-Zip](https://7-zip.org/download.html))
5. Open the **HopWorld** ZIP/7z file, and extract the folder to inside your `\Big Hops\BepInEx\plugins\` folder.
   ##### (If a `plugins` folder does not exist, create one... the final result should look like `\Big Hops\BepInEx\plugins\HopWorld\` with two DLL files inside)

## Usage
- Just start a New Game, you will be placed into Duster Bluffs by the cactus juicer

## Bingo Usage
1. Open the [BingoSync](https://bingosync.com/) website, and start a room with any settings and the Nickname you will go by
2. Invite players to the website's room if any by providing them the link to it
3. In-Game, open the BingoSync menu using the `F3` key on your keyboard
4. Enter the valid information for the room:
- The Room ID (The random characters found in the rooms URL after `bingosync.com/room/`)
- The rooms Password
- The Player Name, which should match your nickname you use in the room
- The Player Color, which should match the color you use in the room
5. Once the rooms info is set up, press `Connect`
6. Once connected, you can press `Generate Board`, which will automatically set up a lock-out bingo game for Big Hops
7. All players can now start a New Game and get ready before setting off after a countdown

## Bingo Commands
- Once you're connected to a game, you can use the `!help` command to get a list of commands you can use, such as the `!start` command, which will begin a countdown on-screen for all players

## Rules
- Each objective on the board is 1 point, a bingo line (5 in a row either vertical, horizontal, or diagonal) is 1 bonus point, so for example, a full line of 5 filled objectives would be 1 point for each objective, plus 1 extra point for the line, equalling a total of 6 points for your team color

[hopworld-icon]: https://github.com/user-attachments/assets/4358b47f-499c-44f8-869a-68f6f5142526
