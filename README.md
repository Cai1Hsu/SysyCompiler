# SysyCompiler

A compiler implementation for the SysY programming language, developed as part of a compiler principles course project.

## Overview

This project is my coursework assignment for the Compiler Principles course. Unlike traditional implementations, this compiler features a **redesigned grammar system** that provides enhanced flexibility and maintainability while maintaining compatibility with the original SysY specification through an adaptation layer.

## Key Features

### 1. Flattened Grammar Design

The compiler **rewrites the original grammar** by:

- **Eliminating nested expressions**: Using flattened syntax nodes for better parsing and analysis
- **Removing special cases**: No hardcoded special handling for identifiers like `printf`, `getint`, `main`, etc.
- **More generic grammar**: Provides a foundation for:
  - Custom type definitions
  - More complex expression combinations
  - Extensible language features

### 2. Online Judge (OJ) Adapter

To meet assignment grading requirements, an **OJ adaptation layer** is provided that:

- Restores syntax nodes to match the original grammar specification during semantic analysis
- Ensures compatibility with automated testing systems
- Bridges the gap between the enhanced internal representation and expected output format

## Original SysY Grammar Specification

<details>
<summary>Click to expand the complete SysY grammar</summary>

The SysY language grammar is defined as follows, with `CompUnit` as the start symbol:

| No. | Meaning | LHS | Symbol | RHS | Notes |
|-----|---------|-----|--------|-----|-------|
| 1 | Compilation Unit | CompUnit | → | {Decl} {FuncDef} MainFuncDef | 1. Decl existence 2. FuncDef existence |
| 2 | Declaration | Decl | → | ConstDecl \| VarDecl | Covers both declaration types |
| 3 | Function Definition | FuncDef | → | FuncType Ident '(' [FuncFParams] ')' Block | 1. No parameters 2. With parameters |
| 4 | Main Function Definition | MainFuncDef | → | 'int' 'main' '(' ')' Block | Main function exists |
| 5 | Constant Declaration | ConstDecl | → | 'const' BType ConstDef { ',' ConstDef } ';' | Braces repeat 0 or more times, single quotes denote terminals |
| 6 | Variable Declaration | VarDecl | → | BType VarDef { ',' VarDef } ';' | Braces repeat 0 or more times |
| 7 | Function Type | FuncType | → | 'void' \| 'int' \| 'float' | Covers function types |
| 8 | Identifier | Ident | → | identifier-nondigit \| identifier identifier-nondigit \| identifier digit | Lexical analysis |
| 9 | Function Parameters | FuncFParams | → | FuncFParam { ',' FuncFParam } | Braces repeat 0 or more times |
| 10 | Block | Block | → | '{' { BlockItem } '}' | Braces repeat 0 or more times |
| 11 | Basic Type | BType | → | 'int' \| 'float' | Existence check |
| 12 | Variable Definition | VarDef | → | Ident { '[' ConstExp ']' } \| Ident { '[' ConstExp ']' } '=' InitVal | Covers variables, 1D arrays, 2D arrays |
| 13 | Function Parameter | FuncFParam | → | BType Ident ['[' ']' { '[' ConstExp ']' }] | 1. Normal variable 2. 1D array 3. 2D array |
| 14 | Block Item | BlockItem | → | Decl \| Stmt | Covers both statement types |
| 15 | Constant Expression | ConstExp | → | AddExp | Note: Used Ident must be constant |
| 16 | Variable Initial Value | InitVal | → | Exp \| '{' [ InitVal { ',' InitVal } ] '}' | 1. Expression 2. 1D array 3. 2D array |
| 17 | Statement | Stmt | → | LVal '=' Exp ';' \| [Exp] ';' \| Block<br>\| 'if' '(' Cond ')' Stmt ['else' Stmt]<br>\| 'while' '(' Cond ')' Stmt<br>\| 'break' ';' \| 'continue' ';'<br>\| 'return' [Exp] ';' \| LVal '=' 'getint''('')'';'<br>\| 'printf''('FormatString{','Exp}')'';' | Multiple statement types to cover |
| 18 | Additive Expression | AddExp | → | MulExp \| AddExp ('+' \| '−') MulExp | 1. MulExp 2. + required 3. - required |
| 19 | Expression | Exp | → | AddExp | Note: SysY expressions are int type |
| 20 | L-value Expression | LVal | → | Ident {'[' Exp ']'} | 1. Normal variable 2. 1D array 3. 2D array |
| 21 | Condition Expression | Cond | → | LOrExp | Existence check |
| 22 | Format String Terminal | FormatString | → | '"'{&lt;Char&gt;}'"' | Lexical analysis |
| 23 | Multiplicative Expression | MulExp | → | UnaryExp \| MulExp ('*' \| '/' \| '%') UnaryExp | Must cover: 1. UnaryExp 2. * 3. / 4. % |
| 24 | Logical OR Expression | LOrExp | → | LAndExp \| LOrExp '\|\|' LAndExp | Must cover: 1. LAndExp 2. \|\| |
| 25 | Logical AND Expression | LAndExp | → | EqExp \| LAndExp '&&' EqExp | Must cover: 1. EqExp 2. && |
| 26 | Equality Expression | EqExp | → | RelExp \| EqExp ('==' \| '!=') RelExp | Must cover: 1. RelExp 2. == 3. != |
| 27 | Unary Expression | UnaryExp | → | PrimaryExp \| Ident '(' [FuncRParams] ')' \| UnaryOp UnaryExp | All 3 cases must be covered, including function calls |
| 28 | Relational Expression | RelExp | → | AddExp \| RelExp ('&lt;' \| '&gt;' \| '&lt;=' \| '&gt;=') AddExp | Must cover: 1. AddExp 2. &lt; 3. &gt; 4. &lt;= 5. &gt;= |
| 29 | Character | &lt;Char&gt; | → | &lt;FormatChar&gt; \| &lt;NormalChar&gt; | Lexical analysis |
| 30 | Format Character | &lt;FormatChar&gt; | → | %d | Lexical analysis |
| 31 | Normal Character | &lt;NormalChar&gt; | → | ASCII characters with decimal codes 32, 33, 40-126, '\\' (code 92) appears only in '\\n' | Lexical analysis: For consistency with gcc, escape sequences for &lt;NormalChar&gt; are only '\\n', '\\' appearing alone is illegal |
| 32 | Primary Expression | PrimaryExp | → | '(' Exp ')' \| LVal \| Number | All 3 cases must be covered |
| 33 | Function Arguments | FuncRParams | → | Exp { ',' Exp } | 1. Braces repeat 0 times 2. Multiple times 3. Exp covers array and partial array passing |
| 34 | Number | Number | → | IntConst | Existence check |
| 35 | Integer Constant | IntConst | → | decimal-const \| 0 | Lexical analysis |
| 36 | Decimal Constant | decimal-const | → | nonzero-digit \| decimal-const digit | Lexical analysis |
| 37 | Non-zero Digit | nonzero-digit | → | 1\|2\|3\|4\|5\|6\|7\|8\|9 | Lexical analysis |
| 38 | Unary Operator | UnaryOp | → | '+' \| '-' \| '!' | '!' only appears in conditional expressions |
| 39 | Constant Initial Value | ConstInitVal | → | ConstExp \| '{'[ConstInitVal{','ConstInitVal}] '}' | - |
| 40 | Constant Definition | ConstDef | → | Ident{'['ConstExp']'} \| Ident{'['ConstExp']'}'='ConstInitVal | - |

