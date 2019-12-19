using System;
using Microsoft.EntityFrameworkCore;

namespace Haukcode.DatabaseUtils
{
    public interface IEnumConverterPopulator
    {
        void AddEnumToCache<TEnum, TData>(
            DbContext db,
            Func<TData, int> idAccessor,
            Func<TData, string> codeAccessor,
            Func<string, TEnum> enumFromCode = null,
            Func<TEnum, string> codeFromEnum = null,
            bool ignoreUnknown = true) where TEnum : struct, IConvertible where TData : class;
    }
}
