using UnityEngine;

namespace MoreMountains;

public partial class BallPurchaseItem
{
    public myUGUIButton Btn => btn;

    public void SetHovered(bool on)
    {
        if (hovered != null) hovered.setActive(on);
    }

    public void SetRarity(Color top, Color bot)
    {
        if (rarityTop != null) rarityTop.setColor(new(top.r, top.g, top.b));
        if (rarityBot != null) rarityBot.setColor(new(bot.r, bot.g, bot.b));
    }

    public void SetNewTag(bool on) => newTag.setActive(on);
    public void SetIcon(Sprite s) => itemIcon?.setSpriteOnly(s);
    public void SetName(string s) => itemName.setText(s ?? string.Empty);
    public void SetDesc(string s) => itemDesc.setText(s ?? string.Empty);
    public void SetPrice(int price) => itemPrice.setText(price.IToS());
}