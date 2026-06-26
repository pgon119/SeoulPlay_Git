using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.ElementalVFX
{
    public class BaseVfx : MonoBehaviour
    {
        [SerializeField] float _SafetyDestroy; //Destroy the object after a certan time in case user error keeps it active.
        [SerializeField] float _DestoyDelay; //Wait for effect to finish stopping before destroying the GameObject
        protected VfxData _data;
        public virtual void Play(VfxData data)
        {
            _data = data;
            CancelInvoke(nameof(Stop));
            StopAllCoroutines();

            var safetyDestroyDelay = _SafetyDestroy;
            if (_data.Duration > safetyDestroyDelay)
            {
                safetyDestroyDelay += _data.Duration;//Offset the safety destroy by the duration if bigger;
            }

            Destroy(gameObject, safetyDestroyDelay);
            Invoke(nameof(Stop), _data.Duration);
        }

        public virtual void Stop()
        {
            CancelInvoke(nameof(Stop));
            StopAllCoroutines();
            Destroy(gameObject, _DestoyDelay);
        }
    }
}
