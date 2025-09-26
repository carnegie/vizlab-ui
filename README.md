# UI (window system)

## Dependencies
* Nobi.UIRoundedCorners (Unity package for rounding UI assets, [download it here](https://github.com/kirevdokimov/Unity-UI-Rounded-Corners))

## How to use

Much of the window system is self-contained. At a high-level it involves windows which contains a set of paired menu contents and respective tabs. The windows float in 3D space using the Unity UI system. The player can then interact with the 3D menus using UI pointer events (including rearranging tabs and moving menu content out of a window or back into it via the tab).

One can instantiate window znd menu instances using the UIManager.

```
// construct player menu
UIManager.Instance.CreatePlayerMenu(ContentType.MainMenu);

// find UI content (the main menu in this case)
var content = UIManager.Instance.FindContentWindow(ContentType.MainMenu);

// delete menu
UIManager.Instance.DeleteContent(content);
```

The UIContentManager is also vital; in contrast to the UIManager, it manages dictionary of relevant prefabs and assets on disk.

```
// get icon for whatever purposes you have
UIContentManager.GetContentIcon(ContentType.FileBrowser);
```

To add content into the window system, you'll need to write a script that extends the Menu component, create a prefab for that menu instance, then update UIContentManager accordingly so UIManager has the right information to create the menu at runtime.