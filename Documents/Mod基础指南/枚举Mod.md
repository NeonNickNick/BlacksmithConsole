# 枚举Mod
[返回](./引言.md)

本文档介绍如何扩展项目中的“可扩展枚举”系统，包括：

- 新建一个可被继续扩展的枚举类型
- 为现有枚举添加新成员
- 调整成员排序优先级

## 适用范围

项目中的很多核心类型并没有直接使用 C# 原生 `enum`，而是使用继承自 `ClapInfra.ClapEnum.ClapEnum<T>` 的 `BlacksmithEnum<T>` 来模拟可扩展枚举，例如：

- `ResourceType`
- `AttackType`
- `DefenseType`
- `EffectType`
- `DynamicJudgeRuleName`
- `JudgeStage`

这样做的好处是：内置内容和外部 Mod 都可以向同一个类型继续追加成员。

## 总体流程

1. 创建一个 `.NET 8` 类库项目。
2. 引用 `BlacksmithCore.csproj`，或者引用编译后的 `BlacksmithCore.dll`。
3. 编写枚举类或枚举修改类。
4. 将编译产物放到游戏可执行文件所在目录。
5. 程序启动时会自动扫描该目录下的 `.dll` 并加载。

## 创建新的可扩展枚举

假设你想创建一个名字枚举。如果直接写成原生 `enum`：

```csharp
public enum Names
{
    Alice = 0,
    Bob = 1,
    Carol = 2
}
```

它本身不能被其他 Mod 继续扩展。正确做法是继承 `BlacksmithEnum<T>`：

```csharp
using BlacksmithCore.Infra.Attributes;
using BlacksmithCore.Infra.ExtensibleEnum;

public class Names : BlacksmithEnum<Names>
{
    [IsBlacksmithEnumMember(0)]
    public BEValue Alice() => GetBEValue();

    [IsBlacksmithEnumMember(1)]
    public BEValue Bob() => GetBEValue();

    [IsBlacksmithEnumMember(2)]
    public BEValue Carol() => GetBEValue();
}
```

说明：

- 返回类型必须是当前枚举自己的 `BEValue`。
- 方法必须是 `public` 实例方法。
- 方法不能带参数。
- `[IsBlacksmithEnumMember(priority)]` 中的值是排序优先级，不是全局唯一值。

## 比较与排序行为

```csharp
Names.Instance.Alice() == Names.Instance.Alice(); // true
Names.Instance.Alice() == Names.Instance.Bob();   // false
```

不同枚举类型之间不能互相比较。

排序时使用的是 `priority`。如果 `priority` 更小，就会排在更前面；这也是 `JudgeStage` 和部分防御类型能被扩展排序的基础。

## 修改现有枚举

如果你希望为已有枚举添加新成员，或者调整现有成员的排序优先级，需要写一个静态类，并加上 `[IsBlacksmithEnumModifier]`：

```csharp
using BlacksmithCore.Infra.Attributes;

[IsBlacksmithEnumModifier]
public static class NamesExtension
{
    [IsBlacksmithEnumMember(-1)]
    public static Names.BEValue Carol(this Names names) => Names.GetBEValue();

    [IsBlacksmithEnumMember(3)]
    public static Names.BEValue Dave(this Names names) => Names.GetBEValue();
}
```

约定如下：

- 必须是 `public static` 方法。
- 第一个也是唯一一个参数必须是被扩展的枚举类型本身，例如 `this Names names`。
- 返回类型必须是该枚举的 `BEValue`。
- 方法名就是最终成员名。

### 覆盖与追加的区别

- 如果方法名和现有成员同名，例如上面的 `Carol`，则会覆盖该成员的排序优先级。
- 如果方法名不存在，则会追加一个新成员。

## 资源类型的特殊规则

如果你扩展的是 `ResourceType`，并且想引入一组“普通资源 / 金资源”配对资源，那么金资源命名必须使用：

```text
Gold_普通资源名
```

例如：

- `Cross`
- `Gold_Cross`

否则资源系统不会把它们识别成一对共享模板的资源。

## 来自示例项目的真实写法

仓库中的 `Blacksmith/ModExamples/EnumExtension.cs` 提供了一个实际例子：

```csharp
[IsBlacksmithEnumModifier]
public static class ResourceExtension
{
    [IsBlacksmithEnumMember(0)]
    public static ResourceType.BEValue Cross(this ResourceType resourceType)
        => ResourceType.GetBEValue();
}
```

这说明：

- 外部 Mod 不需要修改主工程源码。
- 只要把扩展类编译进外部 DLL，启动时即可自动加载。

## 注意事项

1. 多个 Mod 修改同一个枚举成员时，最终结果取决于 DLL 扫描加载顺序。
2. 如果两个 Mod 定义了同名的枚举类型，程序会抛出异常。
3. `priority` 只决定排序，不保证跨 Mod 的兼容性。
4. 启动阶段结束后，`BlacksmithEnum` 工厂会关闭，因此不要指望运行过程中继续动态创建新成员。
