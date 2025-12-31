using System.Text;

namespace MarbleHero
{
    public class GameDataStringBuilder
    {
        StringBuilder sb = new();

        public void addFieldData(string value)
        {
            sb.Append(value).Append("\t");
        }

        public void addFieldData(int value)
        {
            addFieldData(value.ToString());
        }

        public void addFieldData(bool value)
        {
            addFieldData(value.ToString());
        }

        public string toString()
        {
            return sb.ToString();
        }
    }
}