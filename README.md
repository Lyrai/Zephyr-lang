# Zephyr
Zephyr is a statically-typed language that uses Roslyn to compile into .NET 6 and .NET Framework executables.

# How to build
```
git clone https://github.com/Lyrai/Zephyr-lang
cd Zephyr-lang
git submodule update --init
dotnet build
```

# Overview
A program entry point is global `main` function.
Here is example that prints `Hello world!`
```
fn main! {
    print "Hello world!"
}
```
`!` in a function declaration means that function does not take any parameters. Also, it has the same meaning when calling a function.
The newline symbol `\n` is used as an optional end-of-statement.

Local variables are defined using `let` keyword. Variable type can be infered automatically, or set explicitly.
```
fn main! {
    let a = 5
    let b string = "Hello world!"
    print b
}
```

Functions are defined using `fn` keyword (as you could see earlier with `main`). Function body might be either a block in braces or a single statement. Parameter list starts with `:`. Return type might be specified using `->` or omitted, which means return type `void`. Return value is the last computed value of the block or statement.
```
fn sum1: a int, b int -> int {
    let result = a + b
    result
}

fn sum2: a int, b int -> int
    a + b

fn main! {
    print sum1: 1, 2;
    print sum2: 3, 4
}
```
Here `;` is an optional end-of-the-argument-list. Also here you can see that `print` is not a function, but an operator.

In Zephyr, code blocks and `if-else` statements can return value:
```
fn main! {
    let a = {
        print "Inside a block"
        5
    }
    let b = if a == 5 "true" else "false"

    print a
    print b
}
```
Return value of a block is the last computed value of this block. Technically, a function's body in braces is the same block as above.

There are `for` and `while` loops:
```
fn main! {
    for let a = 0, a < 5, a = a + 1
        print a

    let counter = 5
    while counter > 0 {
        counter = counter - 1
        print counter
    }
}
```
Now there are no compound assignment operators like `+=`.

Arrays are created using `[]`:
```
fn main! {
    let arr = [1, 2, 3]
    print arr[0]
}
```
Here arr will have type `[int]`, or `int[]` in terms of .NET. Now only static arrays are available, as Zephyr does not have support for generics.

Classes are declared using `class` keyword:
```
class Test
    field int
    fn say_hello: name string
        print "Hello " + name + "!"
end

fn main! {
    let a = Test!
    a say_hello: "world"
    a field = 5
    print a field
}
```
Class declaration is the only place where you can see the `end` keyword, meaning end of class declaration. This is because any code in braces is the same - executable, and class declaration cannot be executed in any way. The `Test!` here is constructor call. 
Also you can see that methods are called just by writing method name after the caller object. In this way, method calls are left-associative, meaning that
```
a do_smth1! do_smth2: 5 do_smth3: "test", true
```
will be interpreted as
```
((a do_smth1!) do_smth2: 5) do_smth3: "test", true
```
To use method call or expression other than literal or single name as an argument, you should use parentheses:
```
a do_smth1! do_smth2: (5 + 6) do_smth3: "test", true
```
Other way is to use block:
```
a do_smth1! do_smth2: {5 + 6} do_smth3: "test", true
```
In general, any code that intended to be used as a single value can be grouped using braces, as block is legal in any place, where a literal is legal.

Any non generic .NET class and method can be accessed from Zephyr:
```
use System
use System.Diagnostics

fn main! {
    let sw = Stopwatch!
    Console Writeline: "Hello from .NET"
    let a = 5
    print a ToString!
}
```
If you want to explicitly qualify full class name with namespace you can do it like this:
```
let sw = System.Diagnostics Stopwatch!
```

Global functions can be used as methods (uniform function call syntax):
```
fn say_hello: name string
    print "Hello " + name + "!"

fn main! {
    let name = "world"
    name say_hello!
}
```
Currently, methods cannot be called on literals.