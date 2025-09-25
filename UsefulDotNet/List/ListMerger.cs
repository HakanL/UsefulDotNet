using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haukcode.UsefulDotNet
{
    public static class ListMerger
    {
        public static void MapManyToManyAssociation<T>(
            IEnumerable<T> currentData,
            IEnumerable<T> newData,
            Func<T, T, bool> equality,
            Action<T> insert,
            Action<T> delete)
        {
            var currentList = currentData.ToList();
            var newList = newData.ToList();

            foreach (var existing in currentList)
            {
                bool anyMatch = newList.Any(a => equality(existing, a));

                if (!anyMatch)
                    // Remove item
                    delete(existing);
            }

            foreach (var newItem in newList)
            {
                bool anyMatch = currentList.Any(a => equality(newItem, a));

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
            IEnumerable<TCurrent> currentData,
            IEnumerable<TNew> newData,
            Func<TCurrent, TNew, bool> equality,
            Action<TNew> insert,
            Action<TCurrent> delete,
            ref bool dirty,
            Action<TCurrent, TNew> update = null)
        {
            var currentItemsToRemove = new List<TCurrent>();
            var currentList = currentData.ToList();
            var newDataList = newData?.ToList() ?? new List<TNew>();

            foreach (var current in currentList)
            {
                var matchingNewEntry = newDataList.Where(a => equality(current, a)).ToArray();

                if (matchingNewEntry.Length > 0)
                {
                    newDataList.Remove(matchingNewEntry[0]);
                    update?.Invoke(current, matchingNewEntry[0]);
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
            IEnumerable<TCurrent> currentData,
            IEnumerable<TNew> newData,
            Func<TCurrent, TNew, bool> equality,
            Func<TNew, Task> insert,
            Func<TCurrent, Task> delete,
            Func<TCurrent, TNew, Task> update = null)
        {
            bool dirty = false;

            var currentItemsToRemove = new List<TCurrent>();
            var currentList = currentData.ToList();
            var newDataList = newData?.ToList() ?? new List<TNew>();

            foreach (var current in currentList)
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
            IEnumerable<T> currentData,
            IEnumerable<T> newData,
            Action<T, T> assignment,
            Action<T> insert,
            Action<T> delete)
        {
            var currentDataList = currentData.ToList();
            var newDataList = newData.ToList();

            for (int i = 0; i < currentDataList.Count; i++)
            {
                if (i < newDataList.Count)
                    // Replace
                    assignment(currentDataList[i], newDataList[i]);
            }

            if (currentDataList.Count < newDataList.Count)
            {
                // Insert new items
                newDataList.Skip(currentDataList.Count).ToList().ForEach(a => insert(a));
            }
            else if (currentDataList.Count > newDataList.Count)
            {
                // Delete items
                foreach (var item in currentDataList.Skip(newDataList.Count))
                {
                    delete(item);
                }
            }
        }
    }
}
