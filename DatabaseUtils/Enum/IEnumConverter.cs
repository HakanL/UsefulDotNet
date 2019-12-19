using System;

namespace Haukcode.DatabaseUtils
{
    /// <summary>
    /// Defines a contract that must be supported by a component dealing with conversion for enumerations.
    /// </summary>
    public interface IEnumConverter
    {
        /// <summary>
        /// Returns an enumeration member by the specified <paramref name="id"/>.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="id">The ID of the enumeration member.</param>
        /// <returns>The enumeration member that is mapped to the specified <paramref name="id"/>.</returns>
        T GetEnumValueFromId<T>(int id) where T : struct, IConvertible;

        /// <summary>
        /// Returns an enumeration member by the specified <paramref name="id"/>.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="id">The ID of the enumeration member.</param>
        /// <returns>The enumeration member that is mapped to the specified <paramref name="id"/>.</returns>
        T? GetEnumValueFromId<T>(int? id) where T : struct, IConvertible;

        int GetIdFromEnum<T>(T value) where T : struct, IConvertible;

        int? GetIdFromEnum<T>(T? value) where T : struct, IConvertible;
    }
}
