using UnityEngine;

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
    public void SetDesc(string s) => itemDesc.setText(s ?? string.Empty);
    
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