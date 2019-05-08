using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DBLinqProvider.Data.Common;

namespace DBLinqProvider.Data.OracleCore
{
    public class PLSqlFormatter : SqlFormatter
    {
        protected PLSqlFormatter(QueryLanguage language)
            : base(language)
        {
        }

        public static new string Format(Expression expression)
        {
            return Format(expression, PLSqlLanguage.Default);
        }

        public static string Format(Expression expression, QueryLanguage language)
        {
            PLSqlFormatter formatter = new PLSqlFormatter(language);
            formatter.Visit(expression);
            return formatter.ToString();
        }
        
        protected override void WriteParameterName(string name)
        {
            this.Write(":");
            this.Write(name);
        }

        protected override void WriteAsAliasName(string aliasName)
        {
            this.Write(aliasName);
        }

        protected override void WriteAsColumnName(string columnName)
        {
            this.Write(columnName);
        }

        protected override void WriteTopClause(Expression expression)
        {
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            if (select.Take != null)
            {
                this.Write("SELECT * FROM (");
            }
            Expression exp = base.VisitSelect(select);
            if (select.Take != null)
            {
                this.Write(") WHERE ROWNUM<=");
                this.Write(select.Take);
            }
            return exp;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Member.DeclaringType == typeof(string))
            {
                switch (m.Member.Name)
                {
                    case "Length":
                        this.Write("LENGTH(");
                        this.Visit(m.Expression);
                        this.Write(")");
                        return m;
                }
            }
            else if (m.Member.DeclaringType == typeof(DateTime) || m.Member.DeclaringType == typeof(DateTimeOffset))
            {
                switch (m.Member.Name)
                {
                    case "Day":
                        this.Write("TO_NUMBER(TO_CHAR(");
                        this.Visit(m.Expression);
                        this.Write(",'DD'))");
                        return m;
                    case "Month":
                        this.Write("TO_NUMBER(TO_CHAR(");
                        this.Visit(m.Expression);
                        this.Write(",'MM'))");
                        return m;
                    case "Year":
                        this.Write("TO_NUMBER(TO_CHAR(");
                        this.Visit(m.Expression);
                        this.Write(",'YYYY'))");
                        return m;
                    case "Hour":
                        this.Write("TO_NUMBER(TO_CHAR(");
                        this.Visit(m.Expression);
                        this.Write(",'HH24'))");
                        return m;
                    case "Minute":
                        this.Write("TO_NUMBER(TO_CHAR(");
                        this.Visit(m.Expression);
                        this.Write(",'MI'))");
                        return m;
                    case "Second":
                        this.Write("TO_NUMBER(TO_CHAR(");
                        this.Visit(m.Expression);
                        this.Write(",'SS'))");
                        return m;
                    case "Millisecond":
                        this.Write("TO_NUMBER(TO_CHAR(");
                        this.Visit(m.Expression);
                        this.Write(",'FF'))");
                        return m;
                    case "DayOfWeek":
                        this.Write("(TO_NUMBER(TO_CHAR(");
                        this.Visit(m.Expression);
                        this.Write(",'D'))-1)");
                        return m;
                    case "DayOfYear":
                        this.Write("TO_NUMBER(TO_CHAR(");
                        this.Visit(m.Expression);
                        this.Write(",'DDD'))");
                        return m;
                }
            }
            return base.VisitMemberAccess(m);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(string))
            {
                switch (m.Method.Name)
                {
                    case "StartsWith":
                        this.Write("(");
                        this.Visit(m.Object);
                        this.Write(" LIKE ");
                        this.Visit(m.Arguments[0]);
                        this.Write(" || '%')");
                        return m;
                    case "EndsWith":
                        this.Write("(");
                        this.Visit(m.Object);
                        this.Write(" LIKE '%' || ");
                        this.Visit(m.Arguments[0]);
                        this.Write(")");
                        return m;
                    case "Contains":
                        this.Write("(");
                        this.Visit(m.Object);
                        this.Write(" LIKE '%' || ");
                        this.Visit(m.Arguments[0]);
                        this.Write(" || '%')");
                        return m;
                    case "Concat":
                        IList<Expression> args = m.Arguments;
                        if (args.Count == 1 && args[0].NodeType == ExpressionType.NewArrayInit)
                        {
                            args = ((NewArrayExpression)args[0]).Expressions;
                        }
                        for (int i = 0, n = args.Count; i < n; i++)
                        {
                            if (i > 0) this.Write(" || ");
                            this.Visit(args[i]);
                        }
                        return m;
                    case "IsNullOrEmpty":
                        this.Write("(");
                        this.Visit(m.Arguments[0]);
                        this.Write(" IS NULL)");
                        return m;
                    case "ToUpper":
                        this.Write("UPPER(");
                        this.Visit(m.Object);
                        this.Write(")");
                        return m;
                    case "ToLower":
                        this.Write("LOWER(");
                        this.Visit(m.Object);
                        this.Write(")");
                        return m;
                    case "Replace":
                        this.Write("REPLACE(");
                        this.Visit(m.Object);
                        this.Write(", ");
                        this.Visit(m.Arguments[0]);
                        this.Write(", ");
                        this.Visit(m.Arguments[1]);
                        this.Write(")");
                        return m;
                    case "Substring":
                        this.Write("SUBSTR(");
                        this.Visit(m.Object);
                        this.Write(", ");
                        this.Visit(m.Arguments[0]);
                        this.Write(" + 1, ");
                        if (m.Arguments.Count == 2)
                        {
                            this.Visit(m.Arguments[1]);
                        }
                        else
                        {
                            this.Write("8000");
                        }
                        this.Write(")");
                        return m;
                    case "Remove":
                        this.Write("(SUBSTR(");
                        this.Visit(m.Object);
                        this.Write(",1,");
                        this.Visit(m.Arguments[0]);
                        this.Write(") || SUBSTR(");
                        this.Visit(m.Object);
                        this.Write(",");
                        this.Visit(m.Arguments[0]);
                        this.Write("+1+");
                        if (m.Arguments.Count == 2)
                        {
                            this.Visit(m.Arguments[1]);
                        }
                        else
                        {
                            this.Write("8000");
                        }
                        this.Write("))");
                        return m;
                    case "IndexOf":
                        this.Write("(INSTR(");
                        this.Visit(m.Object);
                        this.Write(", ");
                        this.Visit(m.Arguments[0]);
                        if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write(", ");
                            this.Visit(m.Arguments[1]);
                        }
                        this.Write(") - 1)");
                        return m;
                    case "Trim":
                        this.Write("TRIM(");
                        this.Visit(m.Object);
                        this.Write(")");
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(DateTime))
            {
                switch (m.Method.Name)
                {
                    case "op_Subtract":
                        if (m.Arguments[1].Type == typeof(DateTime))
                        {
                            this.Write("(");
                            this.Visit(m.Arguments[0]);
                            this.Write("-");
                            this.Visit(m.Arguments[1]);
                            this.Write(")");
                            return m;
                        }
                        break;
                    case "AddYears":
                        this.Write("(");
                        this.Visit(m.Object);
                        this.Write("+");
                        this.Visit(m.Arguments[0]);
                        this.Write("*NUMTOYMINTERVAL(1,'YEAR'))");
                        return m;
                    case "AddMonths":
                        this.Write("(");
                        this.Visit(m.Object);
                        this.Write("+");
                        this.Visit(m.Arguments[0]);
                        this.Write("*NUMTOYMINTERVAL(1,'MONTH'))");
                        return m;
                    case "AddDays":
                        this.Write("(");
                        this.Visit(m.Object);
                        this.Write("+");
                        this.Visit(m.Arguments[0]);
                        this.Write("*NUMTODSINTERVAL(1,'DAY'))");
                        return m;
                    case "AddHours":
                        this.Write("(");
                        this.Visit(m.Object);
                        this.Write("+");
                        this.Visit(m.Arguments[0]);
                        this.Write("*NUMTODSINTERVAL(1,'HOUR'))");
                        return m;
                    case "AddMinutes":
                        this.Write("(");
                        this.Visit(m.Object);
                        this.Write("+");
                        this.Visit(m.Arguments[0]);
                        this.Write("*NUMTODSINTERVAL(1,'MINUTE'))");
                        return m;
                    case "AddSeconds":
                        this.Write("(");
                        this.Visit(m.Object);
                        this.Write("+");
                        this.Visit(m.Arguments[0]);
                        this.Write("*NUMTODSINTERVAL(1,'SECOND'))");
                        return m;
                    case "AddMilliseconds":
                        this.Write("(");
                        this.Visit(m.Object);
                        this.Write("+");
                        this.Visit(m.Arguments[0]);
                        this.Write("*NUMTODSINTERVAL(0.001,'SECOND'))");
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(Decimal))
            {
                switch (m.Method.Name)
                {
                    case "Remainder":
                        this.Write("MOD(");
                        this.VisitValue(m.Arguments[0]);
                        this.Write(",");
                        this.VisitValue(m.Arguments[1]);
                        this.Write(")");
                        return m;
                    case "Ceiling":
                        this.Write("CEIL(");
                        this.Visit(m.Arguments[0]);
                        this.Write(")");
                        return m;
                    case "Floor":
                        this.Write("FLOOR(");
                        this.Visit(m.Arguments[0]);
                        this.Write(")");
                        return m;
                    case "Round":
                        if (m.Arguments.Count == 1)
                        {
                            this.Write("ROUND(");
                            this.Visit(m.Arguments[0]);
                            this.Write(", 0)");
                            return m;
                        }
                        else if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write("ROUND(");
                            this.Visit(m.Arguments[0]);
                            this.Write(", ");
                            this.Visit(m.Arguments[1]);
                            this.Write(")");
                            return m;
                        }
                        break;
                    case "Truncate":
                        this.Write("TRUNC(");
                        this.Visit(m.Arguments[0]);
                        this.Write(")");
                        return m;
                }
                switch (m.Method.Name)
                {
                    case "Add":
                    case "Subtract":
                    case "Multiply":
                    case "Divide":
                        this.Write("(");
                        this.VisitValue(m.Arguments[0]);
                        this.Write(" ");
                        this.Write(GetOperator(m.Method.Name));
                        this.Write(" ");
                        this.VisitValue(m.Arguments[1]);
                        this.Write(")");
                        return m;
                    case "Negate":
                        this.Write("-");
                        this.Visit(m.Arguments[0]);
                        this.Write("");
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(Math))
            {
                switch (m.Method.Name)
                {
                    case "Abs":
                    case "Acos":
                    case "Asin":
                    case "Atan":
                    case "Cos":
                    case "Exp":                    
                    case "Sin":
                    case "Tan":
                    case "Sqrt":
                    case "Sign":
                        this.WriteTruncMaxDecimalDigitsStart();
                        this.Write(m.Method.Name.ToUpper());
                        this.Write("(");
                        this.Visit(m.Arguments[0]);
                        this.Write(")");
                        this.WriteTruncMaxDecimalDigitsEnd();
                        return m;
                    case "Floor":
                        this.Write("FLOOR(");
                        this.Visit(m.Arguments[0]);
                        this.Write(")");
                        return m;
                    case "Ceiling":
                        this.Write("CEIL(");
                        this.Visit(m.Arguments[0]);
                        this.Write(")");
                        return m;
                    case "Atan2":
                        this.WriteTruncMaxDecimalDigitsStart();
                        this.Write("ATAN2(");
                        this.Visit(m.Arguments[0]);
                        this.Write(", ");
                        this.Visit(m.Arguments[1]);
                        this.Write(")");
                        this.WriteTruncMaxDecimalDigitsEnd();
                        return m;
                    case "Log10":
                        this.WriteTruncMaxDecimalDigitsStart();
                        this.Write("LOG(");
                        this.Write("10,");
                        this.Visit(m.Arguments[0]);
                        this.Write(")");
                        this.WriteTruncMaxDecimalDigitsEnd();
                        return m;
                    case "Log":
                        this.WriteTruncMaxDecimalDigitsStart();
                        this.Write("LN(");
                        this.Visit(m.Arguments[0]);
                        this.Write(")");
                        this.WriteTruncMaxDecimalDigitsEnd();
                        return m;
                    //case "Log":
                    //    this.WriteTruncMaxDecimalDigitsStart();
                    //    this.Write("LOG(");
                    //    if (m.Arguments.Count == 1)
                    //    {
                    //        this.Write("10,");
                    //        this.Visit(m.Arguments[0]);
                    //    }
                    //    else
                    //    {
                    //        this.Visit(m.Arguments[0]);
                    //        this.Write(", ");
                    //        this.Visit(m.Arguments[1]);
                    //    }
                    //    this.Write(")");
                    //    this.WriteTruncMaxDecimalDigitsEnd();
                    //    return m;
                    case "Pow":
                        this.WriteTruncMaxDecimalDigitsStart();
                        this.Write("POWER(");
                        this.Visit(m.Arguments[0]);
                        this.Write(", ");
                        this.Visit(m.Arguments[1]);
                        this.Write(")");
                        this.WriteTruncMaxDecimalDigitsEnd();
                        return m;
                    case "Round":
                        if (m.Arguments.Count == 1)
                        {
                            this.Write("ROUND(");
                            this.Visit(m.Arguments[0]);
                            this.Write(", 0)");
                            return m;
                        }
                        else if (m.Arguments.Count == 2 && m.Arguments[1].Type == typeof(int))
                        {
                            this.Write("ROUND(");
                            this.Visit(m.Arguments[0]);
                            this.Write(", ");
                            this.Visit(m.Arguments[1]);
                            this.Write(")");
                            return m;
                        }
                        break;
                    case "Truncate":
                        this.Write("TRUNC(");
                        this.Visit(m.Arguments[0]);
                        this.Write(")");
                        return m;
                }
            }
            if (m.Method.Name == "ToString")
            {
                if (m.Object.Type != typeof(string))
                {
                    this.Write("TO_CHAR(");
                    this.Visit(m.Object);
                    this.Write(")");
                }
                else
                {
                    this.Visit(m.Object);
                }
                return m;
            }
            else if (!m.Method.IsStatic && m.Method.Name == "CompareTo" && m.Method.ReturnType == typeof(int) && m.Arguments.Count == 1)
            {
                this.Write("(CASE WHEN ");
                this.Visit(m.Object);
                this.Write(" = ");
                this.Visit(m.Arguments[0]);
                this.Write(" THEN 0 WHEN ");
                this.Visit(m.Object);
                this.Write(" < ");
                this.Visit(m.Arguments[0]);
                this.Write(" THEN -1 ELSE 1 END)");
                return m;
            }
            else if (m.Method.IsStatic && m.Method.Name == "Compare" && m.Method.ReturnType == typeof(int) && m.Arguments.Count == 2)
            {
                this.Write("(CASE WHEN ");
                this.Visit(m.Arguments[0]);
                this.Write(" = ");
                this.Visit(m.Arguments[1]);
                this.Write(" THEN 0 WHEN ");
                this.Visit(m.Arguments[0]);
                this.Write(" < ");
                this.Visit(m.Arguments[1]);
                this.Write(" THEN -1 ELSE 1 END)");
                return m;
            }
            return base.VisitMethodCall(m);
        }

