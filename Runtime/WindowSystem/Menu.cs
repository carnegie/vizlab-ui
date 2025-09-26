using UnityEngine;
using UnityEngine.UI;

namespace windowsystem
{
    /// <summary>
    /// Generic menu class - menu content should inherit from this!
    /// </summary>
    public class Menu : MonoBehaviour
    {
        [SerializeField]
        private RectTransform contentTransform;
        void Start()
        {
            if (contentTransform != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
            }
        }
        public void Reparent(Transform t)
        {
            gameObject.transform.SetParent(t, false);
        }
    }
}