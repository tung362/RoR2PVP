# RoR2PVP
RoR2PVP is a mod that adds a free for all and team PVP game mode to the game Risk of Rain 2.  
Note: Everything is server sided, so only host needs this mod installed

(Optional: non host players can also vote on settings if they have the mod installed but isn't needed necessarily to play)  
![alt text](https://i.imgur.com/vksJWY8.png)  

Menus  
![alt text](https://i.imgur.com/onUtK43.png)  
![alt text](https://i.imgur.com/BSXNwk0.gif)  
![alt text](https://i.imgur.com/9wwsxXW.png)  
![alt text](https://i.imgur.com/yslNCEw.png)  

Also unlocks all artifacts  
![alt text](https://i.imgur.com/SQzJkPB.png)  

### Videos of mod in action:  
**PVP Mod With PlayerBots Mod**  
[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/a9G1v51GITQ/0.jpg)](https://youtu.be/a9G1v51GITQ)  
**Vanilla Characters**  
[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/H9WwMHg8jIE/0.jpg)](https://youtu.be/H9WwMHg8jIE)  
**Bandit and Sniper Only (Old)**  
[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/UEWw_pyDfuM/0.jpg)](https://youtu.be/UEWw_pyDfuM)  

### Found a bug? Want a feature added?:  
Feel free to submit an issue here on my [github](https://github.com/tung362/RoR2PVP/issues)!  

### To do:  
- **~~Teams~~ ✔️**
- **~~Free for all~~ ✔️**
- **King of the hill**
- **Capture the flag**

### Features:  
- **Vanilla and Modded client compatibility**
  - Can be played with both vanilla and modded players should the host have the mod installed
  
- **PVP game modes**
  - Free For All PVP game mode
  - Team PVP game mode
  - Game mode rule voting system while in the lobby menu
    - Free For All/Team/Vanilla gamemodes
    - Enable/Disable randomized or fixed teams
    - Enable/Disable mob spawns
    - Enable/Disable banning of items
    - Enable/Disable companion item share
    - Enable/Disable custom characters
    - Enable/Disable custom loot generation
    - Enable/Disable extra death planes
    - Enable/Disable randomized stage progression with a wider selection
  - In-game menu and config entry for banning items and equipments (includes loading and saving presets)
  - In-game menu and config entry for playing custom characters, you and others can even play as mobs and hidden characters! (includes loading and saving presets)
  - Config entry for changing to vanilla version number, can be used to host vanilla lobbies
  - Config entry for setting the multiplayer limit, can play with up to 16 players!
  - Config entries for changing game mode settings
  
- **Extras**
  - Unlocking of all artifacts
  - Unlocking of all vanilla characters and loadouts
  
- **Includes 4 config files to balance your game as you see fit**
  - PVP.cfg
  - PVPCustomPlayableCharacters.cfg
  - PVPBannedItemList.cfg
  - PVPCustomInteractablesSpawner.cfg

### Requirements:  
[BepInExPack 5.3.1](https://thunderstore.io/package/download/bbepis/BepInExPack/5.3.1/)  
[R2API 2.5.28](https://thunderstore.io/package/download/tristanmcpherson/R2API/2.5.28/)  
Friends :(  

### Installation:  
1. Install BepInExPack (Version is provided above)
2. Install R2API (Version is provided above)
3. Download and unzip RoR2PVP (From releases or on thunderstore.io)
4. Place `RoR2PVP.dll` into your `\Risk of Rain 2\BepInEx\plugins\` folder

### Configuration:  
1. Run your game at least once with the RoR2PVP mod installed
2. Navigate to `\Risk of Rain 2\BepInEx\config`
3. Open `PVP.cfg` with any text editor
5. Open `PVPBannedItemList.cfg` with any text editor
6. Open `PVPCustomPlayableCharacters.cfg` with any text editor
7. Open `PVPCustomInteractablesSpawner.cfg` with any text editor
8. Edit the configs as you see fit

**Default config values**  
**`PVP.cfg`**  

| Keys                          | Default values |
| ----------------------------- | -------------- |
| Grace Timer Duration          |            60  |
| Cash Delay                    |            10  |
| Cash Grant Amount             |            70  |
| Respawns Per Round            |             2  |
| Max Multiplayer Count         |             4  |
| Modded                        |          true  |
| Unlock All                    |         false  |

**`PVPBannedItemList.cfg`**  


| Default banned items    |
| ----------------------- |
| Behemoth                |
| NovaOnHeal              |
| ShockNearby             |
| SprintWisp              |
| Thorns                  |
| LunarUtilityReplacement |
| Tonic                   |
| Meteor                  |
| BurnNearby              |
| BFG                     |
| Blackhole               |
| Lightning               |
| CommandMissile          |
| FireBallDash            |
| GoldGat                 |
| DroneBackup             |
| DeathProjectile         |


**`PVPCustomPlayableCharacters.cfg`**  

| Character slots | Default custom characters   |
| --------------- | --------------------------- |
| Commando slot   |               AssassinBody  |
| Huntress slot   |               AssassinBody  |
| MUL-T slot      |               AssassinBody  |
| Engineer slot   |               BanditBody    |
| Artificer slot  |               BanditBody    |
| Mercenary slot  |               BanditBody    |
| REX slot        |               SniperBody    |
| Loader slot     |               SniperBody    |
| Acrid slot      |               SniperBody    |
| Captain slot    |               SniperBody    |


**`PVPCustomInteractablesSpawner.cfg`**  

| keys                        | Default Values |
| --------------------------- | -------------- |
| Mega Drone Amount           |             0  |
| Mega Drone Price            |           300  |
| Gunner Drone Amount         |             0  |
| Gunner Drone Price          |            -1  |
| Missile Drone Amount        |             0  |
| Missile Drone Price         |            -1  |
| Healer Drone Amount         |             8  |
| Healer Drone Price          |            -1  |
| Equipment Drone Amount      |             0  |
| Flame Drone Amount          |             0  |
| Flame Drone Price           |            -1  |
| Turret Amount               |             0  |
| Turret Price                |            -1  |
| Shrine Of Order Amount      |             2  |
| Shrine Of Blood Amount      |             0  |
| Shrine Of Chance Amount     |             3  |
| Shrine Of Chance Price      |            -1  |
| Shrine Of Combat Amount     |             0  |
| Shrine Of Healing Amount    |             0  |
| Shrine Of Healing Price     |            -1  |
| Gold Shrine Amount          |             0  |
| Gold Shrine Price           |            -1  |
| Capsule Amount              |             0  |
| Radar Tower Amount          |             1  |
| Radar Tower Price           |            -1  |
| Celestial Portal Amount     |             0  |
| Shop Portal Amount          |             0  |
| Duplicator Amount           |             2  |
| Duplicator Large Amount     |             1  |
| Duplicator Military Amount  |             0  |
| Gold Chest Amount           |             2  |
| Gold Chest Price            |            300 |
| Small Chest Amount          |            16  |
| Small Chest Price           |            -1  |
| Large Chest Amount          |             8  |
| Large Chest Price           |            -1  |
| Damage Chest Amount         |             4  |
| Damage Chest Price          |            -1  |
| Healing Chest Amount        |             4  |
| Healing Chest Price         |            -1  |
| Utility Chest Amount        |             4  |
| Utility Chest Price         |            -1  |
| Triple Shop Amount          |             3  |
| Triple Shop Price           |            -1  |
| Triple Shop Large Amount    |             3  |
| Triple Shop Large Price     |            -1  |
| Equipment Barrel Amount     |             6  |
| Equipment Barrel Price      |            -1  |
| Lockbox Amount              |             4  |
| Lunar Chest Amount          |             4  |

### Compatibility: 
- If you have any custom character mods that conflicts with this mod set `Custom Playable Characters` to off in the lobby menu
- If you have any custom companion item giving mods that conflicts with this mod set `Companions Share Items` to off in the lobby menu
- If you have any custom maps/generation mods that conflicts with this mod set `Custom Interactables Spawner` to off in the lobby menu
- Note to modders with custom maps: In this mod a failsafe kill zone is generated at y coord -2200 if that is a problem set `Use Death Plane Failsafe` to off in the lobby menu, just keep in mind that there will be no failsafes should this be disabled.

### Credits:  
Special thanks to my friend Riley for helping me test https://github.com/SimpleManGames  
Special thanks to my friend Justin for helping me test https://github.com/Sethix  

### Change log:  
**1.5.1 (Current)**  
- Updated to R2API 2.5.28  
- Default config value for cash grant is changed from 50 to 70  
- Default config value for golden chest price is changed from 400 to 300  
- Companions (drones, turrets, etc) now attacks each other as intended in free for all pvp mode  
- Added "Sundered Grove" to the list of destinations when "Wider stage transition" is turned on  
**1.5.0**  
- Added a Free For All game mode  
- https://imgur.com/PRBt3AP  
- Added a Team Picker menu to the lobby room, host can now pick people's teams in-game  
- Added a Item Banner menu to the lobby room, host can now ban items in-game without needing to edit the config and restarting the game  
- Added a Custom Playable Characters menu to the lobby room, host can now choose custom characters for everyone without needing to edit the config and restarting the game  
- Added Assassin to the default custom playable characters  
- Changed default grace time to 60 seconds instead of 120 seconds, less looting more fighting!  
- Fixed bug where mod would still run even when joining a vanilla lobby if hosted with mod enabled before joining  
- Fixed bug where duplicate entries in PVPBannedItemList.cfg would cause errors  
- Fixed bug where giving an empty item would cause errors  
**1.4.5**  
- Now compatible with PlayerBots mod (https://thunderstore.io/package/Meledy/PlayerBots/)  
  - https://youtu.be/a9G1v51GITQ  
- Added banned item enforce, preventing mods from giving banned items  
- Added banned equipment enforce, preventing mods from giving banned equipments  
- Fixed maps not randomizing before the first full loop when "Wider stage transition" is turned on  
**1.4.3**  
- Added logo to the title screen  
- Added "Wider stage transition" to the votable config system  
- Added a wider selection of stage transitions
- Added item "Strides of Heresy" to the default item ban list  
- Added equipment "Forgive Me Please" to the default item ban list  
- Added a second teleporter to the stage "skymeadow" so that players can loop stages instead of being forced to fight Mithrix ending the run  
- Added stage "arena" to the list of safe zones  
- Removed commando from the default custom playable characters (too powerful)  
- Removed stage "goldshores" from the list of safe zones  
- Fixed filtering of disconnected and dead players from the team shuffle  
- Fixed frozen screen on game over when playing with a vanilla host  
**1.4.0**  
- Updated to BepInExPack 5.3.1 and R2API 2.5.14  
- Added character slot "Captain" to the Custom Playable Characters config  
- Added the ability to unlock all characters and loadouts temporarily (configurable in TeamPVP.cfg)  
- Mithrix stage is now considered a safe zone  
- Fixed max lobby size  
**1.3.5**  
- Updated to BepInExPack 3.2.0 and R2API 2.4.10  
- Mobs and pvp team 2 are no longer in the same team!  
- Added full unlock of artifacts  
- Added item "Spinel Tonic" to the default item ban list  
- Removed debug code left in from the previous build  
- Fixed bug where some areas would result in a softlock due to destroying essential gameobjects  
**1.3.0**  
- Updated to BepInExPack 3.1.0 and R2API 2.4.2  
- Added a votable config system for certain mod settings during lobby  
- Made the votable artifacts visible during lobby  
- Added item "Razorwire" to the default item ban list  
- Re-enabled config for removing the "Mod" build id for easy lobby hosting with non modded players (does not enable quickplay)  
- Fixed bug where players were able to spawn infinite fireworks using the teleporter  
- Migrated config entry "Random Teams" over to the votable config system  
- Migrated config entry "Companions Share Items" over to the votable config system  
- Migrated config entry "Disable Mob Spawn" over to the votable config system  
- Migrated config entry "Custom Interactables Spawner" over to the votable config system  
- Migrated config entry "Ban Items" over to the votable config system  
- Migrated config entry "Custom Playable Characters" over to the votable config system  
- Migrated config entry "Use Death Plane Failsafe" over to the votable config system  
**1.1.3**  
- Updated to R2API 2.3.7  
- Added equipment "The Back-up" to the default item ban list  
- Added character slot "Acrid" to the Custom Playable Characters config  
**1.1.2**  
- Updated to BepInExPack 3.0.0 and R2API 2.3.0  
**1.1.1**  
- Removed physical failsafe death plane for a player Y coord check  
**1.1.0**  
- Added commando to the default custom playable characters  
- Added custom generation config (Balance the spawns how you like!)  
- Added compatibility notice to the readme to make a few things clear  
- Added money scaling when Custom Interactables Spawner is false  
- Tweaked the config  
- Buffed sniper (No more slow debuff when using the scope and instead gains an attack speed buff)  
- Added built-in kill zone (height: -2200) as a fail safe if you fall past the vanilla kill zone to prevent soft lock  
  - Note: if you still somehow get past this death plane, then there was either no CharacterBody component or you desynced from the server in which case i can't do anything for you  
- Fixed death planes softlocking certain players when teams are changed  
- Fixed bug where custom characters would still be notified to the player even when Custom Playable Characters is false  
- Fixed bug where companions wouldn't showing up in the ui for the player when pvp mode is enabled  
- Fixed bug where companions wouldn't attack certain players when pvp mode is enabled  
**1.0.2**  
- Removed leftovers  
**1.0.1**  
- Removed unallowed stuff  
**1.0.0**  
- Initial release  