        protected override NewExpression VisitNew(NewExpression nex)
        {
            if (nex.Constructor.DeclaringType == typeof(DateTime))
            {
                if (nex.Arguments.Count == 3)
                {
                    this.Write("TO_DATE((");
                    this.Visit(nex.Arguments[0]);
                    this.Write(") || '-' || (");
                    this.Visit(nex.Arguments[1]);
                    this.Write(") || '-' || (");
                    this.Visit(nex.Arguments[2]);
                    this.Write("),'YYYY-MM-DD')");
                    return nex;
                }
                else if (nex.Arguments.Count == 6)
                {
                    this.Write("TO_TIMESTAMP((");
                    this.Visit(nex.Arguments[0]);
                    this.Write(") || '-' || (");
                    this.Visit(nex.Arguments[1]);
                    this.Write(") || '-' || (");
                    this.Visit(nex.Arguments[2]);
                    this.Write(") || ' ' || (");
                    this.Visit(nex.Arguments[3]);
                    this.Write(") || ':' || (");
                    this.Visit(nex.Arguments[4]);
                    this.Write(") || ':' || (");
                    this.Visit(nex.Arguments[5]);
                    this.Write("),'YYYY-MM-DD HH24:MI:SS')");
                    return nex;
                }
            }
            return base.VisitNew(nex);
        }

