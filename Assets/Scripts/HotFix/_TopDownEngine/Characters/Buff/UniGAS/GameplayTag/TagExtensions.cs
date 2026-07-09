using System.Collections.Generic;
using System.Linq;

namespace MoreMountains.TopDownEngine
{
    public static class TagExtensions
    {
        public static IEnumerable<string> GetAllTags(this IEnumerable<Buff> buffs)
        {
            return buffs.SelectMany(buff => buff.BuffType.GrantedTags);
        }

        public static bool HasAllTags(this IEnumerable<Buff> buffs, string[] targetTags)
        {
            if (targetTags == null || targetTags.Length == 0)
            {
                return true;
            }

            var grantedTags = buffs.SelectMany(buff => buff.BuffType.GrantedTags);
            return targetTags.All(grantedTags.Contains);
        }

        public static bool HasNoneTags(this IEnumerable<Buff> buffs, string[] targetTags)
        {
            if (targetTags == null || targetTags.Length == 0)
            {
                return true;
            }

            var grantedTags = buffs.SelectMany(buff => buff.BuffType.GrantedTags);
            return targetTags.All(t => !grantedTags.Contains(t));
        }

        public static bool HasAny(this IEnumerable<Buff> buffs, string[] target)
        {
            var grantedTags = buffs.SelectMany(buff => buff.BuffType.GrantedTags);
            return target.Any(grantedTags.Contains);
        }

        public static bool HasAny(this string[] source, string[] target)
        {
            if (target == null || target.Length == 0)
            {
                return false;
            }

            foreach (var tag in target)
            {
                foreach (var srcTag in source)
                {
                    if (srcTag == tag)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}