using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace windowsystem 
{
    public enum WindowType 
    {
        Base,
        Popup
    }

    public enum ContentType
    {
        Graph,
        MainMenu,
        SaveLoad,
        Querying,
        Scale,
        Color,
        Data,
        Tutorial,
        Camera,
        LoadFromDisk,
        Settings,
        GlobalOptions,
        QuickLoad,
        FileBrowser,
        Playback,
        Controls,
        Spectral
    }

    public enum PopupType 
    {
        FloatSelector,
        IntSelector,
        ColorSelector,
        ColormapSelector,
        UnitSelector
    }
    
    public static class UIContentManager 
    {
        private static readonly Dictionary<WindowType, GameObject> windowDict = new Dictionary<WindowType, GameObject> 
            { { WindowType.Base, Resources.Load<GameObject>("Prefabs/UI/Windows/MainWindow") },
              { WindowType.Popup, Resources.Load<GameObject>("Prefabs/UI/Windows/PopupWindow") }};

        private static readonly Dictionary<ContentType, GameObject> contentDict = new Dictionary<ContentType, GameObject>
            { { ContentType.Graph, Resources.Load<GameObject>("Prefabs/UI/Menus/Graph") },
              { ContentType.MainMenu, Resources.Load<GameObject>("Prefabs/UI/Menus/MainMenu")},
              { ContentType.Settings, Resources.Load<GameObject>("Prefabs/UI/Menus/Settings")},
              { ContentType.Camera, Resources.Load<GameObject>("Prefabs/UI/Menus/Camera")},
              { ContentType.Scale, Resources.Load<GameObject>("Prefabs/UI/Menus/ScaleMenu")},
              { ContentType.Querying, Resources.Load<GameObject>("Prefabs/UI/Menus/Querying")},
              { ContentType.Color, Resources.Load<GameObject>("Prefabs/UI/Menus/Color")},
              { ContentType.Data, Resources.Load<GameObject>("Prefabs/UI/Menus/DataMenu")},
              { ContentType.GlobalOptions, Resources.Load<GameObject>("Prefabs/UI/Menus/GlobalOptions")},
              { ContentType.LoadFromDisk, Resources.Load<GameObject>("Prefabs/UI/Menus/LoadFromDisk")},
              { ContentType.QuickLoad, Resources.Load<GameObject>("Prefabs/UI/Menus/QuickLoad")},
              { ContentType.Playback, Resources.Load<GameObject>("Prefabs/UI/Menus/Playback")},
              { ContentType.FileBrowser, Resources.Load<GameObject>("Prefabs/UI/Menus/FileBrowser")},
              { ContentType.Controls, Resources.Load<GameObject>("Prefabs/UI/Menus/Controls")},
              { ContentType.Spectral, Resources.Load<GameObject>("Prefabs/UI/Menus/Spectral")},
              { ContentType.SaveLoad, Resources.Load<GameObject>("Prefabs/UI/Menus/SaveLoad")}};

        private static readonly Dictionary<ContentType, Sprite> iconDict = new Dictionary<ContentType, Sprite>
            { { ContentType.Graph, Resources.Load<Sprite>("UIResources/HistogramIcon") },
              { ContentType.MainMenu, Resources.Load<Sprite>("UIResources/WorldIcon")},
              { ContentType.Scale, Resources.Load<Sprite>("UIResources/ScaleIcon")},
              { ContentType.Color, Resources.Load<Sprite>("UIResources/ColorMenuIcon")},
              { ContentType.Data, Resources.Load<Sprite>("UIResources/DataMenuIcon")},
              { ContentType.Querying, Resources.Load<Sprite>("UIResources/MaskIcon")},
              { ContentType.Settings, Resources.Load<Sprite>("UIResources/Settings_white")},
              { ContentType.Camera, Resources.Load<Sprite>("UIResources/Snapshot_white")},
              { ContentType.GlobalOptions, Resources.Load<Sprite>("UIResources/WorldIcon")},
              { ContentType.LoadFromDisk, Resources.Load<Sprite>("UIResources/PlusIcon")},
              { ContentType.FileBrowser, Resources.Load<Sprite>("UIResources/FolderIcon")},
              { ContentType.Playback, Resources.Load<Sprite>("UIResources/lightning-bolt-icon-white")},
              { ContentType.QuickLoad, Resources.Load<Sprite>("UIResources/lightning-bolt-icon-white")},
              { ContentType.Spectral, Resources.Load<Sprite>("UIResources/lightning-bolt-icon-white")},
              { ContentType.Controls, Resources.Load<Sprite>("UIResources/lightning-bolt-icon-white")},
              { ContentType.SaveLoad, Resources.Load<Sprite>("UIResources/lightning-bolt-icon-white")}};

        private static readonly GameObject tabPrefab = Resources.Load<GameObject>("Prefabs/UI/Tab");
        
        public static GameObject GetWindowPrefab(WindowType type)
        {
            return windowDict[type];
        }

        public static GameObject GetContentPrefab(ContentType type)
        {
            return contentDict[type];
        }

        public static Sprite GetContentIcon(ContentType type)
        {
            return iconDict[type];
        }

        public static GameObject GetTab()
        {
            return tabPrefab;
        }
    }
}
