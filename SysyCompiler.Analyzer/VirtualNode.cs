using SysyCompiler.Frontend.Syntax;

namespace SysyCompiler.Analyzer;

public enum VirtualNode
{
    CompUnit,
    MainFuncDef,
    FuncDef,
    FuncFParams,
    FuncFParam,
    ConstDecl,
    VarDecl,
    ConstDef,
    VarDef,
    InitVal,
    Block,
    Stmt,
    Exp,
    Cond,
    LVal,
    PrimaryExp,
    Number,
    UnaryExp,
    UnaryOp,
    FuncRParams,
    MulExp,
    AddExp,
    RelExp,
    EqExp,
    LAndExp,
    LOrExp,
    ConstExp,
    FuncType,
    ConstInitVal,
}
