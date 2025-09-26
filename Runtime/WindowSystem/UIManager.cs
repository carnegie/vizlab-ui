using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace windowsystem
{
    /// <summary>
    /// UI manager that centralizes options for the window system.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        #region Fields
        [SerializeField]
        private Transform playerTransform;

        [SerializeField]
        private Camera pickingCamera;
        public Camera PickingCamera
        {
            get { return pickingCamera; }
        }

        private Vector3 eventPos;
        public Vector3 EventPos
        {
            get { return eventPos; }
            set { eventPos = value; }
        }

        [SerializeField]
        private WandHitMarker wandPointer;
        public WandHitMarker WandPointer
        {
            get { return wandPointer; }
        }

        private float borderThickness = 35f;
        public float BorderThickness
        {
            get { return borderThickness; }
            set
            {
                borderThickness = value;
                UpdateBorderThickness();
            }
        }

        private bool lockToSurface;
        public bool LockToSurface
        {
            get { return lockToSurface; }
            set
            {
                lockToSurface = value;
            }
        }

        private bool preventMenuRotation;
        public bool PreventMenuRotation
        {
            get { return preventMenuRotation; }
            set
            {
                preventMenuRotation = value;
            }
        }

        private bool keepWithinCameraBounds;
        public bool KeepWithinCameraBounds
        {
            get { return keepWithinCameraBounds; }
            set
            {
                keepWithinCameraBounds = value;
            }
        }

        [SerializeField]
        private HandleFileBrowser fileBrowser;

        private WindowContents hoveredContents;

        private MainWindow hoveredWindow;

        private WindowContents draggedContents;

        private List<MainWindow> windows = new List<MainWindow>();

        private PlayerInputs playerInputs;

        [SerializeField]
        private GameObject basicsTutorial;

        [SerializeField]
        private GameObject dataAnalysisTutorial;

        #endregion

        #region Methods

        public override void Awake()
        {
            playerInputs = playerTransform.gameObject.GetComponent<PlayerInputs>();
            CreatePlayerMenu(ContentType.MainMenu);

            // retrieve config values for ui behavior

        }

        void Update()
        {
            if (playerInputs.AUp && draggedContents != null)
            {
                draggedContents.ContentReleaseEvent.Invoke(draggedContents);
                draggedContents = null;
            }
        }

        public MainWindow CreateEmptyWindow()
        {
            var windowObj = GameObject.Instantiate(UIContentManager.GetWindowPrefab(WindowType.Base), playerTransform) as GameObject;
            return windowObj.GetComponent<MainWindow>();
        }

        public void CreatePopup(PopupType type, WindowContents parentContent)
        {

        }

        private void UpdateBorderThickness()
        {
            // get all of the windows currently open and set their border thickness appropriately
            Debug.Log(windows.Count);
            foreach (var window in windows)
            {
                window.UpdateBorderThickness();
            }
        }

        public WindowContents CreateContentInWindow(ContentType contentType, WindowType windowType)
        {
            // if (contentType == ContentType.FileBrowser)
            // {
            //     fileBrowser.SetupFileBrowser();
            //     fileBrowser.ConnectFileBrowserInput();
            //     return null;
            // }

            // figure out best window placement
            (var pos, var rot) = ChooseWindowOrientation();

            // instantiate window and set placement
            var windowObj = GameObject.Instantiate(UIContentManager.GetWindowPrefab(windowType), playerTransform) as GameObject;
            windowObj.transform.position = pos;
            windowObj.transform.rotation = rot;
            var window = windowObj.GetComponent<MainWindow>();

            // instantiate content prefab
            var contentObj = GameObject.Instantiate(UIContentManager.GetContentPrefab(contentType), window.ContentTransform) as GameObject;
            var content = contentObj.GetComponent<WindowContents>();

            // set content
            content.Menu = contentObj.GetComponent<Menu>();

            // set tab
            var tabObj = GameObject.Instantiate(UIContentManager.GetTab()) as GameObject;
            var tab = tabObj.GetComponent<Tab>();
            tab.Name = contentType.ToString();
            tab.Icon = UIContentManager.GetContentIcon(contentType);
            content.Tab = tab;

            content.Type = contentType;

            // add content to window
            window.AddContents(content);

            window.UpdateBorderThickness();

            windows.Add(window);

            window.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(window.ContentTransform);

            return content;
        }

        public WindowContents CreatePlayerMenu(ContentType type)
        {
            // if menu exists, activate content and set 'alert' state
            var existingMenu = MenuExists(type);

            if (existingMenu != null)
            {
                existingMenu.ContentSelectEvent.Invoke(existingMenu);
                existingMenu.ContentAlert();
                return null;
            }

            // otherwise build menu 
            return CreateContentInWindow(type, WindowType.Base);
        }

        public WindowContents CreateObjectMenu(ContentType type, VizObject vizObj)
        {
            // if menu exists, activate content and set 'alert' state
            var existingMenu = MenuExists(type);

            if (existingMenu != null && existingMenu.gameObject.GetComponent<BaseMenuPresenter>().VizObj == vizObj)
            {
                existingMenu.ContentSelectEvent.Invoke(existingMenu);
                return null;
            }

            // otherwise build menu 
            var content = CreateContentInWindow(type, WindowType.Base);

            var vizPresenter = content.gameObject.GetComponent<BaseMenuPresenter>();
            vizPresenter.SetVizObject(vizObj);

            content.Tab.TabSelectedColor = vizObj.GetObjectColor();
            content.Tab.TabUnselectedColor = ColorOperations.DarkenColor(vizObj.GetObjectColor());

            return content;

        }

        public void DeleteAllObjectMenus(VizObject vizObj)
        {
            foreach (var window in windows)
            {
                // go through all window contents, check if contents has BaseMenuPresenter and if vizObj matches
                foreach (var content in window.Contents)
                {
                    var presenter = content.gameObject.GetComponent<BaseMenuPresenter>();
                    if (presenter != null)
                    {
                        if (presenter.VizObj == vizObj)
                        {
                            content.ContentRemove();
                        }
                    }
                }
            }
        }

        private (Vector3, Quaternion) ChooseWindowOrientation()
        {
            // if it's the first window just put it in the center of camera
            if (windows.Count == 0)
            {
                return (pickingCamera.transform.position + pickingCamera.transform.forward * 2.0f, pickingCamera.transform.rotation);
            }

            Transform lastActiveWindow = null;
            for (int i = windows.Count - 1; i > 0; i--)
            {
                if (windows[i].gameObject.activeSelf)
                {
                    lastActiveWindow = windows[i].gameObject.transform;
                    break;
                }
            }

            return lastActiveWindow == null ? (pickingCamera.transform.position + pickingCamera.transform.forward * 2.0f, pickingCamera.transform.rotation) : (lastActiveWindow.position - lastActiveWindow.right * 1.3f, lastActiveWindow.rotation);

            // // get world corners for all existing windows
            // List<Vector3[]> corners = new List<Vector3[]>();
            // foreach (var window in windows)
            // {
            //     corners.Add(window.WorldCorners);
            // }

            // return (Vector3.zero, Quaternion.identity);
        }

        private WindowContents MenuExists(ContentType type)
        {
            foreach (var window in windows)
            {
                foreach (var content in window.Contents)
                {
                    if (content.Type == type)
                    {
                        return content;
                    }
                }
            }

            return null;
        }

        public MainWindow GetContentWindow(ContentType type)
        {
            foreach (var window in windows)
            {
                foreach (var content in window.Contents)
                {
                    if (content.Type == type)
                    {
                        return window;
                    }
                }
            }

            return null;
        }

        public WindowContents GetDraggedContent()
        {
            if (draggedContents != null)
            {
                return draggedContents;
            }

            return null;
        }

        public void SetDraggedContent(WindowContents c)
        {
            if (draggedContents != c)
            {
                Debug.Log("SETTING DRAGGED CONTENT");
                draggedContents = c;
                c.Tab.SetRaycastTargets(false);
            }
        }

        public MainWindow GetHoveredWindow()
        {
            return hoveredWindow;
        }

        public void SetHoveredWindow(MainWindow window)
        {
            hoveredWindow = window;
        }

        public void DeleteWindow(MainWindow window)
        {
            windows.Remove(window);
            Destroy(window.gameObject);
        }

        public void MoveToNewWindow(WindowContents c)
        {
            // store old tab position
            var oldTabPos = c.Tab.gameObject.transform.position;
            var oldTabRot = c.Tab.gameObject.transform.rotation;
            Debug.Log(oldTabPos);

            // remove from old window 
            c.ContentRemoveEvent.Invoke(c);
            c.ContentReleaseEvent.Invoke(c);

            // create window 
            var windowObj = GameObject.Instantiate(UIContentManager.GetWindowPrefab(WindowType.Base), playerTransform) as GameObject;
            var window = windowObj.GetComponent<MainWindow>();

            // compute proper window dimensions
            window.WindowDimensions = c.Dimensions + window.Padding;

            // set window rotation to match tab
            window.gameObject.transform.rotation = c.Tab.TabTransform.rotation;
            window.AddContents(c);
            window.AlignWithTabPos(oldTabPos, oldTabRot);

            // set window pos to be aligned with old tab pos but a little forward as well
            // window.gameObject.transform.position = oldTabPos;
            // window.gameObject.transform.position -= window.gameObject.transform.forward*0.2f;
        }

        public void MoveToExistingWindow(WindowContents c, MainWindow w)
        {
            // clear links to old window
            c.ContentRemoveEvent.Invoke(c);

            // add to new window
            w.AddContents(c);

            // reset dragged state
            draggedContents.Tab.SetRaycastTargets(true);
            draggedContents = null;
        }

        public void StartBasicsTutorial()
        {
            var menu = MenuExists(ContentType.MainMenu);
            menu.ContentRemoveEvent.Invoke(menu);
            basicsTutorial.SetActive(true);
        }

        public void StartDataAnalysisTutorial()
        {
            var menu = MenuExists(ContentType.MainMenu);
            menu.ContentRemoveEvent.Invoke(menu);
            dataAnalysisTutorial.SetActive(true);
        }
    }
    #endregion
}