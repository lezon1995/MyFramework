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
                var barRenderer = view.DamageChunkHealthBarUI;
                barRenderer.SetHealth(_brick.Health);
                _brick.Health.onShieldChanged = (pre, cur) =>
                {
                    if (cur > 0)
                        view.Shield.setText(cur);
                    else
                        view.Shield.setText(null);
                };
                healthBar = new(t, barRenderer, view.Health.getTextComponent());
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