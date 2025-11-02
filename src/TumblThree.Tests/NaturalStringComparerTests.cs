using System.Runtime.InteropServices;
using Xunit;

namespace TumblThree.Tests
{
    internal static class SafeNativeMethods
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }

    internal sealed class TestNaturalStringComparer
    {
        public int Compare(string a, string b)
        {
            return SafeNativeMethods.StrCmpLogicalW(a, b);
        }
    }

    public class NaturalStringComparerTests
    {
        [Fact]
        public void Compare_SimpleAlphabetical_ReturnsNegative()
        {
            var cmp = new TestNaturalStringComparer();
            Assert.True(cmp.Compare("apple", "banana") < 0);
        }

        [Fact]
        public void Compare_EqualStrings_ReturnsZero()
        {
            var cmp = new TestNaturalStringComparer();
            Assert.Equal(0, cmp.Compare("same", "same"));
        }

        [Fact]
        public void Compare_File2VsFile10_NaturalOrder_File2BeforeFile10()
        {
            var cmp = new TestNaturalStringComparer();
            Assert.True(cmp.Compare("file2", "file10") < 0);
        }

        [Fact]
        public void Compare_File10VsFile2_NaturalOrder_File10AfterFile2()
        {
            var cmp = new TestNaturalStringComparer();
            Assert.True(cmp.Compare("file10", "file2") > 0);
        }

        [Fact]
        public void Compare_Img3VsImg12_NumericPartsCompared()
        {
            var cmp = new TestNaturalStringComparer();
            Assert.True(cmp.Compare("img3.png", "img12.png") < 0);
        }

        [Fact]
        public void Compare_Img12VsImg3_NumericPartsCompared()
        {
            var cmp = new TestNaturalStringComparer();
            Assert.True(cmp.Compare("img12.png", "img3.png") > 0);
        }

        [Fact]
        public void Compare_CaseDifference_TreatedEqual()
        {
            var cmp = new TestNaturalStringComparer();
            // StrCmpLogicalW is case-insensitive for ASCII letters in typical locales
            Assert.Equal(0, cmp.Compare("Apple", "apple"));
        }

        [Fact]
        public void Compare_AlphaNumericComplex()
        {
            var cmp = new TestNaturalStringComparer();
            Assert.True(cmp.Compare("a1b2", "a1b10") < 0);
        }
    }
}
