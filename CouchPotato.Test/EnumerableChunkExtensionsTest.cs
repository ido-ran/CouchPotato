using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CouchPotato.LoveSeatAdapter;
using System.Collections.Generic;

namespace CouchPotato.Test {
  [TestClass]
  public class EnumerableChunkExtensionsTest {
    [TestMethod]
    public void EmptyList() {
      int[] empty = new int[0];
      int chunks = empty.Chunks(600).Count();

      Assert.AreEqual(0, chunks);
    }

    [TestMethod]
    public void ExactlyTwoChunks() {
      int[] source = Enumerable.Range(1, 20).ToArray();

      int[][] chunks = source.Chunks(10).ToArray();

      int[] chunk1 = chunks[0];
      int[] chunk2 = chunks[1];

      int[] expectedChunk1 = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
      int[] expectedChunk2 = new int[] { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

      CollectionAssert.AreEqual(expectedChunk1, chunk1);
      CollectionAssert.AreEqual(expectedChunk2, chunk2);
      Assert.AreEqual(2, chunks.Length);
    }

    [TestMethod]
    public void TwoChunksAndChange() {
      int[] source = Enumerable.Range(1, 23).ToArray();

      int[][] chunks = source.Chunks(10).ToArray();

      int[] chunk3 = chunks[2];

      int[] expectedChunk3 = new int[] { 21, 22, 23 };

      CollectionAssert.AreEqual(expectedChunk3, chunk3);
      Assert.AreEqual(3, chunks.Length);
    }

    [TestMethod]
    public void LessThanChunk() {
      int[] source = new int[] { 1, 2, 3, 4 };

      int[][] chunks = source.Chunks(10).ToArray();

      Assert.AreEqual(1, chunks.Length);
      CollectionAssert.AreEqual(source, chunks[0]);
    }
  }
}
