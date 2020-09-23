# RoR2TeamPVP
RoR2TeamPVP is a mod that adds a team PVP game mode to the game Risk of Rain 2.  
Note: Everything is server sided, so only host needs this mod installed

(Optional: non host players can also vote on settings if they have the mod installed but isn't needed necessarily to play)  
![alt text](https://i.imgur.com/2871Zif.png)  

Also unlocks all artifacts  
![alt text](https://i.imgur.com/SQzJkPB.png)  

### Old video example of mod in action (NOTE: a lot has been improved since):  
https://youtu.be/UEWw_pyDfuM

### Found a bug? Want a feature added?:  
Feel free to submit an issue here on my [github](https://github.com/tung362/RoR2PVP/issues)!  

### Features:  
- **Custom Team PVP game mode**
  - PVP
  - lobby rule voting system
  - unlocking of all artifacts
  - configurable unlocking of all characters and loadouts
  - configurable randomized or fixed teams
  - configurable grace time until pvp starts
  - configurable periodic money gain during grace period
  - configurable respawn amount per match
  
- **Custom Playable Characters**
  - configurable playable characters while in the character select screen, list of characters included!
  
- **Custom map generation**
  - configurable custom map generation or vanilla
  - configurable disable or enable mob spawns
  
- **Ability to increase multiplayer limit**
  - configurable multiplayer limit for if you want to play with more people
  
- **Ability for companions to share items**
  - configurable giving of items to your companions
  
- **Ability to ban items and equipments**
  - configurable banning of items, list of items included!
  
- **Includes 4 config files to balance your game as you see fit**
  - TeamPVP.cfg
  - TeamPVPCustomPlayableCharacters.cfg
  - TeamPVPBannedItemList.cfg
  - TeamPVPCustomInteractablesSpawner.cfg
  
- **Extra features for you to find**
  - Uka Uka Ouuu?

### Requirements:  
[BepInExPack 5.3.1](https://thunderstore.io/package/download/bbepis/BepInExPack/5.3.1/)  
[R2API 2.5.14](https://thunderstore.io/package/download/tristanmcpherson/R2API/2.5.14/)  
Friends :(  

### Installation:  
1. Install BepInExPack (Version is provided above)
2. Install R2API (Version is provided above)
3. Download and unzip RoR2TeamPVP (From releases or on thunderstore.io)
4. Place `RoR2PVP.dll` into your `\Risk of Rain 2\BepInEx\plugins\` folder

### Configuration:  
1. Run your game at least once with the RoR2TeamPVP mod installed
2. Navigate to `\Risk of Rain 2\BepInEx\config`
3. Open `TeamPVP.cfg` with any text editor
5. Open `TeamPVPBannedItemList.cfg` with any text editor
6. Open `TeamPVPCustomPlayableCharacters.cfg` with any text editor
7. Open `TeamPVPCustomInteractablesSpawner.cfg` with any text editor
8. Edit the configs as you see fit

**Default Config Values**  
**`TeamPVP.cfg`**  

| Keys                          | Default values |
| ----------------------------- | -------------- |
| Grace Timer Duration          |           120  |
| Cash Delay                    |            10  |
| Cash Grant Amount             |            50  |
| Respawns Per Round            |             2  |
| Max Multiplayer Count         |             4  |
| Modded                        |          true  |
| Unlock All                    |         false  |

**`TeamPVPBannedItemList.cfg`**  


| Default banned items |
| -------------------- |
| Behemoth             |
| NovaOnHeal           |
| ShockNearby          |
| SprintWisp           |
| Thorns               |
| Tonic                |
| Meteor               |
| BurnNearby           |
| BFG                  |
| Blackhole            |
| Lightning            |
| CommandMissile       |
| FireBallDash         |
| GoldGat              |
| DroneBackup          |


**`TeamPVPCustomPlayableCharacters.cfg`**  

| Character slots | Default custom characters |
| --------------- | ------------------------- |
| commando slot   |             CommandoBody  |
| MUL-T slot      |             CommandoBody  |
| Huntress slot   |             CommandoBody  |
| Engineer slot   |               BanditBody  |
| Artificer slot  |               BanditBody  |
| Mercenary slot  |               BanditBody  |
| REX slot        |               SniperBody  |
| Loader slot     |               SniperBody  |
| Acrid slot      |               SniperBody  |
| Captain slot    |               SniperBody  |


**`TeamPVPCustomInteractablesSpawner.cfg`**  

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
| Gold Chest Price            |            -1  |
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
- If you have any custom character mods that conflicts with this mod set `Custom Playable Characters` to false in `TeamPVP.cfg`
- If you have any custom companion item giving mods that conflicts with this mod set `Companions Share Items` to false in `TeamPVP.cfg`
- If you have any custom maps/generation mods that conflicts with this mod set `Custom Interactables Spawner` to false in `TeamPVP.cfg`
- Note to modders with custom maps: In this mod a failsafe kill zone is generated at y coord -2200 if that is a problem set `Use Death Plane Failsafe` to false in `TeamPVP.cfg`. Just keep in mind that there will be no failsafes should this be disabled.

### Credits:  
Special thanks to my friend Riley for helping me test https://github.com/SimpleManGames  
Special thanks to my friend Justin for helping me test https://github.com/Sethix  

### Change log:  
**1.4.0 (Current)**  
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
- Buffed sniper(No more slow debuff when using the scope and instead gains an attack speed buff)  
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
