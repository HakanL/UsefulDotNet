using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Haukcode.DatabaseUtils
{
    internal class EnumCache
    {
        private readonly string enumName;
        private readonly Dictionary<int, int> intValueIndex;
        private readonly Dictionary<int, int> idIndex;
        private readonly Dictionary<int, string> codes;
        private readonly Action<string, string, string> logWarningAction;
        private readonly Action<string, string, string> logErrorAction;

        internal EnumCache(string enumName, Action<string, string, string> logWarningAction, Action<string, string, string> logErrorAction)
        {
            this.enumName = enumName;
            this.logWarningAction = logWarningAction;
            this.logErrorAction = logErrorAction;

            this.intValueIndex = new Dictionary<int, int>();
            this.idIndex = new Dictionary<int, int>();
            this.codes = new Dictionary<int, string>();
        }

        public static EnumCache Create<T, TData>(DbSet<TData> dbSet,
            Func<TData, int> idAccessor,
            Func<TData, string> codeAccessor,
            Func<string, T> enumFromCode,
            Func<T, string> codeFromEnum,
            string[] enumNames,
            bool deleteFromTable,
            string enumTypeName,
            Action<string, string, string> logWarningAction,
            Action<string, string, string> logErrorAction) where T : struct, IConvertible where TData : class
        {
            var enumMapping = new EnumCache(typeof(T).Name, logWarningAction, logErrorAction);

            enumMapping.PopulateEnumCache(dbSet, idAccessor, codeAccessor, enumFromCode, codeFromEnum, enumNames, deleteFromTable, enumTypeName);

            return enumMapping;
        }

        public int GetIdFromIntValue(int intValue)
        {
            if (!this.intValueIndex.TryGetValue(intValue, out int id))
                throw new ArgumentException(string.Format("Value {0} is not valid for enum {1}", intValue, this.enumName));

            return id;
        }

        public int GetIntValueFromId(int id)
        {
            if (!this.idIndex.TryGetValue(id, out int intValue))
                throw new ArgumentException(string.Format("Id {0} is not valid for enum {1}", id, this.enumName));

            return intValue;
        }

        public string GetCodeFromId(int id)
        {
            if (!this.codes.TryGetValue(id, out string code))
                throw new ArgumentException(string.Format("Id {0} is not valid for enum {1}", id, this.enumName));

            return code;
        }

        private void AddMapping(int intValue, int id, string code)
        {
            this.intValueIndex.Add(intValue, id);
            this.idIndex.Add(id, intValue);
            this.codes.Add(id, code);
        }

        private void PopulateEnumCache<T, TData>(
            DbSet<TData> dbSet,
            Func<TData, int> idAccessor,
            Func<TData, string> codeAccessor,
            Func<string, T> enumFromCode,
            Func<T, string> codeFromEnum,
            string[] enumNames,
            bool deleteFromTable,
            string enumTypeName) where T : struct, IConvertible where TData : class
        {
            var used = new HashSet<string>();
            int highestValue = 0;

            foreach (var record in dbSet)
            {
                int id = idAccessor(record);
                string code = codeAccessor(record);

                if (id > highestValue)
                    highestValue = id;

                T enumValue;
                try
                {
                    enumValue = enumFromCode(code);
                }
                catch
                {
                    this.logWarningAction?.Invoke("Enum {Code} in {Table} doesn't exist in code, only in the table", code, enumTypeName);

                    if (deleteFromTable)
                    {
                        // Attempt to delete
                        dbSet.Remove(record);
                    }

                    continue;
                }

                used.Add(enumValue.ToString());

                AddMapping((int)(ValueType)enumValue, id, code);
            }

            foreach (string enumName in enumNames)
            {
                if (used.Contains(enumName))
                    continue;

                // Insert

                if(Enum.TryParse<T>(enumName, out T enumValue))
                    throw new ArgumentException($"Invalid enum name {enumName}");

                string code;
                try
                {
                    code = codeFromEnum(enumValue);
                }
                catch
                {
                    this.logErrorAction?.Invoke("Unable to get code from enum {Enum} for {EnumType}", enumName, enumTypeName);

                    throw;
                }

                int id = ++highestValue;

                var newItem = Activator.CreateInstance<TData>();

                var properties = typeof(TData).GetProperties();
                if (properties.Length < 3)
                    throw new ArgumentException("Invalid enum table type");

                if (properties[0].PropertyType != typeof(int))
                    throw new ArgumentException("First property has to be of of type int");

                properties[0].SetValue(newItem, id);
                properties.Single(x => x.Name == "Code").SetValue(newItem, code);
                properties.Single(x => x.Name == "Name").SetValue(newItem, enumName);

                dbSet.Add(newItem);

                AddMapping((int)(ValueType)enumValue, id, code);
            }
        }
    }
}
