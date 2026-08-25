using UnityEngine;

namespace MoreMountains
{
    public class BossBrickRenderer : BrickRenderer
    {
        protected override void SetupHealthBar(GameObject obj)
        {
            var view = OverlayMenuService.Instance?.Binder?.Panel?.BossHealthBarView;
            if (view)
            {
                view.setActive(true);
                var t = view.getRoot().transform;
                healthBar = new(t, view.DamageChunkHealthBarUI, view.Health.getTextComponent());
            }
            else
            {
                base.SetupHealthBar(obj);
            }
        }

        protected override void SetupHealthBarSize(BrickDef def)
        {
            var view = OverlayMenuService.Instance?.Binder?.Panel?.BossHealthBarView;
            if (view)
            {
                var size = def.BossHealthBarSize;
                view.setSize(size);
            }
        }
    }
}