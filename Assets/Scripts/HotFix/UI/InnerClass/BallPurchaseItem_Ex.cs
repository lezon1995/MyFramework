using UnityEngine;

namespace MoreMountains;

public partial class BallPurchaseItem
{
    public myUGUIButton Btn => btn;

    public void Refresh(BallDef def)
    {
        var item = BallItem.New(def);
        SetName(def.DisplayName.GetLocalizedString());
        SetPrice(item.BuyPrice);
        SetDesc(item);
        if (def.Icon)
            SetIcon(def.Icon);
        SetRarity(def.Rarity);
        BallItem.Release(item);
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

    public void SetDesc(BallItem item)
    {
        using var _ = new MyStringBuilderScope(out var sb);
        BallDef.BuildDescription(sb, item, player);
        itemDesc.setText(sb.ToString());
    }
}