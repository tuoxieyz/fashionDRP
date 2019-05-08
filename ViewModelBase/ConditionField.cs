using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ViewModelBasic
{
    public class ConditionField
    {
        public string DisplayName { get; set; }

        public string PropertyName { get; set; }

        //public abstract Expression CreateConditionExpression(LambdaExpression otherCondition);
    }

    public class ConditionField<TValue> : ConditionField
    {
        public TValue Value { get; set; }
    }

    //public class ConditionField<TSource, TValue> : ConditionField
    //{
    //    public TValue Value { get; set; }

    //    public override Expression CreateConditionExpression(LambdaExpression otherCondition)
    //    {
    //        ParameterExpression o = Expression.Parameter(typeof(TSource), "o");
    //        var property = Expression.Property(o, PropertyName);
    //        Expression equalExpr = Expression.Equal(property, Expression.Constant(Value, typeof(TValue)));
    //        Expression condition = Expression.AndAlso(otherCondition.Body, equalExpr);            
    //        return  Expression.Lambda<Func<TSource, bool>>(condition, o);
    //    }
    //}
}
