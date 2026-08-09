using UnityEngine;

namespace MoreMountains;

public partial class BallPurchaseItem
{
    public myUGUIButton Btn => btn;

    public void SetHovered(bool on)
    {
        hovered?.setActive(on);
    }

    public void SetRarity(Color top, Color bot)
    {
        rarityTop?.setColor(new(top.r, top.g, top.b));
        rarityBot?.setColor(new(bot.r, bot.g, bot.b));
    }

    public void SetNewTag(bool on) => newTag.setActive(on);
    public void SetIcon(Sprite s) => itemIcon?.setSpriteOnly(s);
    public void SetName(string s) => itemName.setText(s ?? string.Empty);
    public void SetDesc(string s) => itemDesc.setText(s ?? string.Empty);
    public void SetPrice(int price) => itemPrice.setText(price.IToS());

    public void SetSold(bool sold)
    {
        hovered.setActive(!sold);
        rarityTop.setActive(!sold);
        itemSold.setActive(sold);
        if (btn.tryGetUnityComponent<ButtonScaleAnim>(out var btnScaleAnim))
        {
            btnScaleAnim.ResetToNormal();
            btnScaleAnim.enabled = !sold;
        }
    }
}