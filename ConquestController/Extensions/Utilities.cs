using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ConquestController.Extensions
{
    public static class Utilities
    {
        public static ObservableCollection<T> CopyCollection<T>(this ObservableCollection<T> options)
        {
            var newList = new ObservableCollection<T>();

            foreach (var option in options)
            {
                if (!(option is ICloneable element)) throw new InvalidOperationException("Item passed to collection does not implement ICloneable");

                newList.Add((T)element.Clone());
            }

            return newList;
        }
    }
}
