Adds mounts, mastery skills and novelty icons in the form of radial, icon rows and corner icons.

Settings need to be configured before using the module!


## Settings

### keybinds
The keybinds in the module need to match the ones in you in-game settings:

1. Enable the module
  ![](./readme/enable_module.png)
1. Go to settings
  ![](./readme/go_to_settings.png)
1. Match the keybinds to the game settings and configure a keybind (purple)
  ![](./readme/keybinds.png)
1. When you are just standing on the ground, not in combat, etc you should see this result when you press the keybind.
  ![](./readme/initial_setup_result.png)

There are have 3 types of UI elements visible:

1. radial
- keybind
- default radial
- custom radials per context of your character
- evaluation order
- automatic activation when single option selected
- show bounds via debug

1. icon rows
- draggable
- different rows

1. corner icons


- custom icons (folder, diff name, restart modile)
- default mount, selected when dragging mouse + center option
- out of combat queuing

### debugging option and logging
Before reproducing an issue, please do the following steps:
- make a screenshot of the relevant module settings and in game settings
- turn on debug logs in BlishHUD (Settings > Overlay settings > enable debug logging).
  ![](./readme/debug_info.png)
You'll see extra logging and extra debug output on the screen like so:
  ![](./readme/debug_info_result.png)


Then reproduce the issue in as little steps as possible.

Add the latest log file and screenshots with the problem description. Logs can be found at:
Documents\Guild Wars 2\addons\blishhud\logs (or a similar folder inside your Settings folder when running in portable mode).

## FAQ

### Q: I want to support you
A: I don't expect anything in return, but if you want you can:
- send some gold/items ingame: Bennieboj.2607
- donate via https://ko-fi.com/bennieboj

### Q: Can I configure mouse buttons for keybinds?
A: No, this is a limitation of BlishHUD atm, see [this issue](https://github.com/blish-hud/Blish-HUD/issues/611) for more information.

### Q: Skyscale/skyscale leap is not possible in combat even when I have combat launch mastery
A: These masteries are not available in the API yet, see these issues for more information:
  - https://github.com/gw2-api/issues/issues/31
  - https://github.com/gw2-api/issues/issues/32

You can however enable the "Combat mastery unlocked" option in the General Settings tab to fix this.

## Releases

GitHub releases are out of date.
Recent releases moved to in-Blish-HUD repository: https://blishhud.com/docs/user/installing-modules.

## Credits
- [Manlaan](https://github.com/manlaan) for the original implementation with only icons and mounts.
- Ghost for the original transparant mount art for Raptor, Springer, Skimmer, Jackal, Griffon, Roller Beetle, Skyscale and Warclaw (taken from https://github.com/Friendly0Fire/GW2Radial/blob/master/readme.md with FriendlyFire's permission.)
- Vixen for the transparant mount art for Siege Turtle.
- maanlichtje for the new transparant mount art, both colored and white with and without outline.