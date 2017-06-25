﻿using System;
using System.Linq.Expressions;

namespace Infrastructure.Specification
{
    public class OrSpecification<T> : CompositeSpecification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public OrSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, bool>> IsSatisfiedBy()
        {
            var expr1 = _left.IsSatisfiedBy();
            var expr2 = _right.IsSatisfiedBy();
            var parameter = expr1.Parameters[0];
            return Expression.Lambda<Func<T, bool>>(ReferenceEquals(parameter, expr2.Parameters[0]) 
                ? Expression.OrElse(expr1.Body, expr2.Body) 
                : Expression.OrElse(expr1.Body, Expression.Invoke(expr2, parameter)), parameter);
        }
    }
}
