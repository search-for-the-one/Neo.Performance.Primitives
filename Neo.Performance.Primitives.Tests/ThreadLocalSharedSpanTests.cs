using System;
using Neo.Performance.Primitives.ThreadLocal;
using NUnit.Framework;

namespace Neo.Performance.Primitives.Tests
{
    public class ThreadLocalSharedSpanTests
    {
        [Test]
        public void SharedSpanTest()
        {
            using (ThreadLocalSharedSpan<Bar>.Get<int>(10, out var array))
            {
                for (var i = 0; i < array.Length; i++)
                    array[i] = i;

                for (var i = 0; i < array.Length; i++)
                    Assert.AreEqual(i, array[i]);
            }
        }

        [Test]
        public void SharedSpanMultipleClassesTest()
        {
            using (ThreadLocalSharedSpan<Foo>.Get<int>(5, out var array1))
            using (ThreadLocalSharedSpan<Bar>.Get<int>(10, out var array2))
            {
                for (var i = 0; i < array1.Length; i++)
                    array1[i] = i;

                for (var i = 0; i < array2.Length; i++)
                    array2[i] = 100 + i;

                for (var i = 0; i < array1.Length; i++)
                    Assert.AreEqual(i, array1[i]);

                for (var i = 0; i < array2.Length; i++)
                    Assert.AreEqual(100 + i, array2[i]);
            }
        }

