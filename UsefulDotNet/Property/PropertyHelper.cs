using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Haukcode.UsefulDotNet
{
    public static class PropertyHelper
    {
        public static void Set<T, TProp>(T target, Expression<Func<T, TProp>> memberLamda, TProp sourceValue, ref bool isDirty)
        {
            Set(target, memberLamda, sourceValue, (a, b) =>
            {
                if (b == null && a == null)
                    return true;

                if ((b == null && a != null) || !b.Equals(a))
                    return false;

                return true;
            }, ref isDirty);
        }

        public static void Set<T>(T target, Expression<Func<T, decimal>> memberLamda, decimal sourceValue, int decimals, ref bool isDirty)
        {
            Set(target, memberLamda, Math.Round(sourceValue, decimals), (a, b) => b.Equals(a), ref isDirty);
        }

        public static void Set<T>(T target, Expression<Func<T, string>> memberLamda, string sourceValue, int maxLength, ref bool isDirty)
        {
            if (sourceValue?.Length > maxLength)
                sourceValue = sourceValue.Substring(0, maxLength);

            Set(target, memberLamda, sourceValue, ref isDirty);
        }

        public static void Set<T>(T target, Expression<Func<T, decimal?>> memberLamda, decimal? sourceValue, int decimals, ref bool isDirty)
        {
            Set(target, memberLamda, sourceValue.HasValue ? Math.Round(sourceValue.Value, decimals) : (decimal?)null, (a, b) =>
            {
                if (b == null && a == null)
                    return true;

                if ((b == null && a != null) || !b.Equals(a))
                    return false;

                return true;
            }, ref isDirty);
        }

        public static void Set<T, TProp>(T target, Expression<Func<T, TProp>> memberLamda, TProp sourceValue, Func<TProp, TProp, bool> comparer, ref bool isDirty)
        {
            if (memberLamda.Body is MemberExpression memberSelectorExpression)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null)
                {
                    var targetValue = (TProp)property.GetValue(target);
                    if (!comparer(sourceValue, targetValue))
                    {
                        // Changed
                        property.SetValue(target, sourceValue, null);
                        isDirty = true;
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid property type");
                }
            }
            else
            {
                throw new ArgumentException("Invalid property");
            }
        }
    }
}
