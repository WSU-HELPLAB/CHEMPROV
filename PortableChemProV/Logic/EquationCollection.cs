/*
Copyright 2012 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

// Original file author: Evan Olds

// Avoid referencing UI-specific stuff here. This class must remain UI indepdent and no Silverlight 
// specific things (or WinForms, or WPF, etc.) should be referenced in this code.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using ChemProV.Logic.Equations;

namespace ChemProV.Logic
{
    /// <summary>
    /// Represents a collection of EquationModel objects. This is a UI-independent core-logic class.
    /// </summary>
    public class EquationCollection : ICollection<EquationModel>, INotifyCollectionChanged
    {
        public delegate void EquationModelPropertyChangedDelegate(EquationModel sender, string propertyName);
        
        public event NotifyCollectionChangedEventHandler CollectionChanged = null;

        /// <summary>
        /// Event that gets fired when any property on any model in the collection gets modified. This is a 
        /// convenience event, as the same funtionality could be achieved by attaching event listeners to 
        /// each individual model in the collection.
        /// </summary>
        public event EquationModelPropertyChangedDelegate EquationModelPropertyChanged = null;

        #region Member variables

        private List<EquationModel> m_eqs = new List<EquationModel>();

        private bool m_readOnly = false;

        #endregion

        public EquationCollection() { }

        public void Add(EquationModel model)
        {
            if (m_readOnly)
            {
                throw new InvalidOperationException(
                    "Cannot add to a read-only equation collection");
            }
            
            m_eqs.Add(model);

            // Watch for property changes on this model
            model.PropertyChanged += new PropertyChangedEventHandler(Model_PropertyChanged);

            // Fire the CollectionChanged event if non-null
            if (null != CollectionChanged)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, model, m_eqs.Count - 1));
            }
        }

        public void Clear()
        {
            if (m_readOnly)
            {
                throw new InvalidOperationException(
                    "Cannot clear a read-only equation collection");
            }

            // Unsubscribe from all property change events before clearing the list
            foreach (EquationModel eq in m_eqs)
            {
                eq.PropertyChanged -= this.Model_PropertyChanged;
            }
            
            m_eqs.Clear();

            // Fire the CollectionChanged event if non-null
            if (null != CollectionChanged)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Checks to see if the collection contains the specified item. The 
        /// EquationModel.Equals(EquationModel) function is used for equality 
        /// comparisons.
        /// </summary>
        public bool Contains(EquationModel item)
        {
            foreach (EquationModel em in m_eqs)
            {
                if (em.Equals(item))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(EquationModel[] array, int arrayIndex)
        {
            // Documentation states 3 cases where we throw exceptions:
            if (null == array)
            {
                throw new ArgumentNullException();
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (m_eqs.Count > array.Length - arrayIndex)
            {
                // The number of elements in the source ICollection<T> is greater than the available 
                // space from arrayIndex to the end of the destination array.
                throw new ArgumentException();
            }

            // Copy references into the array
            foreach (EquationModel em in m_eqs)
            {
                array[arrayIndex++] = em;
            }
        }

        public int Count
        {
            get
            {
                return m_eqs.Count;
            }
        }

        public EquationModel GetById(int id)
        {
            foreach (EquationModel em in m_eqs)
            {
                if (id == em.Id)
                {
                    return em;
                }
            }

            return null;
        }

        public IEnumerator<EquationModel> GetEnumerator()
        {
            return new ECEnumerator(m_eqs);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(EquationModel item)
        {
            return m_eqs.IndexOf(item);
        }

        public void Insert(int index, EquationModel item)
        {
            if (m_readOnly)
            {
                throw new NotSupportedException(
                    "Cannot insert an item into a read-only equation collection");
            }
            
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "Index value for insertion into an equation collection cannot be " +
                    "negative (was " + index.ToString() + ")");
            }

            m_eqs.Insert(index, item);

            // Watch for property changes on this item
            item.PropertyChanged += new PropertyChangedEventHandler(Model_PropertyChanged);

            // Fire the CollectionChanged event if non-null
            if (null != CollectionChanged)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return m_readOnly;
            }
        }

        /// <summary>
        /// Event for a property change on any model in the collection. All models added to the collection 
        /// must be have their PropertyChanged event fire this method. When models are removed from this 
        /// collection then we must unsubscribe from the event.
        /// </summary>
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (null != EquationModelPropertyChanged)
            {
                EquationModelPropertyChanged(sender as EquationModel, e.PropertyName);
            }
        }

        public bool Remove(EquationModel item)
        {
            if (m_readOnly)
            {
                // Documentation states that we throw a NotSupportedException if the collection is read-only
                throw new NotSupportedException(
                    "Cannot remove an item from a read-only equation collection");
            }
            
            for (int i = 0; i < m_eqs.Count; i++)
            {
                if (item.Equals(m_eqs[i]))
                {
                    // Unsubscribe from the property change event before removing
                    m_eqs[i].PropertyChanged -= this.Model_PropertyChanged;
                    
                    // Remove the item
                    m_eqs.RemoveAt(i);

                    // Fire the CollectionChanged event if non-null
                    if (null != CollectionChanged)
                    {
                        CollectionChanged(this, new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Remove, item, i));
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the equation object at the specified index. Will throw an exception if the index is out 
        /// of range.
        /// </summary>
        public EquationModel this[int index]
        {
            get
            {
                return m_eqs[index];
            }
        }

        #region Enumerator class declartion

        private class ECEnumerator : IEnumerator<EquationModel>
        {
            private bool m_disposed = false;
            
            private List<EquationModel> m_eqsRef = new List<EquationModel>();

            // Enumerators are positioned before the first element until the first MoveNext() call.
            private int m_pos = -1;

            public ECEnumerator(List<EquationModel> eqs)
            {
                // Store a reference to the equation list
                m_eqsRef = eqs;
            }
            
            public EquationModel Current
            {
                get
                {
                    if (m_disposed)
                    {
                        throw new ObjectDisposedException("EquationCollection.ECEnumerator");
                    }
                    return m_eqsRef[m_pos];
                }
            }

            public void Dispose()
            {
                m_eqsRef = null;
                m_disposed = true;
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if 
            /// the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException("EquationCollection.ECEnumerator");
                }

                m_pos++;
                return m_pos < m_eqsRef.Count;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the 
            /// collection.
            /// </summary>
            public void Reset()
            {
                if (m_disposed)
                {
                    throw new ObjectDisposedException("EquationCollection.ECEnumerator");
                }
                
                m_pos = -1;
            }
        }


        #endregion
    }
}
