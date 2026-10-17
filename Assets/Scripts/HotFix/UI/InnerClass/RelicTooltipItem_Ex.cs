using UnityEngine;

namespace MoreMountains;

public partial class RelicTooltipItem
{
    public myUGUIButton Btn => btn;

    public void Refresh(RelicDef def)
    {
        SetName(def.DisplayName.GetLocalizedString());
        int realPrice = def.GetBuyPrice(out var rawPrice);
        SetPrice(realPrice, rawPrice);
        if (def)
            SetDesc(def);
        if (def.Icon)
            SetIcon(def.Icon);
        SetRarity(def.Rarity);
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
    public void SetPrice(int realPrice, int rawPrice)
    {
        if (realPrice < rawPrice)
        {
            using var _ = new MyStringBuilderScope(out var sb);
            sb.add(realPrice).add(" ").add("(").add(rawPrice).add(" - ").add(rawPrice - realPrice).add(")");
            var enhanced = gameDesign.universalColor.enhanced;
            itemPrice.setText(sb.ToString().color(enhanced));
        }
        else if (realPrice > rawPrice)
        {
            using var _ = new MyStringBuilderScope(out var sb);
            sb.add(realPrice).add(" ").add("(").add(rawPrice).add(" + ").add(realPrice - rawPrice).add(")");
            var reduced = gameDesign.universalColor.reduced;
            itemPrice.setText(sb.ToString().color(reduced));
        }
        else
        {
            itemPrice.setText(realPrice.IToS());
        }
    }

    public void SetDesc(RelicDef def)
    {
        using var _ = new MyStringBuilderScope(out var sb);
        RelicDef.BuildDescription(sb, def);
        itemDesc.setText(sb.ToString());
    }

}