        protected override Expression VisitValue(Expression expr)
        {
            if (IsPredicate(expr))
            {
                this.Write("CASE WHEN (");
                this.Visit(expr);
                this.Write(") THEN 1 ELSE 0 END from dual");
                return expr;
            }
            return base.VisitValue(expr);
        } 

        protected override Expression VisitRowNumber(RowNumberExpression rowNumber)
        {
            this.Write("ROW_NUMBER() OVER(");
            if (rowNumber.OrderBy != null && rowNumber.OrderBy.Count > 0)
            {
                this.Write("ORDER BY ");
                for (int i = 0, n = rowNumber.OrderBy.Count; i < n; i++)
                {
                    OrderExpression exp = rowNumber.OrderBy[i];
                    if (i > 0)
                    {
                        this.Write(", ");
                    }
                    this.VisitValue(exp.Expression);
                    if (exp.OrderType != OrderType.Ascending)
                    {
                        this.Write(" DESC");
                    }
                }
            }
            this.Write(")");
            return rowNumber;
        }

        protected override Expression VisitIf(IFCommand ifx)
        {
            if (!this.Language.AllowsMultipleCommands)
            {
                return base.VisitIf(ifx);
            }
            this.Write("IF ");
            this.Visit(ifx.Check);
            this.WriteLine(Indentation.Same);
            this.Write("THEN BEGIN");
            this.WriteLine(Indentation.Inner);
            this.VisitStatement(ifx.IfTrue);
            this.WriteLine(Indentation.Outer);
            if (ifx.IfFalse != null)
            {
                this.Write("ELSE BEGIN");
                this.WriteLine(Indentation.Inner);
                this.VisitStatement(ifx.IfFalse);
                this.WriteLine(Indentation.Outer);
            }
            this.Write("END IF;");
            return ifx;
        }

