using HitMe.Audio;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HitMe.UI
{
    /// <summary>Plays one sample at the selected volume when the SFX slider is released.</summary>
    public sealed class SfxSliderPreview : MonoBehaviour, IPointerUpHandler
    {
        public void OnPointerUp(PointerEventData eventData)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayClick();
            else
                AudioService.Instance?.PlayClick();
        }
    }
}
