using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace windowsystem 
{
    [System.Serializable]
    public class WindowContentsEvent : UnityEvent<WindowContents> { }

    /// <summary>
    /// Class that encapsulates menu content and tab and coordinates with them via events.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class WindowContents : MonoBehaviour 
    {
        #region Fields and properties

        [SerializeField]
        private Tab tab;
        public Tab Tab 
        {
            get { return tab; }
            set 
            {
                tab = value;
                SetContentListeners();
            }
        }

        [SerializeField]
        private Menu menu;
        public Menu Menu
        {
            get { return menu; }
            set{ menu = value; }
        }

        private ContentType type;
        public ContentType Type 
        {
            get { return type; }
            set { type = value; }
        }

        private RectTransform contentTransform;
        public Vector2 Dimensions
        {
            get { return new Vector2(contentTransform.rect.width + 70.0f, contentTransform.rect.height + 70.0f); }
        }
        
        private List<BaseWindow> popups;

        #endregion
        #region Events

        private WindowContentsEvent contentSelectEvent = new WindowContentsEvent();
        public WindowContentsEvent ContentSelectEvent 
        {
            get { return contentSelectEvent; }
        }

        private WindowContentsEvent contentPickupEvent = new WindowContentsEvent();
        public WindowContentsEvent ContentPickupEvent
        {
            get { return contentPickupEvent; }
        }

        private WindowContentsEvent contentReleaseEvent = new WindowContentsEvent();
        public WindowContentsEvent ContentReleaseEvent
        {
            get { return contentReleaseEvent; }
        }

        private WindowContentsEvent contentDragEvent = new WindowContentsEvent();
        public WindowContentsEvent ContentDragEvent 
        {
            get { return contentDragEvent; }
        }
        private WindowContentsEvent contentRemoveEvent = new WindowContentsEvent();
        public WindowContentsEvent ContentRemoveEvent
        {
            get { return contentRemoveEvent; }
        }
        private WindowContentsEvent contentAlertEvent = new WindowContentsEvent();
        public WindowContentsEvent ContentAlertEvent 
        {
            get { return contentAlertEvent; }
        }

        #endregion
        #region Methods

        void Awake()
        {
            contentTransform = GetComponent<RectTransform>();
            if (tab != null)
            {
                SetContentListeners();
            }
        }

        private void SetContentListeners()
        {
            tab.TabPickupEvent.AddListener(ContentPickup);
            tab.TabReleaseEvent.AddListener(ContentRelease);
            tab.TabDragEvent.AddListener(ContentDrag);
            tab.TabSelectEvent.AddListener(ContentSelect);
            tab.TabRemoveEvent.AddListener(ContentRemove);
        }

        public void ContentPickup() { contentPickupEvent.Invoke(this); }
        public void ContentRelease() { contentReleaseEvent.Invoke(this); }
        public void ContentDrag() { contentDragEvent.Invoke(this); }
        public void ContentSelect() { contentSelectEvent.Invoke(this); }
        public void ContentAlert() { contentAlertEvent.Invoke(this); }

        public void ContentRemove() 
        { 
            contentRemoveEvent.Invoke(this); 
            Destroy(this.gameObject);
        }

        public void Select()
        {
            // show menu content and activate tab
            menu.gameObject.SetActive(true);
            tab.Activate();

            // NOTE: (don't call tab.Select() here because that's where
            // Select propagates from originally with a button click!)
        }

        public void Unselect()
        {
            // hide menu content
            menu.gameObject.SetActive(false);

            // unselect tab
            tab.Unselect();
        }

        /// <summary>
        /// Make sure associated tab and menu are destroyed when WindowContents is destroyed.
        /// </summary>
        void OnDestroy()
        {
            Destroy(menu.gameObject);
            if (tab != null)
            {
                Destroy(tab.gameObject);
            } 
        }
        #endregion
    }
}
