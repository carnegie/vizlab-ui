using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace windowsystem 
{
    /// <summary>
    /// Tab container class that handles reorganizing, resizing, and cycling for a set of tabs in a MainWindow.
    /// Inheirits from UIBehaviour to let us override OnRectTransformDimensionsChange (making resizing much easier).
    /// </summary>
    public class TabContainer : UIBehaviour
    {
        #region Fields
        [SerializeField]
        private RectTransform rt;

        [SerializeField]
        private OptionCycler tabCycler;

        [SerializeField]
        private float tabMinSize = 50.0f;

        [SerializeField]
        private float tabSpacing = 5.0f;

        [SerializeField]
        private GameObject freeSpacePrefab;
        private GameObject freeSpace = null;

        private List<Tab> tabs = new List<Tab>();

        #endregion
        #region Methods

        /// <summary>
        /// Add tab to the TabContainer and set its state accordingly.
        /// </summary>
        /// <param name="contents">WindowContents (with tab at contents.Tab).</param>
        public void AddTab(WindowContents contents)
        {
            // add tab to internal list and set its transform accordingly
            tabs.Add(contents.Tab);
            contents.Tab.gameObject.transform.SetParent(rt.transform, false);

            // clear any event listeners it had from previous windows/containers and add this one's
            ResetContentListeners(contents);
            AddContentListeners(contents);

            // check for container resizing
            CheckTabSizes();
        }

        /// <summary>
        /// Remove tab from TabContainer and set its state accordingly.
        /// </summary>
        /// <param name="contents">WindowContents (with tab at contents.Tab).</param>
        public void RemoveTab(WindowContents contents)
        {
            tabs.Remove(contents.Tab);
            ResetContentListeners(contents);

            CheckTabSizes();
        }

        /// <summary>
        /// Clear listeners from given tab's pickup, release, and drag events.
        /// </summary>
        /// <param name="contents">WindowContents (with tab at contents.Tab).</param>
        private void ResetContentListeners(WindowContents contents)
        {
            contents.ContentPickupEvent.RemoveAllListeners();
            contents.ContentReleaseEvent.RemoveAllListeners();
            contents.ContentDragEvent.RemoveAllListeners();
        }

        /// <summary>
        /// Add listeners from given tab's pickup, release, and drag events.
        /// </summary>
        /// <param name="contents">WindowContents (with tab at contents.Tab).</param>
        private void AddContentListeners(WindowContents contents)
        {
            contents.ContentPickupEvent.AddListener(OnTabPickup);
            contents.ContentReleaseEvent.AddListener(OnTabRelease);
            contents.ContentDragEvent.AddListener(OnTabDrag);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contents">WindowContents (with tab at contents.Tab).</param>
        private void OnTabPickup(WindowContents contents)
        { 
            // get index of picked tab
            var pickedTabIndex = contents.Tab.gameObject.transform.GetSiblingIndex();

            // move tab out of container transform
            contents.Tab.gameObject.transform.SetParent(rt.transform.parent, true);

            // create appropriately sized free space, replace tab with it
            freeSpace = GameObject.Instantiate(freeSpacePrefab, rt.transform) as GameObject;
            var freeSpaceRect = freeSpace.gameObject.GetComponent<RectTransform>();
            freeSpaceRect.GetComponent<LayoutElement>().minWidth = contents.Tab.TabTransform.rect.width;
            freeSpace.transform.SetSiblingIndex(pickedTabIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contents">WindowContents (with tab at contents.Tab).</param>
        private void OnTabRelease(WindowContents contents)
        {
            // remove free space 
            var freeSpaceIndex = freeSpace.transform.GetSiblingIndex();
            Destroy(freeSpace);

            // move tab back where free space was
            contents.Tab.gameObject.transform.SetParent(rt);
            contents.Tab.gameObject.transform.SetSiblingIndex(freeSpaceIndex);
            
            CheckTabSizes();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contents">WindowContents (with tab at contents.Tab).</param>
        private void OnTabDrag(WindowContents contents)
        {
            bool inContainer = RectTransformUtility.RectangleContainsScreenPoint(rt, contents.Tab.gameObject.transform.position);
            if (inContainer) // if tab within container, update free space if dragged tab has moved enough
            {
                Vector2 pos;
                bool inRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, UIManager.Instance.EventPos, UIManager.Instance.PickingCamera, out pos);
                if (inRect && freeSpace != null)
                {
                    // if (Mathf.Abs(pos.x - freeSpace.transform.localPosition.x) > contents.Tab.TabTransform.rect.width)
                    // {
                        // compute new index for free space amd update
                        var containerWidth = rt.rect.width*0.5f;
                        contents.Tab.transform.localPosition = new Vector2(pos.x, contents.Tab.transform.localPosition.y);
                        var index = (int)Mathf.Floor((contents.Tab.gameObject.transform.localPosition.x + (rt.rect.width*0.5f)) / (contents.Tab.TabTransform.rect.width + 5.0f));

                        freeSpace.transform.SetSiblingIndex(index+GetFirstActiveChildIndex(rt));
                    // }
                }
            }
            else // if it leaves, remove from container, trigger moving that content into a new window
            {
                Destroy(freeSpace);
                RemoveTab(contents);

                UIManager.Instance.MoveToNewWindow(contents);
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            CheckTabSizes();
        }

        private void CheckTabSizes()
        {
            // if present tabs exceed container length, set up tab cycler
            if (GetTabsTotalLength() > rt.rect.width)
            {
                tabCycler.enabled = true;
                SetCyclerDetails();
            } 
            else if (tabCycler.enabled)
            {
                ShowAllTabs();
                tabCycler.enabled = false;
            }
        }

        private void SetCyclerDetails()
        {
            tabCycler.numObjectsVisible = GetLastContainedTabIndex()+1;
            tabCycler.options = GetTabGameObjects();
            tabCycler.CyclerInit();
        }

        private List<GameObject> GetTabGameObjects()
        {
            List<GameObject> gameObjs = new List<GameObject>();
            foreach (var tab in tabs)
            {
                gameObjs.Add(tab.gameObject);
            }
            return gameObjs;
        }

        private void ShowAllTabs()
        {
            foreach (var tab in tabs)
            {
                tab.gameObject.SetActive(true);
            }
        }

        private float GetTabsTotalLength()
        {
            return ((tabMinSize + tabSpacing) * tabs.Count) - tabSpacing;
        }

        private int GetLastContainedTabIndex()
        {
            var currentTabsLength = 0.0f;
            var index = -1;
            while (currentTabsLength < rt.rect.width)
            {
                index++;
                currentTabsLength += (tabMinSize + tabSpacing);
            }
            return index-1;
        }

        private int GetFirstActiveChildIndex(Transform baseTransform)
        {
            for (int i = 0; i < baseTransform.childCount; i++)
            {
                if (baseTransform.GetChild(i).gameObject.activeSelf)
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion
    }
}