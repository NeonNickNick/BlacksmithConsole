using ClapInfra.ClapEnum;

namespace BlacksmithCore.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class IsBlacksmithEnumMember : Attribute, IIsClapEnumMember
    {
        public int Priority { get; }
        public IsBlacksmithEnumMember(int priority)
        {
            Priority = priority;
        }
    }
}
