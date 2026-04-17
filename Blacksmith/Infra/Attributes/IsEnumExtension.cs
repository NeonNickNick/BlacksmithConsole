namespace Blacksmith.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Class, 
        AllowMultiple = false, Inherited = false)]
    public class IsBlacksmithEnumModifier : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class IsBlacksmithEnumMenberExtension : Attribute
    {
        public readonly int Priority;
        public IsBlacksmithEnumMenberExtension(int priority)
        {
            Priority = priority;
        }
    }
}
