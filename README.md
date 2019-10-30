# RoR2TeamPVP
RoR2TeamPVP is a mod that adds a team PVP game mode to the game Risk of Rain 2.  
Note: Everything is server sided, so only host needs this mod installed

### Old video example of mod in action (NOTE: a lot has been improved since):  
https://youtu.be/UEWw_pyDfuM

### Features:  
- **Custom Team PVP game mode**
  - PVP
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
  
- **Includes 3 config files to balance your game as you see fit**
  - TeamPVP.cfg
  - TeamPVPCustomPlayableCharacters.cfg
  - TeamPVPBannedItemList.cfg
  
- **Extra features for you to find**
  - Uka Uka Ouuu?

### Requirements:  
[BepInExPack 2.0.0](https://thunderstore.io/package/download/bbepis/BepInExPack/2.0.0/)  
[R2API 2.2.32](https://thunderstore.io/package/download/tristanmcpherson/R2API/2.2.32/)  
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
7. Edit the configs as you see fit

**Default Config Values**  
**`TeamPVP.cfg`**  

| Keys                          | Default values |
| ----------------------------- | -------------- |
| Grace Timer Duration          |           120  |
| Cash Delay                    |            10  |
| Cash Grant Amount             |            50  |
| Respawns Per Round            |             2  |
| Random Teams                  |          true  |
| Companions Share Items        |          true  |
| Disable Mob Spawn             |          true  |
| Custom Interactables Spawner  |          true  |
| Max Multiplayer Count         |             4  |
| Ban Items                     |          true  |
| Custom Playable Characters    |          true  |

**`TeamPVPBannedItemList.cfg`**  


| Default banned items |
| -------------------- |
| Behemoth             |
| NovaOnHeal           |
| ShockNearby          |
| SprintWisp           |
| Meteor               |
| BurnNearby           |
| BFG                  |
| Blackhole            |
| Lightning            |
| CommandMissile       |
| FireBallDash         |
| GoldGat              |


**`TeamPVPCustomPlayableCharacters.cfg`**  

| Character slots | Default custom characters |
| --------------- | ------------------------- |
| commando slot   |               BanditBody  |
| MUL-T slot      |               BanditBody  |
| Huntress slot   |               BanditBody  |
| Engineer slot   |               BanditBody  |
| Artificer slot  |               SniperBody  |
| Mercenary slot  |               SniperBody  |
| REX slot        |               SniperBody  |
| Loader slot     |               SniperBody  |

### Credits:  
Special thanks to my friend Riley for helping me test https://github.com/SimpleManGames

### Change log:  
**1.0.2 (Current)**  
- Removed leftovers  
**1.0.1 (Current)**  
- Removed unallowed stuff  
**1.0.0**  
- Initial release  
