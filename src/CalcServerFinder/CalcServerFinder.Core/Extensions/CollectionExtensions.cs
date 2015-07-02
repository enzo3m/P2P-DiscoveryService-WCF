using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalcServerFinder.Core.Extensions
{
    /// <summary>
    /// Metodi di estensione per collezioni generiche.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Crea un <see cref="HashSet{T}"/> da un <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">Il tipo di elementi di collection.</typeparam>
        /// <param name="collection">La collezione <see cref="IEnumerable{T}"/> da cui creare l'<see cref="HashSet{T}"/>.</param>
        /// <returns>Un <see cref="HashSet{T}"/> che contiene gli elementi della collezione di input.</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection)
        {
            return new HashSet<T>(collection);
        }
    }
}
