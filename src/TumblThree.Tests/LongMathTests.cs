using System;
using Xunit;
using Guava.RateLimiter;

namespace TumblThree.Tests
{
    public class LongMathTests
    {
        [Fact]
        public void SaturatedAdd_PositiveNoOverflow_ReturnsSum()
        {
            Assert.Equal(30L, LongMath.SaturatedAdd(10L, 20L));
        }

        [Fact]
        public void SaturatedAdd_NegativeNoOverflow_ReturnsSum()
        {
            Assert.Equal(-30L, LongMath.SaturatedAdd(-10L, -20L));
        }

        [Fact]
        public void SaturatedAdd_PositiveOverflow_ReturnsLongMax()
        {
            var result = LongMath.SaturatedAdd(long.MaxValue, 1L);
            Assert.Equal(long.MaxValue, result);
        }

        [Fact]
        public void SaturatedAdd_LargePositiveOverflow_ReturnsLongMax()
        {
            var result = LongMath.SaturatedAdd(long.MaxValue - 1L, 10L);
            Assert.Equal(long.MaxValue, result);
        }

        [Fact]
        public void SaturatedAdd_NegativeOverflow_ReturnsLongMin()
        {
            var result = LongMath.SaturatedAdd(long.MinValue, -1L);
            Assert.Equal(long.MinValue, result);
        }

        [Fact]
        public void SaturatedAdd_MixedSigns_NoOverflow()
        {
            var result = LongMath.SaturatedAdd(long.MaxValue, -1L);
            Assert.Equal(long.MaxValue - 1L, result);
        }

        [Fact]
        public void SaturatedAdd_Boundary_MaxPlusZero_ReturnsMax()
        {
            Assert.Equal(long.MaxValue, LongMath.SaturatedAdd(long.MaxValue, 0L));
        }
    }
}
