using UnityEngine;
using DG.Tweening;

public enum MoveType
{ MoveTo, MoveBy }

namespace Aezakmi.Tweens
{
    public class Move : TweenBase
    {
        [Header("Move Tween Settings")]
        [SerializeField] private MoveType _moveType;


        [SerializeField] private Vector3 _position;
   
        [SerializeField] private Vector3 _amount;

        private Vector3 _targetPosition;
        private bool _isMovingTo { get { return _moveType == MoveType.MoveTo; } }
        private bool _isMovingBy { get { return _moveType == MoveType.MoveBy; } }

        protected override void SetTweener()
        {
            SetTargetPosition();

            Tweener = transform
                .DOLocalMove(_targetPosition, LoopDuration)
                .SetLoops(LoopCount, LoopType)
                .SetEase(LoopEase)
                .SetDelay(Delay)
                .SetRelative(_isMovingBy);
        }

        private void SetTargetPosition() => _targetPosition = _isMovingTo ? _position : _amount;
    }
}
