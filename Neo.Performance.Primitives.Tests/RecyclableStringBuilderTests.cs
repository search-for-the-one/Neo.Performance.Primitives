using System;
using System.Collections.Generic;
using System.Linq;
using Neo.Performance.Primitives.Text;
using NUnit.Framework;

namespace Neo.Performance.Primitives.Tests
{
    public class RecyclableStringBuilderTests
    {
        [TestCaseSource(nameof(AppendCases))]
        public void AppendTests(params object[] values)
        {
            var builder = new RecyclableStringBuilder();
            foreach (var o in values)
                Append(builder, o);

            var expected = string.Join(string.Empty, values.Select(GetString));
            Assert.AreEqual(expected, builder.ToString());

            builder.Clear();
            Assert.AreEqual(string.Empty, builder.ToString());

            const string afterClear = "after clear";
            builder.Append(afterClear);
            Assert.AreEqual(afterClear, builder.ToString());

            Assert.Throws<ArgumentNullException>(() => builder.Append((RecyclableStringBuilder)null));

            builder.Dispose();
            builder.Dispose(); // Dispose multiple times is ok
            Assert.Throws<ObjectDisposedException>(() => builder.ToString());
            Assert.Throws<ObjectDisposedException>(() => builder.Append(string.Empty));
            Assert.Throws<ObjectDisposedException>(() => builder.Append(char.MinValue));
            Assert.Throws<ObjectDisposedException>(() => builder.Append(new RecyclableStringBuilder()));
        }

        private static IEnumerable<object[]> AppendCases()
        {
            yield return new object[] { string.Empty };
            yield return new object[] { "test" };
            yield return new object[] { new string('a', 256) };
            yield return new object[] { string.Empty, string.Empty };
            yield return new object[] { string.Empty, "test" };
            yield return new object[] { string.Empty, "test", null };
            yield return new object[] { string.Empty, new string('a', 256), string.Empty };
            yield return new object[] { new string('a', 256), string.Empty };
            yield return new object[] { new string('a', 256), "b", new string('c', 256) };
            yield return new object[] { 'a' };
            yield return new object[] { 'a', 'b', 'c', "test" };
            yield return new object[] { 'a', 'b', 'c', new string('d', 256 - 3), 'e' };
            yield return new object[] { string.Empty, 'a' };
            yield return new object[] { null, 'a', string.Empty };
            yield return new object[] { string.Empty, "a", 'b' };
            yield return new object[] { string.Empty, "a", 'b', string.Empty };
            yield return new object[] { null, "a", 'b', "c" };
            yield return new object[] { string.Empty, "a", 'b', "c", string.Empty };
            yield return new object[] { new RecyclableStringBuilder().Append("abc"), "abc", 'a', 'b', 'c' };
            yield return new object[] { new RecyclableStringBuilder(), "abc", 'a', 'b', 'c' };
            yield return new object[] { new RecyclableStringBuilder(new RecyclableStringBuilder().Append("abc")), "abc", 'a', 'b', 'c' };
            yield return new object[] { new string('a', 256), string.Empty, new RecyclableStringBuilder().Append(new string('b', 256)), 'c' };
            yield return new object[] { new string('a', 256), null, 'b', new RecyclableStringBuilder().Append(new string('c', 255)), 'd' };
        }

        private static object GetString(object o)
        {
            if (o == null)
                return string.Empty;

            switch (o)
            {
                case string str:
                    return str;
                case char c:
                    return c;
                case RecyclableStringBuilder other:
                    using (other)
                        return other.ToString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(o));
            }
        }

        private static void Append(RecyclableStringBuilder builder, object o)
        {
            if (o == null)
            {
                builder.Append((string)null);
                return;
            }

            switch (o)
            {
                case string str:
                    builder.Append(str);
                    break;
                case char c:
                    builder.Append(c);
                    break;
                case RecyclableStringBuilder other:
                    builder.Append(other);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(o));
            }
        }
    }
}
