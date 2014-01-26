using System;
using System.Collections.Generic;
using System.Linq;

namespace CouchPotato.LoveSeatAdapter {
  /// <summary>
  /// Helper class for chunking enumerables.
  /// </summary>
  internal static class EnumerableChunkExtensions {

    /// <summary>
    /// Chunk list to chunks in the specified size.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="chunkSize"></param>
    /// <returns></returns>
    public static IEnumerable<T[]> Chunks<T>(this IList<T> source, int chunkSize) {
      int chunksCount = (int)Math.Floor((double)source.Count / chunkSize);
      for (int chunkIndex = 0; chunkIndex < chunksCount; chunkIndex++) {
        IEnumerable<T> chunk = source.Skip(chunkSize * chunkIndex).Take(chunkSize);
        yield return chunk.ToArray();
      }

      if (chunksCount * chunkSize < source.Count) {
        // We have leftover which is not a full chunk.
        IEnumerable<T> chunk = source.Skip(chunkSize * chunksCount).Take(chunkSize);
        yield return chunk.ToArray();
      }
    }

  }
}
