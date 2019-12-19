using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Haukcode.DatabaseUtils
{
    public class EnumConverter : IEnumConverter, IEnumConverterPopulator
    {
        private readonly Func<DbContext> getDbContext;
        private readonly Action<IEnumConverterPopulator, DbContext> populateEnumCacheMethod;
        private readonly Action<string, string, string> logWarningAction;
        private readonly Action<string, string, string> logErrorAction;
        private readonly Type enumDatabaseConstantCodeType;
        private Dictionary<Type, EnumCache> enumCache;

        public EnumConverter(
            Func<DbContext> getDbContext,
            Action<IEnumConverterPopulator, DbContext> populateEnumCacheMethod,
            Action<string, string, string> logWarningAction,
            Action<string, string, string> logErrorAction,
            bool initializeEnumCache,
            Type enumDatabaseConstantCodeType)
        {
            this.getDbContext = getDbContext;
            this.populateEnumCacheMethod = populateEnumCacheMethod;
            this.logWarningAction = logWarningAction;
            this.logErrorAction = logErrorAction;
            this.enumDatabaseConstantCodeType = enumDatabaseConstantCodeType;

            if (initializeEnumCache)
                PopulateEnumCache();
        }

        public static IList<FieldInfo> GetAllPublicConstantValues<T>(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                .ToList();
        }

        private void PopulateEnumCache()
        {
            if (this.enumCache != null)
                return;

            lock (this)
            {
                if (this.enumCache != null)
                    return;

                this.enumCache = new Dictionary<Type, EnumCache>();

                using (var db = this.getDbContext())
                {
                    this.populateEnumCacheMethod(this, db);

                    // Save changes if there are any
                    db.SaveChanges();
                }
            }
        }

        public void AddEnumToCache<TEnum, TData>(
            DbContext db,
            Func<TData, int> idAccessor,
            Func<TData, string> codeAccessor,
            Func<string, TEnum> enumFromCode = null,
            Func<TEnum, string> codeFromEnum = null,
            bool ignoreUnknown = true) where TEnum : struct, IConvertible where TData : class
        {
            var prop = db.GetType().GetProperty(typeof(TData).Name);

            var dbSet = (DbSet<TData>)prop.GetValue(db);

            AddEnumToCache(dbSet, idAccessor, codeAccessor, enumFromCode, codeFromEnum, ignoreUnknown);
        }

        private void AddEnumToCache<TEnum, TData>(
            DbSet<TData> dbSet,
            Func<TData, int> idAccessor,
            Func<TData, string> codeAccessor,
            Func<string, TEnum> enumFromCode = null,
            Func<TEnum, string> codeFromEnum = null,
            bool ignoreUnknown = true,
            bool deleteFromTable = false) where TEnum : struct, IConvertible where TData : class
        {
            string[] enumNames;
            if (ignoreUnknown)
                enumNames = Enum.GetNames(typeof(TEnum)).Where(x => !x.Equals("Unknown", StringComparison.OrdinalIgnoreCase)).ToArray();
            else
                enumNames = Enum.GetNames(typeof(TEnum));

            string enumTypeName = typeof(TEnum).Name;

            if (enumFromCode == null)
            {
                // Use reflection
                string name = typeof(TEnum).Name + "_";
                var allConstants = GetAllPublicConstantValues<string>(this.enumDatabaseConstantCodeType)
                    .Where(x => x.Name.StartsWith(name));

                var mapping = allConstants.ToDictionary(x => (string)x.GetRawConstantValue(), x =>
                {
                    string code = x.Name.Substring(x.Name.IndexOf('_') + 1).ToUpper();

                    try
                    {
                        return (TEnum)Enum.Parse(typeof(TEnum), code, true);
                    }
                    catch (ArgumentException)
                    {
                        throw new ArgumentException($"Missing mapping for enum {enumTypeName} and code {code} in the DatabaseConstantCode class");
                    }
                });

                enumFromCode = code =>
                {
                    return mapping[code];
                };

                codeFromEnum = en =>
                {
                    return mapping.First(x => x.Value.Equals(en)).Key;
                };
            }

            if (codeFromEnum == null)
                throw new ArgumentNullException(nameof(codeFromEnum));

            var enumMapping = EnumCache.Create(
                dbSet: dbSet,
                idAccessor: idAccessor,
                codeAccessor: codeAccessor,
                enumFromCode: enumFromCode,
                codeFromEnum: codeFromEnum,
                enumNames: enumNames,
                deleteFromTable: deleteFromTable,
                enumTypeName: enumTypeName,
                logWarningAction: this.logWarningAction,
                logErrorAction: this.logErrorAction);

            this.enumCache.Add(typeof(TEnum), enumMapping);
        }

        public int GetIdFromEnum<T>(T value) where T : struct, IConvertible
        {
            if (this.enumCache == null)
                PopulateEnumCache();

            if (!this.enumCache.TryGetValue(typeof(T), out EnumCache enumMapping))
                throw new InvalidOperationException($"Enum type {typeof(T).Name} not in cache, fix the code in DataManager.cs and register the enum in the PopulateEnumCache method");

            return enumMapping.GetIdFromIntValue((int)(ValueType)value);
        }

        public int? GetIdFromEnum<T>(T? value) where T : struct, IConvertible
        {
            if (!value.HasValue)
                return null;

            return GetIdFromEnum(value.Value);
        }

        /// <summary>
        /// Returns an enumeration member by the specified <paramref name="id"/>.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="id">The ID of the enumeration member.</param>
        /// <returns>The enumeration member that is mapped to the specified <paramref name="id"/>.</returns>
        public T GetEnumValueFromId<T>(int id) where T : struct, IConvertible
        {
            if (this.enumCache == null)
                PopulateEnumCache();

            if (!this.enumCache.TryGetValue(typeof(T), out EnumCache enumMapping))
                throw new InvalidOperationException($"Enum type {typeof(T).Name} not in cache, fix the code in DataManager.cs and register the enum in the PopulateEnumCache method");

            return (T)(ValueType)enumMapping.GetIntValueFromId(id);
        }

        /// <summary>
        /// Returns an enumeration member by the specified <paramref name="id"/>.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="id">The ID of the enumeration member.</param>
        /// <returns>The enumeration member that is mapped to the specified <paramref name="id"/>.</returns>
        public T? GetEnumValueFromId<T>(int? id) where T : struct, IConvertible
        {
            if (!id.HasValue)
                return null;

            return GetEnumValueFromId<T>(id.Value);
        }

        internal string GetEnumCodeFromId<T>(int id) where T : struct, IConvertible
        {
            if (this.enumCache == null)
                PopulateEnumCache();

            if (!this.enumCache.TryGetValue(typeof(T), out EnumCache enumMapping))
                throw new InvalidOperationException($"Enum type {typeof(T).Name} not in cache, fix the code in DataManager.cs");

            return enumMapping.GetCodeFromId(id);
        }
    }
}
