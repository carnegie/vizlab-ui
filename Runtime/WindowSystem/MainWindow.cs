using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace windowsystem 
{
    /// <summary>
    /// Window class for containing a variable number of UI menus/WindowContents. 
    /// Sub-class of BaseWindow. Implements IPointerEnter/ExitHandler to handle menu drag/hover state.
    /// </summary>
    public class MainWindow : BaseWindow, IPointerEnterHandler, IPointerExitHandler
    {
        #region Fields and properties

        [SerializeField]
        private VolumeMenuDrag drag;

        [SerializeField]
        private TabContainer tabContainer;

        [SerializeField]
        private GameObject hoverObj;

        private List<WindowContents> contents = new List<WindowContents>();
        public List<WindowContents> Contents
        {
            get { return contents; }
        }

        public bool Disabled
        {
            set
            {
                WindowTransform.Find("DeleteWindow").GetComponent<Button>().interactable = !value;
            }
        }

        private Vector3 FirstTabToWindowDisp
        {
            get
            {
                return new Vector3(-0.2525f, -0.95f, 0.0f);
            }
        }

        private WindowContents activeContent = null;
        private bool inAlertMode = false;

        #endregion
        #region Methods

        void Start()
        {
            foreach (Transform child in ContentTransform)
            {
                // this catches window contents that exist in the scene initially
                var menu = child.gameObject.GetComponent<WindowContents>();
                if (menu != null && !contents.Contains(menu))
                {
                    AddContents(menu);
                }
            }
        }

        /// <summary>
        /// Add WindowContents to internal list and set state depending on amount of content in window.
        /// </summary>
        /// <param name="c">WindowContents we're currently managing.</param>
        public void AddContents(WindowContents c)
        {
            // update first content if we're going 1 -> 2 tabs
            if (contents.Count == 1)
            {
                SetMultiTabState(contents[0]);
            }

            // add to internal list
            contents.Add(c);

            // set content position
            c.Menu.Reparent(ContentTransform);

            // set state for current content
            if (contents.Count == 1)
            {
                SetSingleTabState(c);
            }
            else
            {
                SetMultiTabState(c);
            }

            // add tab listeners 
            c.ContentSelectEvent.RemoveAllListeners();
            c.ContentSelectEvent.AddListener(SetActiveContent);
            c.ContentRemoveEvent.RemoveAllListeners();
            c.ContentRemoveEvent.AddListener(RemoveContents);
            c.ContentAlertEvent.RemoveAllListeners();
            c.ContentAlertEvent.AddListener(SetAlertState);

            SetActiveContent(c);
            hoverObj.SetActive(false);
        }

        public void AlignWithTabPos(Vector3 pos, Quaternion rot)
        {
            gameObject.transform.rotation = rot;

            gameObject.transform.position = pos + rot * FirstTabToWindowDisp;
        }

        /// <summary>
        /// Set necessary state for WindowContents that is the only content in its window.
        /// Ignores TabContainer and allows for tab menu dragging.
        /// </summary>
        /// <param name="contents">WindowContents to set state in.</param>
        private void SetSingleTabState(WindowContents contents)
        {
            contents.Tab.gameObject.transform.SetParent(tabContainer.gameObject.transform, false);
            // add tab to drag
            drag.m_draggable.Add(contents.Tab.gameObject);

            // set tab pickup event to link with UIManager
            contents.ContentPickupEvent.AddListener(SetTabDrag);
            contents.ContentReleaseEvent.AddListener(SetTabRelease);
        }

        /// <summary>
        /// Set necessary state for WindowContents where other content exists in the window.
        /// Add tab to TabContainer, which allows it to be rearranged and dragged.
        /// </summary>
        /// <param name="contents">WindowContents to set state in.</param>
        private void SetMultiTabState(WindowContents contents)
        {
            // clear single tab state if it was set
            if (drag.m_draggable.Contains(contents.Tab.gameObject))
            {
                drag.m_draggable.Remove(contents.Tab.gameObject);
            }

            // add to TabContainer
            tabContainer.AddTab(contents);
        }

        /// <summary>
        /// Trigger a visual alert state for the window containing desired contents.
        /// </summary>
        /// <param name="contents">WindowContents we're alerting with.</param>
        private void SetAlertState(WindowContents contents)
        {
            // set contents to be active if we haven't done so already
            if (activeContent != contents)
            {
                SetActiveContent(contents);
            }

            // trigger color flash coroutine
            inAlertMode = true;
            StartCoroutine(AlertFlash());
        }

        /// <summary>
        /// Coroutine for setting alert state on menus.
        /// Exits once menu has been 'acknowledged' (currently OnPointerEnter).
        /// </summary>
        /// <returns></returns>
        private IEnumerator AlertFlash()
        {
            var normalBorderColor = BorderColor;
            var normalTabColor = activeContent.Tab.TabSelectedColor;
            while (inAlertMode)
            {
                yield return new WaitForSeconds(0.3f);
                // change color of window and tab to alert color
                BorderColor = Color.green;
                activeContent.Tab.TabSelectedColor = Color.green;

                yield return new WaitForSeconds(0.3f);
                // change color of window and tab to normal color
                BorderColor = normalBorderColor;
                activeContent.Tab.TabSelectedColor = normalTabColor;
            }
        }

        /// <summary>
        /// Remove content from the current window and adjust its state accordingly.
        /// </summary>
        /// <param name="c">WindowContents we're removing from the window.</param>
        public void RemoveContents(WindowContents c)
        {
            // remove from internal list
            contents.Remove(c);

            // delete the window if it has no more content
            if (contents.Count == 0)
            {
                UIManager.Instance.DeleteWindow(this);
            }

            // adjust content state if we're going from 2 -> 1 
            if (contents.Count == 1)
            {
                tabContainer.RemoveTab(contents[0]);
                SetSingleTabState(contents[0]);
            }

            // make first content in window active if we're removing active content
            if (activeContent == c && contents.Count > 0)
            {
                SetActiveContent(contents[0]);
                // TODO: Make this more sophisticated!
            }
        }

        /// <summary>
        /// Delete current window. Public method wrapper to UIManager so Delete button on menu can access it easily.
        /// </summary>
        public void DeleteWindow()
        {
            UIManager.Instance.DeleteWindow(this);
        }

        /// <summary>
        /// Set the content that's currently visible in this window.
        /// </summary>
        /// <param name="c">WindowContents to set active.</param>
        private void SetActiveContent(WindowContents c)
        {
            if (activeContent != null)
            {
                activeContent.Unselect();
            }

            activeContent = c;
            BorderColor = c.Tab.TabSelectedColor;
            activeContent.Select();
        }

        /// <summary>
        /// Called on the window containing single-tab content that is being dragged (dragging the window with it).
        /// </summary>
        /// <param name="c">WindowContents that is currently picked up.</param>
        private void SetTabDrag(WindowContents c)
        {
            if (UIManager.Instance.GetDraggedContent() != c)
            {
                UIManager.Instance.SetDraggedContent(c);
            }
        }

        /// <summary>
        /// Called on the window containing content that is to be moved to another window (as part of a tab drag operation).
        /// </summary>
        /// <param name="c">WindowContents to be moved and then removed from the current window.</param>
        public void SetTabRelease(WindowContents c)
        {
            if (UIManager.Instance.GetDraggedContent() != null && UIManager.Instance.GetHoveredWindow() != null)
            {
                UIManager.Instance.MoveToExistingWindow(UIManager.Instance.GetDraggedContent(), UIManager.Instance.GetHoveredWindow());
                RemoveContents(c);
            }
        }


        /// <summary>
        /// Runs when the UI pointer enters the window canvas to manage hover state.
        /// Implementation of IPointerEnterHandler from UnityEngine.EventSystems.
        /// </summary>
        /// <param name="eventData">Data from UI pointer.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            inAlertMode = false;
            if (UIManager.Instance.GetDraggedContent() != null && !contents.Contains(UIManager.Instance.GetDraggedContent()))
            {
                UIManager.Instance.SetHoveredWindow(this);
                hoverObj.SetActive(true);
            }
        }

        /// <summary>
        /// Runs when the UI pointer exits the window canvas to manage hover state.
        /// Implementation of IPointerExitHandler from UnityEngine.EventSystems.
        /// </summary>
        /// <param name="eventData">Data from UI pointer.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (UIManager.Instance.GetDraggedContent() != null)
            {
                UIManager.Instance.SetHoveredWindow(null);
                hoverObj.SetActive(false);
            }
        }

        /// <summary>
        /// Toggle the pinned state of the window (pinned windows cannot be moved or resized).
        /// </summary>
        public void PinWindow()
        {

            IsPinned = !IsPinned;
            var button = transform.Find("PinButton").GetComponent<Image>();
            var icon = transform.Find("PinButton/Icon").GetComponent<Image>();
            if (IsPinned)
            {
                button.color = Color.green;
                icon.color = Color.green;
            }
            else
            {
                button.color = Color.white;
                icon.color = Color.white;
            }

        }

        #endregion
    }
}