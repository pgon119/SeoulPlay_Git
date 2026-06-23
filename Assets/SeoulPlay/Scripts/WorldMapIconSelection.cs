using UnityEngine;
using UnityEngine.EventSystems;

namespace SeoulPlay
{
    [DisallowMultipleComponent]
    public sealed class WorldMapIconSelection : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private GameObject selectionImage;

        public void Configure(GameObject image)
        {
            selectionImage = image;
            SetSelected(false);
        }

        public void OnSelect(BaseEventData eventData)
        {
            SetSelected(true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            SetSelected(false);
        }

        private void SetSelected(bool selected)
        {
            if (selectionImage != null)
            {
                selectionImage.SetActive(selected);
            }
        }
    }
}
