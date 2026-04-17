using System.Reflection;
using System.Runtime.CompilerServices;
using Blacksmith.Infra.Attributes;
namespace Blacksmith.Infra.ExtensibleEnum
{
    public abstract class BlacksmithEnum
    {
        protected static bool _isOpen = true;
        public static void CloseFactory() => _isOpen = false;
        public abstract Type GetEEValueType();
        public abstract void Create(string name, int priority);
    }
    public abstract class BlacksmithEnum<T> : BlacksmithEnum 
        where T : BlacksmithEnum<T>, new()
    {
        //实际上可断言一定是先调用构造函数，此时已经不是null
        public static T Instance { get; private set; } = null!;
        
        public struct EEValue : IComparable<EEValue>
        {
            private static int _counter = 0;
            private readonly int _uniqueID;
            public readonly int _priority;
            internal EEValue(int priority)
            {
                if (!_isOpen)
                {
                    throw new ArgumentException("EEValue Factory has been closed!");
                }
                _uniqueID = _counter++;
                _priority = priority;
            }
            public int CompareTo(EEValue other)
            {
                return _priority.CompareTo(other._priority);
            }
            public static bool operator ==(EEValue left, EEValue right)
            {
                return left._uniqueID == right._uniqueID;
            }
            public static bool operator !=(EEValue left, EEValue right)
            {
                return left._uniqueID != right._uniqueID;
            }
            public override bool Equals(object? obj)
            {
                return obj is EEValue other && _uniqueID == other._uniqueID;
            }
            public override int GetHashCode()
            {
                return _uniqueID.GetHashCode();
            }
        }
        public override Type GetEEValueType()
        {
            return typeof(EEValue);
        }
        public override void Create(string name, int priority)
        {
            if (!_isOpen)
            {
                throw new ArgumentException("EEValue Factory has been closed!");
            }
            //这里选择直接覆盖。程序启动时就已经被构造
            //情况与技能包不同，技能包每次使用都需要创建实例，不便于指定构造参数来应用Modifier
            //因此采用的方法是在构造函数插入一个修改阶段
            //而Enum是全局单例，干脆在初始化阶段就修改
            _enumDict[name] = new EEValue(priority);
        }
        protected BlacksmithEnum()
        {
            Instance = (T)this;
            var type = GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var metaData = method.GetCustomAttribute<IsBlacksmithEnumMenberExtension>();
                if (method.ReturnType != typeof(EEValue) ||
                    method.GetParameters().Length != 0 || 
                    metaData == null)
                {
                    continue; 
                }
                string methodName = method.Name;
                Create(methodName, metaData.Priority);
            }
        }
        private static Dictionary<string, EEValue> _enumDict = new();
        public static EEValue GetEEValue([CallerMemberName] string name = "") => _enumDict[name];
    }
    public class TestType : BlacksmithEnum<TestType>
    {
        [IsBlacksmithEnumMenberExtension(256)]
        public EEValue Physical() => GetEEValue();

        [IsBlacksmithEnumMenberExtension(128)]
        public EEValue Magical() => GetEEValue();

        [IsBlacksmithEnumMenberExtension(0)]
        public EEValue Real() => GetEEValue();
    }
    //模拟外部程序集
    [IsBlacksmithEnumModifier]
    public static class MyTestEnumExtension
    {
        [IsBlacksmithEnumMenberExtension(256)]
        public static TestType.EEValue Magical(this TestType testType) => TestType.GetEEValue();
    }

}
