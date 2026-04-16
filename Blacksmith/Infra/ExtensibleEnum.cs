using System.Reflection;
using System.Runtime.CompilerServices;
namespace Blacksmith.Infra
{
    public abstract class ExtensibleEnum
    {
        protected static bool _isOpen = true;
        public static void CloseFactory() => _isOpen = false;
        public abstract Type GetEEValueType();
        public abstract void Create(string name, int priority);
    }
    public class ExtensibleEnum<T> : ExtensibleEnum where T : new()
    {
        private static readonly Lazy<T> _lazy = new Lazy<T>(() => new T());
        public static T Instance => _lazy.Value;
        
        public struct EEValue : IComparable<EEValue>
        {
            private static int _counter = 0;
            private readonly int _uniqueID;
            private readonly int _priority;
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
            if(!_enumDict.TryGetValue(name, out var value))
            {
                var e = new EEValue(priority);
                _enumDict[name] = e;
            }
        }
        protected ExtensibleEnum()
        {
            var type = this.GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static)
                             .Where(f => f.IsInitOnly)
                             .Where(f => f.FieldType == typeof(int))
                             .ToList();
            foreach (var method in methods)
            {
                if (method.ReturnType != typeof(EEValue) || method.GetParameters().Length != 0)
                {
                    continue; 
                }
                string methodName = method.Name;
                string prefix = "_" + methodName.ToLower();
                var fieldNames = fields.Select(f => f.Name).Where(f => f.StartsWith(prefix)).ToList();
                if(fieldNames.Count != 1)
                {
                    continue;
                }
                int priority = 0;
                try
                {
                    priority = int.Parse(fieldNames[0].Remove(0, prefix.Length));
                }
                catch
                {
                    continue;
                }

                Create(methodName, priority);
            }
        }
        private static Dictionary<string, EEValue> _enumDict = new();
        public static EEValue GetEEValue([CallerMemberName] string name = "") => _enumDict[name];
    }
    public class TestType : ExtensibleEnum<TestType>
    {
        private static readonly int _physical256 = 256;
        public EEValue Physical() => GetEEValue();

        private static readonly int _magical128 = 128;
        public EEValue Magical() => GetEEValue();

        private static readonly int _real0 = 0;
        public EEValue Real() => GetEEValue();
    }
    //模拟外部程序集
    public static class MyTestEnumExtension
    {
        private static readonly int _mytype64 = 64;
        public static TestType.EEValue MyType(this TestType testType) => TestType.GetEEValue();
    }

}
