using UnityEngine;
using UnityEngine.Localization.Settings;

namespace MoreMountains;

public partial class RewardChooseItem
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
        iconBg.setColor(c.iconBg);
        itemName.setColor(c.title);
    }

    public void SetIcon(Sprite s) => itemIcon?.setSpriteOnly(s);
    public void SetName(string s) => itemName.setText(s ?? string.Empty);

    public void SetDesc(BallStatOffer s)
    {
        using var _ = new MyStringBuilderScope(out var sb);
        var enhanced = gameDesign.universalColor.enhanced;
        sb.add("Ball");
        sb.add("+".color(enhanced));

        if (s.BonusFlat > 0)
            sb.add(s.Def.DisplayConfig.displayValue(s.BonusFlat).color(enhanced));

        if (s.BonusPct > 0)
            sb.add(s.Def.DisplayConfig.displayValue(s.BonusPct).color(enhanced));

        sb.add(" ");

        var str = LocalizationSettings.StringDatabase.GetTable("Stats").GetEntry(s.Def.statKey).Value;
        sb.add(str);
        itemDesc.setText(sb.ToString());
    }

    public void SetDesc(PlayerStatOffer s)
    {
        using var _ = new MyStringBuilderScope(out var sb);
        var enhanced = gameDesign.universalColor.enhanced;
        sb.add("+".color(enhanced));

        if (s.BonusFlat > 0)
            sb.add(s.Def.DisplayConfig.displayValue(s.BonusFlat).color(enhanced));

        if (s.BonusPct > 0)
            sb.add(s.Def.DisplayConfig.displayValue(s.BonusPct).color(enhanced));

        sb.add(" ");
        
        var str = LocalizationSettings.StringDatabase.GetTable("Stats").GetEntry(s.Def.statKey).Value;
        sb.add(str);
        itemDesc.setText(sb.ToString());
    }

    public void SetSold(bool sold)
    {
        hovered.setActive(!sold);
        // itemSold.setActive(sold);
        if (btn.tryGetUnityComponent<ButtonScaleAnim>(out var btnScaleAnim))
        {
            btnScaleAnim.ResetToNormal();
            btnScaleAnim.enabled = !sold;
        }
    }
}