using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace APIScripts.Utils
{
    /// <summary>
    /// A LinkedDictionary is a hybrid of a LinkedList and a Dictionary.
    /// The LinkedDictionary can be traversed in four ways:
    /// 1. Enumerated using .GetEnumerator() - which doesn't guarantee key order
    /// 2. Indexed using .ElementAt() - risky, as the underlying dictionary doesn't guarantee order integrity
    /// 3. Looked up by key like a regular Dictionary - slow due to hashing and binary-tree search overhead https://referencesource.microsoft.com/#mscorlib/system/collections/generic/dictionary.cs,bcd13bb775d408f1
    /// 4. NEW WAY: crawled using .Next() or .Previous() - guarantees sequential order if values are added in key-sequential order
    /// IT IS LINKED: Each node can be traversed back and forwards using Previous() and Next() like a LinkedList
    /// IT IS OPTIONALLY SEQUENTIAL: if this flag is set, new values can only be added to at the end of the dictionary (keys are compared) to ensure index-sequence is preserved.
    /// Partial inspiration from http://www.glennslayden.com/code/c-sharp/linked-dictionary
    /// </summary>
    [Serializable]
    public class LinkedDictionary<TKey, TValue>
        : Dictionary<TKey, LinkedDictionaryEntry<TValue>>,
            ILinkedDictionary<TKey, TValue>
        where TKey : IComparable
    {
        #region Members

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => base.Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => base.Values.Select(v => v.NodeValue).ToList();

        public new TValue this[TKey index]
        {
            // kludge for serialization error "InvalidOperationException: You must implement a default accessor because SerializableDictionary inherits from ICollection"
            // https://stackoverflow.com/questions/2331755/xmlserialize-exception
            // TODO: check this works!
            get
            {
                if (this.ContainsKey(index))
                {
                    return base[index].NodeValue;
                }

                return default;
            }
            set => base[index] = new LinkedDictionaryEntry<TValue>(value);
        }

        public bool IsReadOnly => throw new NotImplementedException();

        public bool IsSequential { get; }

        #endregion Members

        #region Constructor

        public LinkedDictionary()
            : base()
        {
        }

        public LinkedDictionary(bool isSequential)
            : base()
        {
            this.IsSequential = isSequential;
        }

        public LinkedDictionary(bool isSequential,
                                SerializationInfo info,
                                StreamingContext context)
            : base(info, context)
        {
            this.IsSequential = isSequential;
        }

        public LinkedDictionary(bool isSequential, IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
            this.IsSequential = isSequential;
        }

        #endregion Constructor

        #region Methods

        #region Methods - Add

        /// <summary>
        /// Add new entry - check order if IsSequential flag is set
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public new void Add(TKey key, TValue value)
        {
            if (this.IsSequential)
            {
                // new item MUST have a key greater than the previous key; this is the optional Sequential character of this collection
                if (this.Count > 0)
                {
                    int comparisonResult = key.CompareTo(base.Keys.Last());
                    if (comparisonResult < 1)
                        throw new Exception("Cannot add new item.  Key must be greater than the last key.");
                }
            }

            // Create the new Linked entry
            LinkedDictionaryEntry<TValue> newEntry = new LinkedDictionaryEntry<TValue>(value);

            // Link the new value in IF there's something to link it to
            if (base.Values.Count > 0)
            {
                LinkedDictionaryEntry<TValue> existingLastValue = base.Values.Last();

                // Create forwards-looking link from the existing last entry to the new entry
                existingLastValue.Next = newEntry;

                // Create backwards-looking link from new entry to the existing last entry
                newEntry.Previous = existingLastValue;
            }

            base.Add(key, newEntry);
            ;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        #endregion Methods - Add

        #region Methods - Get

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (base.TryGetValue(key, out var theNode))
            {
                value = theNode.NodeValue;
                return true;
            }

            value = default(TValue);
            return false;
        }

        public TKey FirstKey() => this.Keys.First();

        public TKey LastKey() => this.Keys.Last();

        public ILinkedDictionaryEntry<TValue> FirstNode() => this.Values.First();

        public ILinkedDictionaryEntry<TValue> LastNode() => this.Values.Last();

        public TValue FirstValue() => this.Values.First().NodeValue;

        public TValue LastValue() => this.Values.Last().NodeValue;

        #endregion Methods - Get

        #region Methods - Remove

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return base.Remove(item.Key);
        }

        #endregion Methods - Remove

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return base.GetEnumerator() as IEnumerator<KeyValuePair<TKey, TValue>>;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    };

    /// <summary>
    /// LinkedDictionaryEntry
    /// Base type for Values in LinkedDictionary<K, V>
    /// </summary>
    [DebuggerDisplay("_previous = {Previous} _next = {Next}")]
    public class LinkedDictionaryEntry<TValue>
        : ILinkedDictionaryEntry<TValue>
    {
        #region Members

        #region Members - Next

        public ILinkedDictionaryEntry<TValue> Next { get; set; }

        public ILinkedDictionaryEntry NextUntyped => this.Next;

        #endregion Members - Next

        #region Members - Previous

        public ILinkedDictionaryEntry<TValue> Previous { get; set; }

        public ILinkedDictionaryEntry PreviousUntyped => this.Previous;

        #endregion Members - Previous

        // VALUE
        public TValue NodeValue { get; }

        #endregion Members

        #region Constructor

        public LinkedDictionaryEntry(TValue nodeValue)
        {
            this.NodeValue = nodeValue;
        }

        #endregion Constructor

        #region Methods

        // Remove this item from a list by patching over it
        public void Unlink()
        {
            this.Previous.Next = this.Next;
            this.Next.Previous = this.Previous;
            this.Next = this.Previous = null;
        }

        // Insert this item into a list after the specified element
        public void InsertAfter(LinkedDictionaryEntry<TValue> e)
        {
            e.Next.Previous = this;
            this.Next = e.Next;
            this.Previous = e;
            e.Next = this;
        }

        public void SetNext<TValue>(ILinkedDictionaryEntry<TValue> newEntry)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return this.NodeValue.ToString();
        }

        #endregion Methods
    };

    public interface ILinkedDictionary<TKey, TValue>
        : IDictionary<TKey, TValue>
    {
        TKey LastKey();

        TKey FirstKey();

        TValue FirstValue();

        TValue LastValue();

        new void Add(TKey key, TValue value);

        string ToString();
    }

    public interface ILinkedDictionaryEntry
    {
        ILinkedDictionaryEntry PreviousUntyped { get; }
        ILinkedDictionaryEntry NextUntyped { get; }
    }

    public interface ILinkedDictionaryEntry<TValue>
        : ILinkedDictionaryEntry
    {
        ILinkedDictionaryEntry<TValue> Previous { get; set; }
        ILinkedDictionaryEntry<TValue> Next { get; set; }

        TValue NodeValue { get; }
    }
}