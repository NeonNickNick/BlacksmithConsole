using BlacksmithCore.Infra.Attributes;
using ClapInfra.ClapEnum;

namespace BlacksmithCore.Infra.Enum
{
    public interface IBlacksmithEnum : IClapEnum
    {

    }
    public abstract class BlacksmithEnum<T> : 
        ClapEnum<T, IsBlacksmithEnumMember>, IBlacksmithEnum
        where T : BlacksmithEnum<T>, new()
    {
    }
}
