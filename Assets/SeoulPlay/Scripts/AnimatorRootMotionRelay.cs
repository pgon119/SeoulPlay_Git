using UnityEngine;

namespace SeoulPlay
{
    [RequireComponent(typeof(Animator))]
    public sealed class AnimatorRootMotionRelay : MonoBehaviour
    {
        [SerializeField] private SimpleHeroMover mover;
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            if (mover == null)
            {
                mover = GetComponentInParent<SimpleHeroMover>();
            }
        }

        private void OnAnimatorMove()
        {
            if (animator != null && mover != null)
            {
                mover.ApplyAnimatorRootMotion(animator.deltaPosition, animator.deltaRotation);
            }
        }
    }
}