        /*
        protected override Expression VisitBlock(BlockCommand block)
        {
            if (!this.Language.AllowsMultipleCommands)
            {
                return base.VisitBlock(block);
            }

            this.Write("DECLARE");
            this.WriteLine(Indentation.Same);
            for (int i = 0, n = block.Commands.Count; i < n; i++)
            {
                if (block.Commands[i] is DeclarationCommand)
                {
                    DeclarationCommand decl = (DeclarationCommand)block.Commands[i];
                    if (decl.Variables.Count > 0)
                    {
                        this.WriteLine(Indentation.Same);
                        for (int i1 = 0, n1 = decl.Variables.Count; i1 < n1; i1++)
                        {
                            var v = decl.Variables[i1];
                            this.Write(v.Name);
                            this.Write(" ");
                            this.Write(this.Language.TypeSystem.GetVariableDeclaration(v.QueryType, false));
                            this.Write(";");
                            this.WriteLine(Indentation.Same);
                        }
                    }
                }
            }

            this.Write("BEGIN");
            this.WriteLine(Indentation.Same);
            for (int i = 0, n = block.Commands.Count; i < n; i++)
            {
                this.WriteLine(Indentation.Same);
                this.VisitStatement(block.Commands[i]);
                this.Write(";");
            }

            this.WriteLine(Indentation.Same);
            this.Write("END;");
            this.WriteLine(Indentation.Same);
            return block;
        }

        protected override Expression VisitDeclaration(DeclarationCommand decl)
        {
            if (!this.Language.AllowsMultipleCommands)
            {
                return base.VisitDeclaration(decl);
            }
            if (decl.Source != null)
            {
                this.WriteLine(Indentation.Same);
                this.Write("SELECT ");
                for (int i = 0, n = decl.Variables.Count; i < n; i++)
                {
                    if (i > 0)
                        this.Write(", ");
                    this.Visit(decl.Source.Columns[i].Expression);
                    this.Write(" INTO V_");
                    this.Write(decl.Variables[i].Name);
                }
                if (decl.Source.From != null)
                {
                    this.WriteLine(Indentation.Same);
                    this.Write("FROM ");
                    this.VisitSource(decl.Source.From);
                }
                if (decl.Source.Where != null)
                {
                    this.WriteLine(Indentation.Same);
                    this.Write("WHERE ");
                    this.Visit(decl.Source.Where);
                }
            }
            else
            {
                for (int i = 0, n = decl.Variables.Count; i < n; i++)
                {
                    var v = decl.Variables[i];
                    if (v.Expression != null)
                    {
                        this.WriteLine(Indentation.Same);
                        this.Write("V_");
                        this.Write(v.Name);
                        this.Write(" = ");
                        this.Visit(v.Expression);
                    }
                }
            }
            return decl;
        }
        */

