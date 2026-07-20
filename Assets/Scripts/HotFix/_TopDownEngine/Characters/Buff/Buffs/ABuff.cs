using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    public class ABuff : Buff
    {
        public DamageOnTouch DamageOnTouch;

        public override void OnNew()
        {
            DamageOnTouch = GetComponent<DamageOnTouch>();
            DamageOnTouch.SetEnabled(false);
        }

        protected override void OnInstant()
        {
            Debug.Log($"OnInstant at{Time.frameCount}");

            SetParent(null);
            transform.position = Target.transform.position;
            var ad = Source.GetStat("AD");
            DamageOnTouch.AddIgnore(Target.gameObject);
            DamageOnTouch.SetDmg(Dmg.AD((int)ad.Value));
            DamageOnTouch.SetEnabled(true);
        }

        protected override void OnDiscard()
        {
            Debug.Log($"OnDiscard at{Time.frameCount}");
        }

        public override void OnRelease()
        {
            Clear();

            Timing.RunCoroutine(delay());
        }

        IEnumerator<float> delay()
        {
            yield return Timing.WaitForSeconds(0.1F);
            gameObject.SetActive(false);
            SetParent(DefaultParent);
            DamageOnTouch.SetEnabled(false);
        }
    }
}