        [Test]
        public void AlreadyInUseTest()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (ThreadLocalSharedSpan<Foo>.Get<int>(5, out _))
                using (ThreadLocalSharedSpan<Foo>.Get<int>(10, out _))
                {
                }
            });
        }

        [Test]
        public void AlreadyInUseWithoutScopeTest()
        {
            IDisposable shared = null;
            Assert.Throws<InvalidOperationException>(() =>
            {
                shared = ThreadLocalSharedSpan<Foo>.Get<int>(5, out _);
                ThreadLocalSharedSpan<Foo>.Get<int>(10, out _);
            });
            shared.Dispose();
        }

        [Test]
        public void AlreadyInUseMultipleClassesTest()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (ThreadLocalSharedSpan<Bar>.Get<int>(10, out _))
                using (ThreadLocalSharedSpan<Foo>.Get<int>(5, out _))
                using (ThreadLocalSharedSpan<Foo>.Get<int, int>(10, out _, 1, out _))
                {
                }
            });
        }

        [Test]
        public void BadCountTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using (ThreadLocalSharedSpan<Foo>.Get<Foo>(-1, out _))
                {
                }
            });
        }

        [Test]
        public void ZeroCountTest()
        {
            using (ThreadLocalSharedSpan<Foo>.Get<char>(0, out var empty))
            {
                Assert.AreEqual(Span<char>.Empty.Length, empty.Length);
            }
        }

        [Test]
        public void BigAllocTest()
        {
            using (ThreadLocalSharedSpan<Bar>.Get<int>(2 * 1024 * 1024, out var array))
            {
                for (var i = 0; i < array.Length; i++)
                    array[i] = i;

                for (var i = 0; i < array.Length; i++)
                    Assert.AreEqual(i, array[i]);
            }
        }

        [Test]
        public void TwoInputsSharedSpanTest()
        {
            using (ThreadLocalSharedSpan<Bar>.Get<int, char>(10, out var array, 5, out var charArray))
            {
                for (var i = 0; i < array.Length; i++)
                    array[i] = i;

                for (var i = 0; i < charArray.Length; i++)
                    charArray[i] = i.ToString()[0];

                for (var i = 0; i < array.Length; i++)
                    Assert.AreEqual(i, array[i]);

                for (var i = 0; i < charArray.Length; i++)
                    Assert.AreEqual(i.ToString()[0], charArray[i]);
            }
        }

        [Test]
        public void TwoInputsSharedSpanMultipleClassesTest()
        {
            using (ThreadLocalSharedSpan<Foo>.Get<int, char>(5, out var array1, 3, out var charArray1))
            using (ThreadLocalSharedSpan<Bar>.Get<int, char>(10, out var array2, 5, out var charArray2))
            {
                for (var i = 0; i < array1.Length; i++)
                    array1[i] = i;

                for (var i = 0; i < array2.Length; i++)
                    array2[i] = 100 + i;

                for (var i = 0; i < charArray1.Length; i++)
                    charArray1[i] = i.ToString()[0];

                for (var i = 0; i < charArray2.Length; i++)
                    charArray2[i] = (1 + i).ToString()[0];

                for (var i = 0; i < array1.Length; i++)
                    Assert.AreEqual(i, array1[i]);

                for (var i = 0; i < array2.Length; i++)
                    Assert.AreEqual(100 + i, array2[i]);

                for (var i = 0; i < charArray1.Length; i++)
                    Assert.AreEqual(i.ToString()[0], charArray1[i]);

                for (var i = 0; i < charArray2.Length; i++)
                    Assert.AreEqual((1 + i).ToString()[0], charArray2[i]);
            }
        }

        [Test]
        public void TwoInputsAlreadyInUseTest()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (ThreadLocalSharedSpan<Foo>.Get<int, Foo>(5, out _, 1, out _))
                using (ThreadLocalSharedSpan<Foo>.Get<int, Bar>(10, out _, 2, out _))
                {
                }
            });
        }

        [Test]
        public void TwoInputsAlreadyInUseWithoutScopeTest()
        {
            IDisposable shared = null;
            Assert.Throws<InvalidOperationException>(() =>
            {
                shared = ThreadLocalSharedSpan<Foo>.Get<int, Foo>(5, out _, 1, out _);
                ThreadLocalSharedSpan<Foo>.Get<int, Bar>(10, out _, 2, out _);
            });
            shared.Dispose();
        }

        [Test]
        public void TwoInputsAlreadyInUseMultipleClassesTest()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (ThreadLocalSharedSpan<Bar>.Get<int, uint>(10, out _, 2345, out _))
                using (ThreadLocalSharedSpan<Foo>.Get<int, uint>(5, out _, 1235, out _))
                using (ThreadLocalSharedSpan<Foo>.Get<int, int>(10, out _, 1, out _))
                {
                }
            });
        }

        [Test]
        public void TwoInputsBadCountTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using (ThreadLocalSharedSpan<Foo>.Get<Foo, Bar>(-1, out _, 0, out _))
                {
                }
            });
        }

        [Test]
        public void TwoInputsZeroCountTest()
        {
            using (ThreadLocalSharedSpan<Foo>.Get<char, byte>(0, out var empty, 0, out var empty2))
            {
                Assert.AreEqual(Span<char>.Empty.Length, empty.Length);
                Assert.AreEqual(Span<byte>.Empty.Length, empty2.Length);
            }

            using (ThreadLocalSharedSpan<Foo>.Get<char, byte>(0, out var empty, 1, out var nonEmpty))
            {
                Assert.AreEqual(Span<char>.Empty.Length, empty.Length);
                Assert.AreEqual(1, nonEmpty.Length);
            }

            using (ThreadLocalSharedSpan<Foo>.Get<char, byte>(1, out var nonEmpty, 0, out var empty))
            {
                Assert.AreEqual(Span<char>.Empty.Length, empty.Length);
                Assert.AreEqual(1, nonEmpty.Length);
            }
        }

        [Test]
        public void TwoInputsBigAllocTest()
        {
            using (ThreadLocalSharedSpan<Foo>.Get<int, long>(2 * 1024 * 1024, out var array, 1 * 1024 * 1024, out var longArray))
            {
                for (var i = 0; i < array.Length; i++)
                    array[i] = i;

                for (var i = 0; i < longArray.Length; i++)
                    longArray[i] = i;

                for (var i = 0; i < array.Length; i++)
                    Assert.AreEqual(i, array[i]);

                for (var i = 0; i < longArray.Length; i++)
                    Assert.AreEqual(i, longArray[i]);
            }
        }


        [Test]
        public void ThreeeInputsSharedSpanTest()
        {
            using (ThreadLocalSharedSpan<Bar>.Get<int, char, double>(10, out var array1, 5, out var array2, 3, out var array3))
            {
                for (var i = 0; i < array1.Length; i++)
                    array1[i] = i;

                for (var i = 0; i < array2.Length; i++)
                    array2[i] = i.ToString()[0];

                for (var i = 0; i < array3.Length; i++)
                    array3[i] = i;

                for (var i = 0; i < array1.Length; i++)
                    Assert.AreEqual(i, array1[i]);

                for (var i = 0; i < array2.Length; i++)
                    Assert.AreEqual(i.ToString()[0], array2[i]);

                for (var i = 0; i < array3.Length; i++)
                    Assert.AreEqual(i, array3[i]);
            }
        }

        [Test]
        public void ThreeInputsSharedSpanMultipleClassesTest()
        {
            using (ThreadLocalSharedSpan<Foo>.Get<int, char, double>(5, out var array1, 3, out var charArray1, 2, out var doubleArray1))
            using (ThreadLocalSharedSpan<Bar>.Get<int, char, double>(10, out var array2, 5, out var charArray2, 4, out var doubleArray2))
            {
                for (var i = 0; i < array1.Length; i++)
                    array1[i] = i;

                for (var i = 0; i < array2.Length; i++)
                    array2[i] = 100 + i;

                for (var i = 0; i < charArray1.Length; i++)
                    charArray1[i] = i.ToString()[0];

                for (var i = 0; i < charArray2.Length; i++)
                    charArray2[i] = (1 + i).ToString()[0];

                for (var i = 0; i < doubleArray1.Length; i++)
                    doubleArray1[i] = i;

                for (var i = 0; i < doubleArray2.Length; i++)
                    doubleArray2[i] = 10 + i;

                for (var i = 0; i < array1.Length; i++)
                    Assert.AreEqual(i, array1[i]);

                for (var i = 0; i < array2.Length; i++)
                    Assert.AreEqual(100 + i, array2[i]);

                for (var i = 0; i < charArray1.Length; i++)
                    Assert.AreEqual(i.ToString()[0], charArray1[i]);

                for (var i = 0; i < charArray2.Length; i++)
                    Assert.AreEqual((1 + i).ToString()[0], charArray2[i]);

                for (var i = 0; i < doubleArray1.Length; i++)
                    Assert.AreEqual(i, doubleArray1[i]);

                for (var i = 0; i < doubleArray2.Length; i++)
                    Assert.AreEqual(10 + i, doubleArray2[i]);
            }
        }

        [Test]
        public void ThreeInputsAlreadyInUseTest()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (ThreadLocalSharedSpan<Foo>.Get<int, Foo, double>(5, out _, 1, out _, 3, out _))
                using (ThreadLocalSharedSpan<Foo>.Get<int, Bar, char>(10, out _, 2, out _, 4, out _))
                {
                }
            });
        }

        [Test]
        public void ThreeInputsAlreadyInUseWithoutScopeTest()
        {
            IDisposable shared = null;
            Assert.Throws<InvalidOperationException>(() =>
            {
                shared = ThreadLocalSharedSpan<Foo>.Get<int, Foo, double>(5, out _, 1, out _, 3, out _);
                ThreadLocalSharedSpan<Foo>.Get<int, Bar, char>(10, out _, 2, out _, 4, out _);
            });
            shared.Dispose();
        }

        [Test]
        public void ThreeInputsAlreadyInUseMultipleClassesTest()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (ThreadLocalSharedSpan<Bar>.Get<int, uint, double>(10, out _, 2345, out _, 4, out _))
                using (ThreadLocalSharedSpan<Foo>.Get<int, uint, double>(5, out _, 1235, out _, 2, out _))
                using (ThreadLocalSharedSpan<Foo>.Get<int, int, decimal>(10, out _, 1, out _, 12, out _))
                {
                }
            });
        }

        [Test]
        public void ThreeInputsBadCountTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using (ThreadLocalSharedSpan<Foo>.Get<Foo, Bar, double>(-1, out _, 0, out _, 2, out _))
                {
                }
            });
        }

        [Test]
        public void ThreeInputsZeroCountTest()
        {
            using (ThreadLocalSharedSpan<Foo>.Get<char, byte, double>(0, out var empty, 0, out var empty2, 0,
                out var empty3))
            {
                Assert.AreEqual(Span<char>.Empty.Length, empty.Length);
                Assert.AreEqual(Span<byte>.Empty.Length, empty2.Length);
                Assert.AreEqual(Span<double>.Empty.Length, empty3.Length);
            }
        }

        [Test]
        public void ThreeInputsBigAllocTest()
        {
            using (ThreadLocalSharedSpan<Foo>.Get<int, long, double>(2 * 1024 * 1024, out var array, 1 * 1024 * 1024,
                out var longArray, 3 * 1024 * 1024, out var doubleArray))
            {
                for (var i = 0; i < array.Length; i++)
                    array[i] = i;

                for (var i = 0; i < longArray.Length; i++)
                    longArray[i] = i;

                for (var i = 0; i < doubleArray.Length; i++)
                    doubleArray[i] = i;

                for (var i = 0; i < array.Length; i++)
                    Assert.AreEqual(i, array[i]);

                for (var i = 0; i < longArray.Length; i++)
                    Assert.AreEqual(i, longArray[i]);

                for (var i = 0; i < doubleArray.Length; i++)
                    Assert.AreEqual(i, doubleArray[i]);
            }
        }

        private struct Foo
        {
        }

        private struct Bar
        {
        }
    }
}
