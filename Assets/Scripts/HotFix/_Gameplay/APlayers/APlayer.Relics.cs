using System.Collections.Generic;

namespace MoreMountains
{
    public partial class APlayer
    {
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

        public void applyOnShootBallRelics(Ball ball)
        {
            foreach (var relic in relics)
                relic.onShootBall(ball);
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

        public void loseRandomRelics(int amount)
        {
            if (amount > relics.Count)
            {
                foreach (var relic in relics)
                    relic.onUnequip(this);
                relics.Clear();
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                int index = MathUtils.random(0, relics.Count - 1);
                relics[index].onUnequip(this);
                relics.RemoveAt(index);
            }

            reorganizeRelics();
        }

        public bool loseRelic(string relicId)
        {
            if (!hasRelic(relicId))
                return false;
            ARelic toRemove = null;
            foreach (var relic in relics)
            {
                if (relic.relicId == relicId)
                {
                    relic.onUnequip(this);
                    toRemove = relic;
                }
            }

            if (toRemove == null)
            {
                log("WHY WAS RELIC: " + name + " NOT FOUND???");
                return false;
            }

            relics.Remove(toRemove);
            reorganizeRelics();
            return true;
        }

        public void reorganizeRelics()
        {
            log("Reorganizing relics");
            List<ARelic> tmpRelics = new();
            tmpRelics.AddRange(relics);
            relics.Clear();
            for (int i = 0; i < tmpRelics.Count; i++)
                tmpRelics[i].reorganizeObtain(this, i, false, tmpRelics.Count);
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

        public List<string> getRelicNames()
        {
            List<string> arr = new();
            foreach (var relic in relics)
                arr.Add(relic.relicId);
            return arr;
        }

        public bool relicsDoneAnimating()
        {
            foreach (var r in relics)
            {
                if (!r.isDone)
                    return false;
            }

            return true;
        }
    }
}