        protected override Expression VisitBinary(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    if (this.IsBoolean(b.Left.Type))
                    {
                        return base.VisitBinary(b);
                    }
                    else
                    {
                        this.Write("BITAND(");
                        this.VisitValue(b.Left);
                        this.Write(",");
                        this.VisitValue(b.Right);
                        this.Write(")");
                        break;            
                    }
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    if (this.IsBoolean(b.Left.Type))
                    {
                        return base.VisitBinary(b);
                    }
                    else
                    {
                        //OR = x-bitand(x,y)+y
                        this.Write("(");
                        this.VisitValue(b.Left);
                        this.Write("-BITAND(");
                        this.VisitValue(b.Left);
                        this.Write(",");
                        this.VisitValue(b.Right);
                        this.Write(")+");
                        this.VisitValue(b.Right);
                        this.Write(")");
                        break;
                    }
                case ExpressionType.ExclusiveOr:
                    //XOR: x-2*bitand(x,y)+y
                    this.Write("(");
                    this.VisitValue(b.Left);
                    this.Write("-2*BITAND(");
                    this.VisitValue(b.Left);
                    this.Write(",");
                    this.VisitValue(b.Right);
                    this.Write(")+");
                    this.VisitValue(b.Right);
                    this.Write(")");
                    break;
                case ExpressionType.Modulo:
                    this.Write("MOD(");
                    this.VisitValue(b.Left);
                    this.Write(",");
                    this.VisitValue(b.Right);
                    this.Write(")");
                    break;
                case ExpressionType.LeftShift:
                    this.Write("(");
                    this.VisitValue(b.Left);
                    this.Write("*POWER(2,");
                    this.VisitValue(b.Right);
                    this.Write("))");
                    break;
                case ExpressionType.RightShift:
                    this.Write("(");
                    this.VisitValue(b.Left);
                    this.Write("/POWER(2,");
                    this.VisitValue(b.Right);
                    this.Write("))");
                    break;
                case ExpressionType.Coalesce:
                    this.Write("COALESCE(");
                    this.VisitValue(b.Left);
                    this.Write(", ");
                    Expression right = b.Right;
                    while (right.NodeType == ExpressionType.Coalesce)
                    {
                        BinaryExpression rb = (BinaryExpression)right;
                        this.VisitValue(rb.Left);
                        this.Write(", ");
                        right = rb.Right;
                    }
                    this.VisitValue(right);
                    this.Write(")");
                    break;
                case ExpressionType.Divide:
                    if (IsInteger(b.Left.Type) && IsInteger(b.Right.Type))
                    {
                        this.Write("TRUNC(");
                        base.VisitBinary(b);
                        this.Write(")");
                    }
                    else
                    {
                        base.VisitBinary(b);
                    }
                    break;
                default:
                    return base.VisitBinary(b);
            }
            return b;
        }

        protected override string GetOperator(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    if (b.Left.Type.Equals(typeof(String)) || b.Right.Type.Equals(typeof(String)))
                    {
                        return "||";
                    }
                    return base.GetOperator(b);
                default:
                    return base.GetOperator(b);
            }
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    string op = this.GetOperator(u);
                    if (IsBoolean(u.Operand.Type) || op.Length > 1)
                    {
                        this.Write(op);
                        this.Write(" ");
                        this.VisitPredicate(u.Operand);
                    }
                    else
                    {
                        //NOT: -1-x
                        this.Write("(-1-");
                        this.VisitValue(u.Operand);
                        this.Write(")");
                    }
                    break;
                default:
                    return base.VisitUnary(u);
            }
            return u;
        }

        protected override Expression VisitConditional(ConditionalExpression c)
        {
            if (this.IsPredicate(c.Test))
            {
                this.Write("(CASE WHEN ");
                this.VisitPredicate(c.Test);
                this.Write(" THEN ");
                this.VisitValue(c.IfTrue);
                Expression ifFalse = c.IfFalse;
                while (ifFalse != null && ifFalse.NodeType == ExpressionType.Conditional)
                {
                    ConditionalExpression fc = (ConditionalExpression)ifFalse;
                    this.Write(" WHEN ");
                    this.VisitPredicate(fc.Test);
                    this.Write(" THEN ");
                    this.VisitValue(fc.IfTrue);
                    ifFalse = fc.IfFalse;
                }
                if (ifFalse != null)
                {
                    this.Write(" ELSE ");
                    this.VisitValue(ifFalse);
                }
                this.Write(" END)");
            }
            else
            {
                this.Write("(CASE ");
                this.VisitValue(c.Test);
                this.Write(" WHEN 0 THEN ");
                this.VisitValue(c.IfFalse);
                this.Write(" ELSE ");
                this.VisitValue(c.IfTrue);
                this.Write(" END)");
            }
            return c;
        }

        protected override Expression VisitAggregate(AggregateExpression aggregate)
        {
            switch (aggregate.AggregateName)
            {
                case "Average":
                    this.WriteTruncMaxDecimalDigitsStart();
                    base.VisitAggregate(aggregate);
                    this.WriteTruncMaxDecimalDigitsEnd();
                    break;
                default:
                    return base.VisitAggregate(aggregate);
            }
            return aggregate;
        }

        //these two functions are to get around OracleClient issue: OCI-22053: overflow error
        void WriteTruncMaxDecimalDigitsStart()
        {
            this.Write("TRUNC(");
        }
        void WriteTruncMaxDecimalDigitsEnd()
        {
            const int MaxDecimalDigits = 20;
            this.Write(",");
            this.Write(MaxDecimalDigits);
            this.Write(")");
        }
        
    } 
}
