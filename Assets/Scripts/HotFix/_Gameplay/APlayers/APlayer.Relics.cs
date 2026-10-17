using System.Collections.Generic;

namespace MoreMountains
{
    public partial class APlayer
    {
        // public List<ARelic> relics = new();

        public List<ARelic> relics => inventory.RelicBag.Relics;

        public void gainRelic(RelicDef def)
        {
            inventory.AddRelic(def);
        }

        public void applyStartOfTurnRelics()
        {
            foreach (var relic in relics)
                relic.atTurnStart();

            // foreach (var b in blights)
            // b?.atTurnStart();
        }

        public void applyStartOfTurnPostDrawRelics()
        {
            foreach (var relic in relics)
                relic.atTurnStartPostDraw();
        }

        public void onShootBall(Ball ball)
        {
            foreach (var relic in relics)
                relic.onShootBall(this, ball);
        }

        public bool hasRelic(string relicId)
        {
            foreach (var relic in relics)
            {
                if (relic.relicId == relicId)
                    return true;
            }

            return false;
        }

        public bool tryGetRelic(string relicId, out ARelic result)
        {
            foreach (var relic in relics)
            {
                if (relic.relicId == relicId)
                {
                    result = relic;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public ARelic getRelic(string relicId)
        {
            foreach (var r in relics)
            {
                if (r.relicId == relicId)
                    return r;
            }

            return null;
        }

        public int getRelicCount<T>() where T : ARelic
        {
            int count = 0;
            foreach (var r in relics)
            {
                if (r is T)
                    count++;
            }

            return count;
        }

        public void getRelicNumber<T>(T relic, out int number, out int maxNumber) where T : ARelic
        {
            number = 0;
            int count = 0;
            foreach (var r in relics)
            {
                if (r is T)
                {
                    count++;
                    if (r == relic)
                    {
                        number = count;
                    }
                }
            }

            maxNumber = count;
        }

        public void fixedUpdateRelics(float dt)
        {
            for (int i = 0; i < relics.Count; i++)
                relics[i].onFixedUpdate(dt);
        }

        public void updateRelics(float dt)
        {
            for (int i = 0; i < relics.Count; i++)
                relics[i].onUpdate(dt);
        }

        public List<string> getRelicNames()
        {
            List<string> arr = new();
            foreach (var relic in relics)
                arr.Add(relic.relicId);
            return arr;
        }
    }
}