</details>

## Project Structure

```plaintext
SysyCompiler/
├── SysyCompiler/              # Main compiler entry point
├── SysyCompiler.Frontend/     # Lexical analysis and parsing
│   ├── Syntax/                # Syntax node definitions
│   └── Tokenization/          # Tokenizer implementation
├── SysyCompiler.Analyzer/     # Semantic analysis and OJ adapter
│   ├── OnlineJudgeAdapter.cs  # OJ compatibility layer
│   └── VirtualNode.cs         # Virtual AST nodes
└── SysyCompiler.*.Tests/      # Unit tests
```

## Building the Project

### Prerequisites

- .NET 8.0 SDK or later
- (Optional) bflat for native compilation

### Build with dotnet

```powershell
dotnet build SysyCompiler/SysyCompiler.csproj -c Release
```

### Build with bflat (Native Binary)

This is useful to produce a standalone executable that can be submitted to OJ platforms(they don't support .NET!).

```powershell
.\deploy.ps1
```

### Custom build options

```powershell
# Specify configuration and output
.\deploy.ps1 -Configuration Release --no-reflection

# Pass additional bflat arguments
.\deploy.ps1 --verbose --os linux
```

## Usage

This project serves as a library, and is not intended to be used as a standalone compiler. Refer to SysyCompiler/Program.cs and other tests for usage examples.

## Architecture Highlights

### Flattened AST

Instead of deeply nested expression trees, the compiler uses a flat representation that:

- Simplifies traversal and transformation
- Reduces recursive complexity
- Improves maintainability

### Generic Symbol Resolution

The compiler treats all identifiers uniformly, without special cases for built-in functions. This allows:

- Easy extension with new built-in functions
- Better separation of concerns
- More consistent error handling

### OJ Adapter Pattern

The adaptation layer acts as a translator between:

- **Internal representation**: Modern, flexible, flattened grammar
- **External interface**: Original SysY grammar for compatibility with course requirements

This design allows the compiler to be both extensible and compliant with the assignment specifications.

## Testing

```powershell
# Run all tests
dotnet test

# Run specific test project
dotnet test SysyCompiler.Frontend.Tests
```

## License

See [LICENSE](LICENSE) file for details.

## Course Information

This project is developed as coursework for a Compiler Principles course, demonstrating:

- Lexical analysis and tokenization
- Syntax parsing (recursive descent)
- Semantic analysis
- Grammar design and optimization
- Compatibility layer implementation

---

**Note**: The flattened grammar and OJ adapter approach represents an engineering trade-off between assignment requirements and software design principles, showcasing the ability to balance practical constraints with good architectural decisions.
