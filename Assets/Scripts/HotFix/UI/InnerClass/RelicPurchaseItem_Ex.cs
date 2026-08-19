using UnityEngine;

namespace MoreMountains;

public partial class RelicPurchaseItem
{
    public myUGUIButton Btn => btn;

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

    public void SetNewTag(bool on) => newTag.setActive(on);
    public void SetIcon(Sprite s) => itemIcon?.setSpriteOnly(s);
    public void SetName(string s) => itemName.setText(s ?? string.Empty);
    public void SetPrice(int price) => itemPrice.setText(price.IToS());

    public void SetSold(bool sold)
    {
        hovered.setActive(!sold);
        itemSold.setActive(sold);
        if (btn.tryGetUnityComponent<ButtonScaleAnim>(out var btnScaleAnim))
        {
            btnScaleAnim.ResetToNormal();
            btnScaleAnim.enabled = !sold;
        }
    }

    public void SetDesc(RelicDef def)
    {
        using var _ = new MyStringBuilderScope(out var sb);

        var mods = def.PlayerStatMods;
        if (mods is { Length: > 0 })
        {
            build_StatMods(sb, mods);
        }

        build_DisplayDescription(sb, def);

        itemDesc.setText(sb.ToString());
    }
    
    static void build_DisplayDescription(MyStringBuilder sb, RelicDef def)
    {
        sb.addLine();
        var localizedString = def.DisplayDescription.GetLocalizedString();
        sb.add(localizedString);
    }

    static void build_StatMods(MyStringBuilder sb, PlayerStatMod[] mods)
    {
        var universalColor = gameDesign.universalColor;

        foreach (var mod in mods)
        {
            var statName = LocalizedStats.getName(mod.StatName);
            sb.add(mod.StatName.toSprite());
            sb.add(" ");

            var bonusFlat = mod.BonusFlat;
            var bonusPct = mod.BonusPct;
            if (bonusFlat > 0)
            {
                var color = universalColor.enhanced.toRGBA();
                sb.colorString(color, "+", $"{bonusFlat.FToS(0)}");
            }
            else if (bonusFlat < 0)
            {
                var color = universalColor.reduced.toRGBA();
                sb.colorString(color, "-", $"{bonusFlat.FToS(0)}");
            }
            else if (bonusPct > 0)
            {
                var color = universalColor.enhanced.toRGBA();
                sb.colorString(color, "+", $"{bonusPct.toPercent(0)}");
            }
            else if (bonusPct < 0)
            {
                var color = universalColor.reduced.toRGBA();
                sb.colorString(color, "-", $"{bonusPct.toPercent(0)}");
            }

            sb.add(statName);
            sb.addLine();
        }
    }
}