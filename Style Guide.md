# SlopperEngine Style Guide

The style of code to be used for any C# code related to any official SlopperEngine releases.
In any ambiguous case, write code the way it looks prettiest / easiest to read. 
This style guide and example is based on and reformatted from https://google.github.io/styleguide/csharp-style.html.

If you find any code that does NOT adhere to the style, feel free to touch it up!

## Formatting guidelines

### Naming rules

**Code**

- Classes, methods, enums, namespaces are all `PascalCase`.
- Public fields/properties are all `PascalCase`.
- Protected fields/properties, and method variables are all `camelCase`.
- Private fields/properties are all `_camelCase`.
- Const, Static, Readonly do not influence naming convention.
- Interfaces are `PascalCase` but prefixed with an I, for example `IMyInterface`.
- Avoid naming objects the same as common `System` objects - for example, do not make a `List` class.
- Generic arguments should be named `T` if they are the only argument and their use is obvious.
	- If their use is not obvious, they should be named after their use (`PascalCase`), prefixed with a T, for example `TGenerator`.

**Files**

- Filenames and directory names are all `PascalCase`, except for the file extension.
- The filename should be equal to the name of the main object in the file, for example `IMyInterface.cs`.

### Organisation rules

**Namespaces**

- Namespace usings are at the top of the file. 
- Namespaces should be file scoped.

**Class member ordering**

- Member types should be ordered as follows:
	- Properties.
	- Fields.
	- Delegates and events.
	- Constructors and finalizers.
	- Methods.
	- Nested classes and nested enums.

- Members in these groups should be ordered by:
	- `public`.
	- `protected`.
	- `private`.
	- A backing field for a property, even if `private`, should be placed next to the property.

- Interface implementations should be grouped.

### Whitespace rules

- Only one statement per line.
- Only one assignment per statement.
- Indentation of four spaces, no tabs.
- No explicit column limit, but try to keep it within reason.
	- If the line is a function call with many arguments for example, it should be broken up into multiple lines.
- Line break before any opening brace.
- Don't use braces when optional, with one exception:
	- In the case of an `if`/`else` where the `else` requires braces, the `if` should also have braces.
- Space after commas.
- Space between all operators and operands, except unary operators.
- Space after `//`.
- Comments should be next to the member they clarify, with the same indentation and no newline seperating them.
- Fields and properties should be seperated by newlines.

## Example
```c#
// Usings on top
using System;
using SlopperEngine.Core;

// Namespace following usings
namespace MyNamespace;

// Interface is prefixed with 'I'
public interface IMyInterface
{
    // Methods are PascalCase
    // Space after commas
    // function inputs are camelCase
    public int Calculate(float value, float coolExponent);
}

// Enums are PascalCase.
public enum MyEnum
{
    // Enumerators are also PascalCase.
    Yes,
    No,
}

// Classes are PascalCase.
public class MyClass
{
    // Public members are PascalCase.
    // Group backing fields with properties.
    public int CoolGetterSetter
    {
        get => _cool;
        set
        {
            _cool = value;
            Console.WriteLine($"I just got set to {value}");
        }
    }
    int _cool;

    // Static does not influence naming. 
    public static int NumTimesCalled = 0;
    
    // Field initializers are encouraged. 
    public bool NoCounting = false;

    // Private members are _camelCase.
    private Results? _results;

    // Const does not influence naming.
    // Private may be omitted.
    const int _bar = 100;

    // Container initializers may be on one line if brief enough,
    int[] _someTable = {2, 3, 4};

    // or they may be broken up.
    float[] _someOtherTable = 
    {
        5.799164878f,
        4.499915771f,
        -0.033781745f,
    };

    public MyClass()
    {
        // Object initializers should not be used. 
        _results = new();
        _results.NumNegativeResults = 1;
        _results.NumPositiveResults = 1;
    }

    public int CalculateValue(int mulNumber)
    {
        // var may be used.
        // Local variables are camelCase.
        var resultValue = CoolGetterSetter * mulNumber;
        NumTimesCalled++;
        CoolGetterSetter += _bar;

        // No space between unary operator and operand
        if(!NoCounting)
        {
            // No braces used when optional
            if(resultValue < 0)
                _results.NumNegativeResults++;
            // else condition on the same line if possible.
            else if(resultValue > 0)
                _results.NumPositiveResults++;
        }

        return resultValue;
    }

    public void ExpressionBodies()
    {
        // Simple lambdas may be fitted on one line
        Func<int, int> increment = x => x + 1;

        // Closing brace aligns with first character on line that includes the opening brace.
        Func<int, int, long> difference = (x, y) =>
        {
            long diff = (long)x - y;
            return diff >= 0 ? diff : -diff;
        };

        // Inline lambda arguments also follow these rules. Prefer newline before the function body.
        CallWithDelegate((x, y) =>
        {
            long diff = (long)x - y;
            return diff >= 0 ? diff : -diff;
        });
    }

    // Empty blocks should be concise.
    void DoNothing() {}

    // If method arguments are just too far, start them on a newline.
    void VeryVeryLongFunctionNameThatMakesTheCodeKindaHardToReadAndGoodnessSomeoneShouldDoSomethingAboutIt(
        int argument1, int p1, int p2
    ) {}

    void CallingLongFunctionName()
    {
        int veryLongArgumentName = 1234;
        
        // Wrap arguments on a newline if it makes the code more readable.
        VeryVeryLongFunctionNameThatMakesTheCodeKindaHardToReadAndGoodnessSomeoneShouldDoSomethingAboutIt(
            veryLongArgumentName,
            veryLongArgumentName,
            veryLongArgumentName
        )
    }

    // Nested objects always at the bottom.
    // If this makes the code harder to navigate, make a new file for the object.
    private class Results
    {
        public int NumNegativeResults = 0;
        public int NumPositiveResults = 0;
    }
}

```
## Code guidelines

