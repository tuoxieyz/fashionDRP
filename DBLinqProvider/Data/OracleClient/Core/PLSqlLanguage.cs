using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DBLinqProvider.Data.OracleCore
{
    using DBLinqProvider.Data.Common;

    /// <summary>
    /// TSQL specific QueryLanguage
    /// </summary>
    public class PLSqlLanguage : QueryLanguage
    {
        DbTypeSystem typeSystem = new DbTypeSystem();

        public PLSqlLanguage()
        {
        }

        public override QueryTypeSystem TypeSystem
        {
            get { return this.typeSystem; }
        }

        /// <summary>
        /// Don't use lower case name in oracle, it's nightmare.
        /// And if you don't use special character in name,e.g. space, just comment Quote function
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string Quote(string name)
        {
            name = name.ToUpper();
            if (name.StartsWith("\"") && name.EndsWith("\""))
            {
                return name;
            }
            else if (name.IndexOf('.') > 0)
            {
                return "\"" + string.Join("\".\"", name.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)) + "\"";
            }
            else
            {
                return "\"" + name + "\"";
            }
        }

        private static readonly char[] splitChars = new char[] { '.' };

        public override bool AllowsMultipleCommands
        {
            get { return false; }
        }

        public override bool AllowSubqueryInSelectWithoutFrom
        {
            get { return true; }
        }

        public override bool AllowDistinctInAggregates
        {
            get { return true; }
        }

        public override Expression GetGeneratedIdExpression(MemberInfo member)
        {
            return new FunctionExpression(TypeHelper.GetMemberType(member), "NEXTID.CURRVAL", null);
            //throw new NotSupportedException("GetGeneratedIdExpression not supported.");
        }

        public override QueryLinguist CreateLinguist(QueryTranslator translator)
        {
            return new PLSqlLinguist(this, translator);
        }

        class PLSqlLinguist : QueryLinguist
        {
            public PLSqlLinguist(PLSqlLanguage language, QueryTranslator translator)
                : base(language, translator)
            {
            }

            public override Expression Translate(Expression expression)
            {
                // fix up any order-by's
                expression = OrderByRewriter.Rewrite(this.Language, expression);

                expression = base.Translate(expression);

                // convert skip/take info into RowNumber pattern
                expression = SkipToRowNumberRewriter.Rewrite(this.Language, expression);

                // fix up any order-by's we may have changed
                expression = OrderByRewriter.Rewrite(this.Language, expression);

                return expression;
            }

            public override string Format(Expression expression)
            {
                return PLSqlFormatter.Format(expression, this.Language);
            }
        }

        private static PLSqlLanguage _default;

        public static PLSqlLanguage Default
        {
            get
            {
                if (_default == null)
                {
                    System.Threading.Interlocked.CompareExchange(ref _default, new PLSqlLanguage(), null);
                }
                return _default;
            }
        }
    }
}
