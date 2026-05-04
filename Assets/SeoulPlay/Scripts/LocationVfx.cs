using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.ElementalVFX
{

    public class LocationVfx : BaseVfx
    {
        [SerializeField] VfxReference _LocationEffect;
        [SerializeField] float _RadiusFactor;
        [SerializeField] bool _IgnoreYDirection;
        public override void Play(VfxData data)
        {
            base.Play(data);
            if (_LocationEffect == null)
            {
                return;
            }

            _LocationEffect.transform.localScale = Vector3.one * _RadiusFactor * _data.Radius;
            _LocationEffect.transform.position = data.Source;
            Vector3 direction = _data.Target - _data.Source;
            if (_IgnoreYDirection)
            {
                direction.y = 0;
            }

            if (direction.sqrMagnitude > 0.001f)
            {
                _LocationEffect.transform.forward = direction.normalized;
            }

            _LocationEffect.gameObject.SetActive(true);
            _LocationEffect.Play();
        }

        public override void Stop()
        {
            base.Stop();
            if (_LocationEffect != null)
            {
                _LocationEffect.Stop();
            }
        }
    }
}
