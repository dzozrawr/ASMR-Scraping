using System;
using Lofelt.NiceVibrations;
using UnityEngine;
using UnityEngine.UI;

namespace CrazyLabsExtension
{
    [RequireComponent( typeof( Button ) )]
    public class HapticFeedbackUIButton : MonoBehaviour
    {
        public static event Action<HapticPatterns.PresetType> OnHapticButtonPressed = delegate{ };

        private Button _button;

        [SerializeField]
        private HapticPatterns.PresetType _hapticType = HapticPatterns.PresetType.Selection;

        private void Awake()
        {
            _button = GetComponent<Button>( );

            _button.onClick.AddListener( OnButtonClicked );
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener( OnButtonClicked );
        }

        private void OnButtonClicked()
        {
            OnHapticButtonPressed.Invoke( _hapticType );
        }
    }
}