using UnityEngine;
using DG.Tweening;


public enum RotateType
{ RotateTo, RotateBy }

namespace Aezakmi.Tweens
{
    public class Rotate : TweenBase
    {
        [Header("Rotate Tween Settings")]
        [SerializeField] private RotateMode _rotateMode;
        [SerializeField] private RotateType _rotateType;

   
        [SerializeField] private Vector3 _rotation;
   
        [SerializeField] private Vector3 _amount;

        private Vector3 _targetRotation;
        private bool _isRotateTo { get { return _rotateType == RotateType.RotateTo; } }
        private bool _isRotateBy { get { return _rotateType == RotateType.RotateBy; } }

        protected override void SetTweener()
        {
            SetTargetRotation();

            Tweener = transform
                .DOLocalRotate(_targetRotation, LoopDuration, _rotateMode)
                .SetLoops(LoopCount, LoopType)
                .SetEase(LoopEase)
                .SetDelay(Delay);
        }

        private void SetTargetRotation() => _targetRotation = _isRotateTo ? _rotation : _amount;
    }
}