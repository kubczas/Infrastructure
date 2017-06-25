using System;
using System.Linq.Expressions;

namespace Infrastructure.Specification
{
    public abstract class CompositeSpecification<T> : ISpecification<T>
    {
        public abstract Expression<Func<T, bool>> IsSatisfiedBy();

        public ISpecification<T> And(ISpecification<T> other)
        {
            return new AddSpecification<T>(this, other);
        }

        public ISpecification<T> Or(ISpecification<T> other)
        {
            return new OrSpecification<T>(this, other);
        }

        public ISpecification<T> Not()
        {
            return new NotSpecification<T>(this);
        }
    }
}
