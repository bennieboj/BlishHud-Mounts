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

1. icon rows
- draggable
- different rows

1. corner icons



- custom icons (folder, diff name, restart modile)
- default mount, selected when dragging mouse + center option
- out of combat queuing
- debug option + instructions

## FAQ

### Q: I want to support you
A: I don't expect anything in return, but if you want you can:
- send me some gold/items ingame: Bennieboj.2607
- donate via https://ko-fi.com/bennieboj

### Q: Can I configure mouse buttons for keybinds?
A: No, this is a limitation of BlishHUD atm, see [this issue](https://github.com/blish-hud/Blish-HUD/issues/611) for more information.

### Q: Skyscale/skyscale leap is not possible in combat even when I have combat launch mastery
A: These masteries are not available in the API yet, see these issues for more information:
  - https://github.com/gw2-api/issues/issues/31
  - https://github.com/gw2-api/issues/issues/32

## Releases

GitHub releases are out of date.
Recent releases moved to in-Blish-HUD repository: https://blishhud.com/docs/user/installing-modules.

## Credits
- [Manlaan](https://github.com/manlaan) for the original implementation with only icons and mounts.
- Ghost for the original transparant mount art for Raptor, Springer, Skimmer, Jackal, Griffon, Roller Beetle, Skyscale and Warclaw (taken from https://github.com/Friendly0Fire/GW2Radial/blob/master/readme.md with FriendlyFire's permission.)
- Vixen for the transparant mount art for Siege Turtle.
- maanlichtje for the new transparant mount art, both colored and white with and without outline.