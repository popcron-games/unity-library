#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace UnityLibrary
{
    /// <summary>
    /// Read only access to <see cref="object"/> description.
    /// </summary>
    public interface IObject
    {
        /// <summary>
        /// Fetches a list of all instances that implement <typeparamref name="T"/>, subtype from it, or are that type.
        /// <para></para>
        /// When the <see cref="foreach"/> statement is used, the element type can be safely assumed to be <typeparamref name="T"/>,
        /// despite the fact that the list is of type <see cref="object"/>.
        /// </summary>
        IReadOnlyList<T> GetAllThatAre<T>();

        /// <summary>
        /// Retrieves the first instance of <typeparamref name="T"/> if it exists.
        /// </summary>
        public bool TryGetFirst<T>([NotNullWhen(true)] out T? value) where T : notnull
        {
            IReadOnlyList<T> all = GetAllThatAre<T>();
            if (all.Count > 0)
            {
                value = all[0];
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
        public T GetFirst<T>() where T : notnull
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

        /// <summary>
        /// Returns the first possible instance of <typeparamref name="T"/>, otherwise <see cref="null"/>
        /// </summary>
        public T? GetFirstPossible<T>() where T : class
        {
            if (TryGetFirst(out T? value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }
    }
}