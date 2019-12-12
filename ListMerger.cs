using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haukcode.UsefulDotNet
{
    public static class ListMerger
    {
        public static void MapManyToManyAssociation<T>(
            ICollection<T> currentData,
            ICollection<T> newData,
            Func<T, T, bool> equality,
            Action<T> insert,
            Action<T> delete)
        {
            foreach (var existing in currentData.ToList())
            {
                bool anyMatch = newData.Any(a => equality(existing, a));

                if (!anyMatch)
                    // Remove item
                    delete(existing);
            }

            foreach (var newItem in newData)
            {
                bool anyMatch = currentData.Any(a => equality(newItem, a));

                if (!anyMatch)
                    insert(newItem);
            }
        }

        /// <summary>
        /// Map the new data to the existing. Changes are handled as a delete/insert.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentData">Existing data</param>
        /// <param name="newData"></param>
        /// <param name="equality"></param>
        /// <param name="insert"></param>
        /// <param name="delete"></param>
        /// <param name="update"></param>
        public static void MapImmutableOneToMany<TCurrent, TNew>(
            ICollection<TCurrent> currentData,
            ICollection<TNew> newData,
            Func<TCurrent, TNew, bool> equality,
            Action<TNew> insert,
            Action<TCurrent> delete,
            ref bool dirty,
            Action<TCurrent, TNew> update = null)
        {
            var currentItemsToRemove = new List<TCurrent>();
            var newDataList = newData?.ToList() ?? new List<TNew>();

            foreach (var current in currentData.ToList())
            {
                var matchingNewEntry = newDataList.Where(a => equality(current, a)).ToArray();

                if (matchingNewEntry.Length > 0)
                {
                    newDataList.Remove(matchingNewEntry[0]);
                    if (update != null)
                        update(current, matchingNewEntry[0]);
                }
                else
                {
                    delete(current);
                    dirty = true;
                }
            }

            // Insert new
            newDataList.ForEach(a => insert(a));

            if (newDataList.Any())
                dirty = true;
        }

        /// <summary>
        /// Map the new data to the existing. Changes are handled as a delete/insert.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentData"></param>
        /// <param name="newData"></param>
        /// <param name="equality"></param>
        /// <param name="insert"></param>
        /// <param name="delete"></param>
        /// <param name="update"></param>
        public static async Task<bool> MapImmutableOneToManyAsync<TCurrent, TNew>(
            ICollection<TCurrent> currentData,
            ICollection<TNew> newData,
            Func<TCurrent, TNew, bool> equality,
            Func<TNew, Task> insert,
            Func<TCurrent, Task> delete,
            Func<TCurrent, TNew, Task> update = null)
        {
            bool dirty = false;

            var currentItemsToRemove = new List<TCurrent>();
            var newDataList = newData?.ToList() ?? new List<TNew>();

            foreach (var current in currentData.ToList())
            {
                var matchingNewEntries = newDataList.Where(a => equality(current, a)).ToList();

                if (matchingNewEntries.Any())
                {
                    foreach (var matchingNewEntry in matchingNewEntries)
                    {
                        newDataList.Remove(matchingNewEntry);
                        if (update != null)
                            await update(current, matchingNewEntry);
                    }
                }
                else
                {
                    if (delete != null)
                    {
                        await delete(current);

                        dirty = true;
                    }
                }
            }

            // Insert new
            if (insert != null)
            {
                foreach (var a in newDataList)
                {
                    await insert(a);
                }

                if (newDataList.Any())
                    dirty = true;
            }


            return dirty;
        }

        /// <summary>
        /// Map objects based on position in the lists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentData"></param>
        /// <param name="newData"></param>
        /// <param name="assignment"></param>
        /// <param name="insert"></param>
        /// <param name="delete"></param>
        public static void SequentialOneToManyMap<T>(
            ICollection<T> currentData,
            ICollection<T> newData,
            Action<T, T> assignment,
            Action<T> insert,
            Action<T> delete)
        {
            var currentDataArray = currentData.ToArray();
            var newDataArray = newData.ToArray();

            for (int i = 0; i < currentData.Count; i++)
            {
                if (i < newData.Count)
                    // Replace
                    assignment(currentDataArray[i], newDataArray[i]);
            }

            if (currentData.Count < newData.Count)
            {
                // Insert new items
                newDataArray.Skip(currentData.Count).ToList().ForEach(a => insert(a));
            }
            else if (currentData.Count > newData.Count)
            {
                // Delete items
                currentData.Skip(newData.Count).ToList().ForEach(a => delete(a));
            }
        }
    }
}
