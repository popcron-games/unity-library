#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Game.Library
{
    /// <summary>
    /// Read only access to <see cref="object"/> instances by their type.
    /// </summary>
    public interface IRegistryView
    {
        /// <summary>
        /// Fetches a list of all instances that implement <typeparamref name="T"/>, subtype from it, or are that type.
        /// <para></para>
        /// When the <see cref="foreach"/> statement is used, the element type can be safely assumed to be <typeparamref name="T"/>,
        /// despite the fact that the list is of type <see cref="object"/>.
        /// </summary>
        IReadOnlyList<object> GetAllThatAre<T>();

        /// <summary>
        /// Retrieves the first instance of <typeparamref name="T"/> if it exists.
        /// </summary>
        public bool TryGetFirst<T>([NotNullWhen(true)] out T? value)
        {
            IReadOnlyList<object> all = GetAllThatAre<T>();
            if (all.Count > 0)
            {
                value = (T)all[0];
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// Returns the first instance of <typeparamref name="T"/>, otherwise a <see cref="NullReferenceException"/> will occur.
        /// </summary>
        public T GetFirst<T>()
        {
            if (TryGetFirst(out T? value))
            {
                return value;
            }
            else
            {
                throw new NullReferenceException($"No instance of {typeof(T)} found in {this}");
            }
        }
    }
}