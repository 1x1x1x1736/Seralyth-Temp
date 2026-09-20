using UnityEngine;
using static SeralythTemp.Menu.Main;
using static SeralythTemp.Settings;

namespace SeralythTemp.Classes
{
	public class Button : MonoBehaviour
	{
		public string relatedText;

		public bool incremental;
		public bool positive;

		public static float buttonCooldown = 0f;
		
		public void OnTriggerEnter(Collider collider)
		{
			if (Time.time > buttonCooldown && collider == buttonCollider && menu != null)
			{
                buttonCooldown = Time.time + 0.2f;
                GorillaTagger.Instance.StartVibration(rightHanded, GorillaTagger.Instance.tagHapticStrength / 2f, GorillaTagger.Instance.tagHapticDuration / 2f);
                VRRig.LocalRig.PlayHandTapLocal(ButtonSound, rightHanded, 0.4f);

                if (incremental)
                    ToggleIncremental(this.relatedText, positive);
                else
                    Toggle(this.relatedText);
            }
		}
	}
}
