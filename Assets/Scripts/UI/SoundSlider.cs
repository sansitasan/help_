using UnityEngine;
using UnityEngine.UI;

namespace SpaceBangBang
{
    public class SoundSlider : MonoBehaviour
    {
        [SerializeField]
        private Slider _slider;
        [SerializeField]
        private Sound type;

        public void DragEvent()
        {
            switch (type)
            {
                case Sound.Effect:
                    OptionPanel.EffectAction?.Invoke(_slider.value);
                    break;

                case Sound.Bgm:
                    OptionPanel.BgmAction?.Invoke(_slider.value);
                    break;
            }
        }
    }
}