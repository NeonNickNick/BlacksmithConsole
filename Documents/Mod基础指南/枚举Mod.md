# 枚举Mod
[返回](./引言.md)

本文档面向拿到本项目编译后DLL或源码并希望对枚举进行扩展的开发者。具体说明如何实现一个枚举Mod。

目前Mod支持的功能具体包括：添加新枚举、修改内置枚举（包括覆盖与追加成员），修改新枚举。

# 总体流程
创建一个.NET 8类库项目，添加对游戏主程序导出的程序集Blacksmith.dll的引用。

在项目中按照下面的步骤编写若干类。

将编译产物放到游戏可执行文件所在目录，程序启动时会自动扫描并加载Mod。

## 编写枚举Mod案例
枚举类型enum本身是不可扩展的。本项目采用单例+静态扩展方法来模拟枚举的行为。
### 创建新的枚举类型
假设希望创建一个名字枚举，例如
```csharp
public enum Names{
    Alice = 0,
    Bob = 1,
    Carol = 2
}
```
这样当然是可以的。但是别的Mod就不能扩展它了。为了让它具有被扩展的能力，需要这样写：
```csharp
public class Names : BlacksmithEnum<Names>{
    [IsBlacksmithEnumMember(0)]//重要，相当于优先级
    public EEValue Alice() => GetEEValue();//不允许填入参数
    [IsBlacksmithEnumMember(1)]
    public EEValue Bob() => GetEEValue();
    [IsBlacksmithEnumMember(2)]
    public EEValue Carol() => GetEEValue();
}
public class OtherNames : BlacksmithEnum<OtherNames>{
    [IsBlacksmithEnumMember(0)]
    public EEValue Dave() => GetEEValue();
    [IsBlacksmithEnumMember(1)]
    public EEValue Eve() => GetEEValue();
    [IsBlacksmithEnumMember(2)]
    public EEValue Francis() => GetEEValue();
}
```
不要深究内部具体发生了什么。它们相互比较时的行为如下：
```csharp
Names.Instance.Alice() == OtherNames.Instance.Dave()//无法通过编译
Names.Instance.Alice() == Names.Instance.Bob()//false
Names.Instance.Alice() == Names.Instance.Alice()//true
```
在Linq方法排序时，例如：
```csharp
List<Names> names = new(){Names.Instance.Carol(), Names.Instance.Alice(),Names.Instance.Bob()}
```
会按照在属性里指定的数值从小到大排序，即第一个是Alice

相比于enum，它的一个额外能力是Alice和Bob实际上数值可以定成一样的，不会出错，==会返回false，但是排序时认为它们是一样的，将会保序。

### 修改现有枚举
现在希望将OtherNames中的三者从外部加进Names，并且想要把Carol的数值改成-1
```csharp
[IsBlacksmithEnumModifier]//重要
public static class Merge{
    [IsBlacksmithEnumMember(-1)]//直接覆盖即可
    public Names.EEValue Carol(this Names names) => Names.GetEEValue();
    [IsBlacksmithEnumMember(0)]//重要，相当于优先级
    public Names.EEValue Dave(this Names names) => Names.GetEEValue();
    [IsBlacksmithEnumMember(1)]
    public Names.EEValue Eve(this Names names) => Names.GetEEValue();
    [IsBlacksmithEnumMember(2)]
    public Names.EEValue Francis(this Names names) => Names.GetEEValue();
}
```
这样就修改完成了。

注意！如果想要做的是扩展资源类型，且需要定义一对具有铁-金铁关系的相关资源，金资源命名必须是配对资源名加前缀“Gold_”！

## 温馨提示
该方案不保证多个Mod能够兼容。如果两个Mod都修改了同一个枚举成员，那么后被主程序加载的会生效。可以通过调整文件名来实现控制生效。如果两个Mod定义了同一个枚举（名字相同），即使命名空间不同，游戏会直接抛出异常。
