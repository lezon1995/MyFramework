using UnityEngine;

namespace MoreMountains;

public partial class BallTooltipItem
{
    public myUGUIButton Btn => btn;

    public void Refresh(BallItem item)
    {
        var def = item.Def;
        SetName(def.DisplayName.GetLocalizedString());
        SetPrice(item.SellPrice);
        if (def)
            SetDesc(item);
        if (def.Icon)
            SetIcon(def.Icon);
        SetRarity(item.getLevelToRarity());
    }

    public void SetHovered(bool on)
    {
        hovered?.setActive(on);
    }

    public void SetRarity(ItemRarity rarity)
    {
        var c = gameDesign.getRarityColor(rarity);
        itemBorder.setColor(c.border);
        itemBg.setColor(c.bg);
        IconBg.setColor(c.iconBg);
        itemName.setColor(c.title);
    }

    public void SetIcon(Sprite s) => itemIcon?.setSpriteOnly(s);
    public void SetName(string s) => itemName.setText(s ?? string.Empty);
    public void SetPrice(int price) => itemPrice.setText(price.IToS());

    public void SetDesc(BallItem item)
    {
        using var _ = new MyStringBuilderScope(out var sb);
        BallDef.BuildDescription(sb, item, player);
        itemDesc.setText(sb.ToString());
    }
}