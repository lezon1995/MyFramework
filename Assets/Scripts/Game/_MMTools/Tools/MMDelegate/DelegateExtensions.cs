using System;

namespace MoreMountains.Tools
{
    public static class DelegateExtensions
    {
        public static Action Cached(this Action action)
        {
            return action;
        }
    }
}