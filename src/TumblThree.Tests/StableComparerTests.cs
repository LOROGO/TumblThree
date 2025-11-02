using System.Collections.Generic;
using System.ComponentModel;
using Moq;
using Xunit;
using TumblThree.Presentation.Comparers;

namespace TumblThree.Tests
{
    public class StableComparerTests
    {
        private class TestItem
        {
            public string Name { get; set; }
            public int Progress { get; set; }
            public string Collection { get; set; }
        }

        [Fact]
        public void Compare_ByStringProperty_Ascending()
        {
            var a = new TestItem { Name = "a" };
            var b = new TestItem { Name = "b" };
            var items = new List<object> { a, b };
            var sort = new List<SortDescription> { new SortDescription(nameof(TestItem.Name), ListSortDirection.Ascending) };

            var cmp = new StableComparer(items, sort);
            Assert.True(cmp.Compare(a, b) < 0);
        }

        [Fact]
        public void Compare_ByStringProperty_Descending()
        {
            var a = new TestItem { Name = "a" };
            var b = new TestItem { Name = "b" };
            var items = new List<object> { a, b };
            var sort = new List<SortDescription> { new SortDescription(nameof(TestItem.Name), ListSortDirection.Descending) };

            var cmp = new StableComparer(items, sort);
            Assert.True(cmp.Compare(a, b) > 0);
        }

        [Fact]
        public void Stability_WhenEqualProperties_UsesInitialOrder()
        {
            var a = new TestItem { Name = "same" };
            var b = new TestItem { Name = "same" };
            var items = new List<object> { a, b };
            var sort = new List<SortDescription> { new SortDescription(nameof(TestItem.Name), ListSortDirection.Ascending) };

            var cmp = new StableComparer(items, sort);
            // a was added before b, so a should come before b when equal
            Assert.True(cmp.Compare(a, b) < 0);
        }

        [Fact]
        public void Compare_UsesCollectionFunc_MockCalled()
        {
            var a = new TestItem { Collection = "C1" };
            var b = new TestItem { Collection = "C2" };
            var items = new List<object> { a, b };
            var sort = new List<SortDescription> { new SortDescription("__collection", ListSortDirection.Ascending) };

            var mockCollection = new Mock<System.Func<object, string>>();
            mockCollection.Setup(f => f(It.IsAny<object>())).Returns((object o) => ((TestItem)o).Collection);

            var cmp = new StableComparer(items, sort, getCollectionName: mockCollection.Object);
            Assert.True(cmp.Compare(a, b) < 0);

            // Expect the delegate to have been called for both items at least once
            mockCollection.Verify(f => f(It.IsAny<object>()), Times.AtLeast(2));
        }

        [Fact]
        public void Compare_UsesProgressFunc_MockCalled()
        {
            var a = new TestItem { Progress = 5 };
            var b = new TestItem { Progress = 10 };
            var items = new List<object> { a, b };
            var sort = new List<SortDescription> { new SortDescription("__progress", ListSortDirection.Ascending) };

            var mockProgress = new Mock<System.Func<object, object>>();
            mockProgress.Setup(f => f(It.IsAny<object>())).Returns((object o) => ((TestItem)o).Progress);

            var cmp = new StableComparer(items, sort, getCollectionName: null, getProgressValue: mockProgress.Object);
            Assert.True(cmp.Compare(a, b) < 0);

            mockProgress.Verify(f => f(It.IsAny<object>()), Times.AtLeast(2));
        }
    }
}
