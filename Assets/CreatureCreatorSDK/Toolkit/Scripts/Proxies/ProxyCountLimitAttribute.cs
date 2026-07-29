using System;

namespace DanielLochner.CreatureCrafter.SDK
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class ProxyCountLimitAttribute : Attribute
    {
        public Type CategoryType { get; }
        public int Maximum { get; }

        public ProxyCountLimitAttribute(int maximum)
        {
            Maximum = maximum;
        }

        public ProxyCountLimitAttribute(Type categoryType, int maximum)
        {
            CategoryType = categoryType;
            Maximum = maximum;
        }
    }
}
