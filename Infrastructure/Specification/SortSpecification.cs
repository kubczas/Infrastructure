using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Infrastructure.Specification
{
    public class SortSpecification<T>
    {
        public ListSortDirection Direction { get; set; }
        public Expression<Func<T,object>> Predicate { get; set; } 
    }
}
