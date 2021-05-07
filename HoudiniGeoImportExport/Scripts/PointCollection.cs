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
    public partial class PointCollection<PointType> : IList<PointType>
        where PointType : PointData
    {
        private List<PointType> points = new List<PointType>();

        #region IList Delegation
        public IEnumerator<PointType> GetEnumerator() => points.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => points.GetEnumerator();

        public void Add(PointType item) => points.Add(item);

        public void Clear() => points.Clear();

        public bool Contains(PointType item) => points.Contains(item);

        public void CopyTo(PointType[] array, int arrayIndex) => points.CopyTo(array, arrayIndex);

        public bool Remove(PointType item) => points.Remove(item);

        public int Count => points.Count;

        public bool IsReadOnly => false;

        public int IndexOf(PointType item) => points.IndexOf(item);

        public void Insert(int index, PointType item) => points.Insert(index, item);

        public void RemoveAt(int index) => points.RemoveAt(index);

        public PointType this[int index]
        {
            get => points[index];
            set => points[index] = value;
        }
        #endregion IList Delegation
    }
}
