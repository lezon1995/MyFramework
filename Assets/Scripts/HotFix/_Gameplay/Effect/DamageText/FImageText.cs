namespace MoreMountains
{
    public class FImageText : FText
    {
        protected override void SetupTextTMP()
        {
            _text = new ImageTextTMP(_tmpOutline);
        }
    }
}