/**
 * Houdini Geo File Importer for Unity
 *
 * Copyright 2015 by Waldo Bronchart <wbronchart@gmail.com>
 * Exporter added in 2021 by Roy Theunissen <roy.theunissen@live.nl>
 * Licensed under GNU General Public License 3.0 or later. 
 * Some rights reserved. See COPYING, AUTHORS.
 */

using System.Collections;
using System.Collections.Generic;

namespace Houdini.GeoImportExport
{
    public partial class SplineCollection<SplineType> : IList<SplineType>
        where SplineType : SplineDataBase
    {
        private List<SplineType> points = new List<SplineType>();

        #region IList Delegation
        public IEnumerator<SplineType> GetEnumerator() => points.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => points.GetEnumerator();

        public void Add(SplineType item) => points.Add(item);

        public void Clear() => points.Clear();

        public bool Contains(SplineType item) => points.Contains(item);

        public void CopyTo(SplineType[] array, int arrayIndex) => points.CopyTo(array, arrayIndex);

        public bool Remove(SplineType item) => points.Remove(item);

        public int Count => points.Count;

        public bool IsReadOnly => false;

        public int IndexOf(SplineType item) => points.IndexOf(item);

        public void Insert(int index, SplineType item) => points.Insert(index, item);

        public void RemoveAt(int index) => points.RemoveAt(index);

        public SplineType this[int index]
        {
            get => points[index];
            set => points[index] = value;
        }
        #endregion IList Delegation
    }
}
