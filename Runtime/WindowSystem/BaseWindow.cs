using UnityEngine;
using Nobi.UiRoundedCorners;
using System.Collections.Generic;

namespace windowsystem
{
    /// <summary>
    /// Parent window class which both MainWindows and PopupWindows inherit from.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class BaseWindow : MonoBehaviour
    {
        [SerializeField]
        private ImageWithRoundedBorderAndInterior windowImg;

        [SerializeField]
        private VolumeMenuDrag drag;

        [SerializeField]
        private RectTransform contentTransform;
        public RectTransform ContentTransform
        {
            get { return contentTransform; }
        }

        [SerializeField]
        private RectTransform windowTransform;
        public RectTransform WindowTransform
        {
            get { return windowTransform; }
        }

        void Awake()
        {
            if (windowTransform == null)
            {
                windowTransform = GetComponent<RectTransform>();
            }
        }

        /// <summary>
        /// Controls the interior color of the window through the image on the Canvas.
        /// </summary>
        public Color InteriorColor
        {
            get { return windowImg.InteriorColor; }
            set { windowImg.InteriorColor = value; }
        }

        /// <summary>
        /// Controls the border color of the window through the image on the Canvas.
        /// </summary>
        public Color BorderColor
        {
            get { return windowImg.BorderColor; }
            set { windowImg.BorderColor = value; }
        }

        /// <summary>
        /// Get and set the total dimensions of the window using the associated BaseWindow RectTransform.
        /// </summary>
        public Vector2 WindowDimensions
        {
            set { windowTransform.sizeDelta = value; }
            get { return new Vector2(windowTransform.rect.width, windowTransform.rect.height); }
        }

        /// <summary>
        /// Get the dimensions of the window's content specifically (excluding the window border and padding, if present).
        /// </summary>
        public Vector2 ContentDimensions
        {
            get { return new Vector2(contentTransform.rect.width, contentTransform.rect.height); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 Padding
        {
            get
            {
                var width = windowTransform.rect.width - contentTransform.rect.width;
                var height = windowTransform.rect.height - contentTransform.rect.height;
                return new Vector2(width, height);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector3[] WorldCorners
        {
            get
            {
                Vector3[] v = new Vector3[4];
                windowTransform.GetWorldCorners(v);
                return v;
            }
        }

        private bool isPinned = false;
        public bool IsPinned
        {
            get { return isPinned; }
            set { isPinned = value; }
        }

        void Update()
        {
            if (UIManager.Instance.PreventMenuRotation)
            {

                var direction = WindowTransform.transform.position - Camera.main.gameObject.transform.position;
                WindowTransform.transform.rotation = Quaternion.LookRotation(direction);
            }
            if (UIManager.Instance.KeepWithinCameraBounds)
            {
                if (!IsWindowWithinCameraBounds())
                {
                    ResetPosition();
                }
            }
        }

        public bool IsWindowWithinCameraBounds()
        {
            var vpPos = Camera.main.WorldToViewportPoint(WindowTransform.position);
            if (vpPos.x > (1f) || vpPos.x < 0f || vpPos.y > (1f) || vpPos.y < 0f)
            {
                return false;
            }
            return true;
        }

        private void ResetPosition()
        {
            WindowTransform.transform.position = UIManager.Instance.PickingCamera.transform.position + UIManager.Instance.PickingCamera.transform.forward * 2.0f;
            WindowTransform.transform.rotation = UIManager.Instance.PickingCamera.transform.rotation;
        }

        public void UpdateBorderThickness()
        {
            var thickness = UIManager.Instance.BorderThickness;

            windowImg.borderWidth = thickness;
            windowImg.enabled = false;
            windowImg.enabled = true;

            SetGrabRegion(drag.m_draggable[0], -0.5f * thickness, 0f, thickness, -thickness - (35f * 2));
            SetGrabRegion(drag.m_draggable[1], 0.5f * thickness, 0f, thickness, -thickness);
            SetGrabRegion(drag.m_draggable[2], 0f, 0.5f * thickness, -thickness, thickness);
            SetGrabRegion(drag.m_draggable[3], 0f, -0.5f * thickness - 22f, -thickness - (65f * 2f), thickness);
            WindowTransform.ForceUpdateRectTransforms();

            ContentTransform.GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(thickness, thickness);
            ContentTransform.GetChild(0).GetComponent<RectTransform>().offsetMax = new Vector2(-thickness, -thickness);
        }

        private void SetGrabRegion(GameObject obj, float xPos, float yPos, float width, float height)
        {
            var t = obj.GetComponent<RectTransform>();
            t.anchoredPosition = new Vector2(xPos, yPos);
            t.sizeDelta = new Vector2(width, height);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Close()
        {
            Destroy(gameObject);
        }

        public virtual void AlignWith(RectTransform rtToMatch, Alignment a, float spacing)
        {
            // set matching rotation 
            transform.rotation = rtToMatch.transform.rotation;

            // get world corners of rt 

            // use size of window to 
        }
    }
}