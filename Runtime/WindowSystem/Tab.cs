using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace windowsystem
{
    /// <summary>
    /// Tab class that largely takes in/responds to input from the user and propagates it further up the UI system.
    /// Interfaces with WindowContents/MainWindow/TabContainer to make the tab system work.
    /// Implements IPointerDown/UpHandler and IDragHandler to facilitate tab dragging when in a TabContainer.
    /// </summary>
    public class Tab : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        #region Fields and properties
        public string Name 
        {
            get { return tabText.text; }
            set { tabText.text = value; }
        }

        public Sprite Icon
        {
            set { tabIcon.sprite = value; }
        }

        private bool isActive = false;

        private MaskableGraphic tabGraphic;

        [SerializeField]
        private Text tabText;

        [SerializeField]
        private Image tabIcon;

        [SerializeField]
        private Color tabSelectedColor;
        public Color TabSelectedColor 
        {
            get { return tabSelectedColor; }
            set
            { 
                tabSelectedColor = value; 
                Select();
            }
        }

        [SerializeField]
        private Color tabUnselectedColor;
        public Color TabUnselectedColor 
        {
            get { return tabUnselectedColor; }
            set 
            { 
                tabUnselectedColor = value;
            }
        }

        private RectTransform tabTransform;
        public RectTransform TabTransform 
        {
            get { return tabTransform; }
        }

        #endregion
        #region Events

        private UnityEvent tabSelectEvent = new UnityEvent();
        public UnityEvent TabSelectEvent 
        {
            get { return tabSelectEvent; }
        }

        private UnityEvent tabRemoveEvent = new UnityEvent();
        public UnityEvent TabRemoveEvent 
        {
            get { return tabRemoveEvent; }
        }

        private UnityEvent tabPickupEvent = new UnityEvent();
        public UnityEvent TabPickupEvent
        {
            get { return tabPickupEvent; }
        }

        private UnityEvent tabReleaseEvent = new UnityEvent();
        public UnityEvent TabReleaseEvent
        {
            get { return tabReleaseEvent; }
        }

        private UnityEvent tabDragEvent = new UnityEvent();
        public UnityEvent TabDragEvent 
        {
            get { return tabDragEvent; }
        }

        #endregion
        #region Methods

        void Awake()
        {
            tabGraphic = GetComponent<Image>();
            tabTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Select current tab and propagate the selection up the UI system through events.
        /// Trigger OnClick of tab's Button component.
        /// </summary>
        public void Select() 
        {
            tabSelectEvent.Invoke();
            Activate();
        }

        /// <summary>
        /// Sets tab-specific state on selection (swapping from button mode to grabbable/draggable mode).
        /// </summary>
        public void Activate()
        {
            isActive = true;

            // disable button component, add wand grabbable component
            gameObject.GetComponent<WandGrabbable>().enabled = true;
            gameObject.GetComponent<Button>().enabled = false;

            tabGraphic.color = tabSelectedColor;
        }

        /// <summary>
        /// Sets tab-specific state on unselection (going back to button mode).
        /// </summary>
        public void Unselect()
        {
            isActive = false;

            gameObject.GetComponent<WandGrabbable>().enabled = false;
            gameObject.GetComponent<Button>().enabled = true;

            tabGraphic.color = tabUnselectedColor;
        }

        /// <summary>
        /// Trigger broader WindowContents deletion higher up the UI system through an event.
        /// </summary>
        public void Delete()
        {
            tabRemoveEvent.Invoke();
        }

        /// <summary>
        /// Enable or disable all raycastable parts of the tab.
        /// </summary>
        /// <param name="state">True or false depending on whether you're enabling or disabling raycasts.</param>
        public void SetRaycastTargets(bool state)
        {
            tabGraphic.raycastTarget = state;
            tabText.raycastTarget = state;
            tabIcon.raycastTarget = state;
        }

        /// <summary>
        /// Runs when the pointer goes down to trigger a tab pickup higher up in the UI system.
        /// Implementation of IPointerDownHandler from UnityEngine.EventSystems.
        /// </summary>
        /// <param name="eventData">Data from UI pointer.</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            // trigger group manager's OnPanelPickup callback
            if (isActive)
            {
                tabPickupEvent.Invoke();
            }
        }

        /// <summary>
        /// Runs when the pointer goes up to trigger a tab release higher up in the UI system.
        /// Implementation of IPointerUpHandler from UnityEngine.EventSystems.
        /// </summary>
        /// <param name="eventData">Data from UI pointer.</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (isActive)
            {
                tabReleaseEvent.Invoke();
            }
        }

        /// <summary>
        /// Runs when the tab gets dragged so tab drag management in TabContainer works.
        /// Implementation of IDragHandler from UnityEngine.EventSystems.
        /// </summary>
        /// <param name="eventData">Data from UI pointer.</param>
        public void OnDrag(PointerEventData eventData)
        {
            if (isActive)
            {
                // Vector2 pos;
                // RectTransformUtility.ScreenPointToLocalPointInRectangle(tabTransform, eventData.position, eventData.pressEventCamera, out pos);
                UIManager.Instance.EventPos = eventData.position;
                // Debug.Log(pos);
                tabDragEvent.Invoke();
            }
        }

        #endregion
    }
}