### Constants

- Variables and fields that are constant should always be `const`.
- Try always marking `readonly` fields.
- Prefer named constants to magic numbers.

### Expression bodies

Expression body example: `int SomeProperty => _someField;`

- Single line readonly properties should have expression bodies when possible.
- Get/set properties should also use expression bodies.
- Use only for very simple methods.

### Structs and classes

The difference between structs and classes, is that classes exist on the heap and are garbage collected, 
while structs almost always exist on the stack. This makes structs significantly more performant, at the
cost of flexibility. 

Structs are almost always passed and returned by value (meaning it gets copied), resulting in possible
unexpected behaviour. Think of them like an `int` in this way, only changing when explicitly assigned to.
Structs also cannot be polymorphic - it is impossible to inherit from one.

- Use a class when it concerns data that may be owned by multiple parties. 
- Use a struct when it concerns data that has a particular owner or is used in mass.
- Use a struct when it concerns data that only exists on the stack.

### SlopperEngine features

- Methods that are called by the engine through the use of an attribute (for example, `[OnFrameUpdate]` or 
`[OnSerialize]`) should at most have only one input, named (attribute name)Args, for example `FrameUpdateArgs`.
- Public and protected members and objects should always have a summary attached, unless it's extremely
obvious what everything means.

### Extension methods

- For most types, avoid extension methods at all costs.
	- Usual static methods are generally preferred for their clarity and non-clutter.
- For enums, extension methods are permissible. 

### The `ref`, `in`, and `out` keywords

`ref` and `in` let us pass a struct by reference - basically, instead of copying the struct (which is the
default behaviour), we pass a pointer to the struct. Passing a large struct by reference like this can
seriously improve performance over using classes or copying structs, and is thus preferred.

- Use `out` for TryGet methods.
- Place `out` parameters after all other parameters.
- Use `ref` when the input may be mutated.
- Use `ref readonly` or `in` instead when this is just for performance.

### Little things

**LINQ**

LINQ is quite handy for creating algorithms that apply well to any situation. However, LINQ methods 
can generate quite a lot of memory garbage. Avoid LINQ and IEnumerable extension method calls for any part 
of the engine.

**Namespaces**

Namespaces match folder structure, for example `SlopperEngine.MyStuff.IMyInterface` should be in the 
`SlopperEngine/MyStuff/IMyInterface.cs` file.

**Nesting**

Avoid nesting as much as possible. 

**Var**

Use the `var` keyword when it aids readability, or if the variable is obvious or unimportant.

**Exceptions**

Avoid throwing exceptions. Methods like `bool TryXYZ(out T success)` are much preferred.
Once a more advanced logging system is in place, use this instead.

**Lambdas vs named methods**

If a lambda is complicated (more than ~3 lines), it should likely be a named method.
This named method may be nested at your discretion.

**Field initializers**

Try adding field initializers, even if it's the default value.
If the value is irrelevant when default, it may be omitted.
