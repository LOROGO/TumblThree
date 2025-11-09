using System;
using System.Linq;
using System.Net;
using Moq;
using NUnit.Framework;
using TumblThree.Applications;

namespace TumblThree.Applications.Tests
{
    /// <summary>
    /// Unit tests for the CookieParser class.
    /// CookieParser is responsible for parsing HTTP cookie headers and converting them into .NET Cookie objects.
    /// This is crucial for authentication and session management across the application.
    /// </summary>
    [TestFixture]
    public class CookieParserTests
    {
        private const string TestHost = "example.com";
        private const string TestHost2 = "tumblr.com";

        #region Basic Cookie Parsing Tests

        [Test]
        public void GetAllCookiesFromHeader_WithEmptyString_ReturnsEmptyCollection()
        {
            // Arrange
            string emptyHeader = string.Empty;

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(emptyHeader, TestHost);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetAllCookiesFromHeader_WithSingleSimpleCookie_ReturnsSingleCookie()
        {
            // Arrange
            string header = "sessionId=abc123";

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("sessionId", result[0].Name);
            Assert.AreEqual("abc123", result[0].Value);
            Assert.AreEqual(TestHost, result[0].Domain);
        }

        [Test]
        public void GetAllCookiesFromHeader_WithMultipleCookies_ReturnsAllCookies()
        {
            // Arrange
            string header = "sessionId=abc123,userId=user456,token=xyz789";

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("sessionId", result[0].Name);
            Assert.AreEqual("abc123", result[0].Value);
            Assert.AreEqual("userId", result[1].Name);
            Assert.AreEqual("user456", result[1].Value);
            Assert.AreEqual("token", result[2].Name);
            Assert.AreEqual("xyz789", result[2].Value);
        }

        #endregion

        #region Cookie with Path Tests

        [Test]
        public void GetAllCookiesFromHeader_WithPath_SetsCookiePath()
        {
            // Arrange
            string header = "sessionId=abc123; path=/api";

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("sessionId", result[0].Name);
            Assert.AreEqual("/api", result[0].Path);
        }

        [Test]
        public void GetAllCookiesFromHeader_WithEmptyPath_SetsDefaultPath()
        {
            // Arrange
            string header = "sessionId=abc123; path=";

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("/", result[0].Path);
        }

        [Test]
        public void GetAllCookiesFromHeader_WithoutPath_SetsDefaultPath()
        {
            // Arrange
            string header = "sessionId=abc123";

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("/", result[0].Path);
        }

        #endregion

        #region Cookie with Domain Tests

        [Test]
        public void GetAllCookiesFromHeader_WithDomain_SetsCookieDomain()
        {
            // Arrange
            string header = "sessionId=abc123; domain=.tumblr.com";

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(".tumblr.com", result[0].Domain);
        }

        [Test]
        public void GetAllCookiesFromHeader_WithEmptyDomain_UsesHostAsDomain()
        {
            // Arrange
            string header = "sessionId=abc123; domain=";

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost2);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(TestHost2, result[0].Domain);
        }

        [Test]
        public void GetAllCookiesFromHeader_WithoutDomain_UsesHostAsDomain()
        {
            // Arrange
            string header = "sessionId=abc123";

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost2);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(TestHost2, result[0].Domain);
        }

        #endregion

        #region Cookie with Expires Tests

        [Test]
        public void GetAllCookiesFromHeader_WithExpiresAttribute_HandlesCookieWithComma()
        {
            // Arrange
            // Cookie with expires attribute contains a comma in the date format
            string header = "sessionId=abc123; expires=Wed, 09 Jun 2025 10:18:14 GMT";

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("sessionId", result[0].Name);
            Assert.AreEqual("abc123", result[0].Value);
        }

        #endregion

        #region Complex Cookie Tests

        [Test]
        public void GetAllCookiesFromHeader_WithComplexCookie_ParsesAllAttributes()
        {
            // Arrange
            string header = "sessionId=abc123; path=/api; domain=.example.com";

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("sessionId", result[0].Name);
            Assert.AreEqual("abc123", result[0].Value);
            Assert.AreEqual("/api", result[0].Path);
            Assert.AreEqual(".example.com", result[0].Domain);
        }

        [Test]
        public void GetAllCookiesFromHeader_WithMultipleCookiesAndAttributes_ParsesCorrectly()
        {
            // Arrange
            string header = "sessionId=abc123; path=/,userId=user456; domain=.example.com";

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("sessionId", result[0].Name);
            Assert.AreEqual("/", result[0].Path);
            Assert.AreEqual("userId", result[1].Name);
            Assert.AreEqual(".example.com", result[1].Domain);
        }

        #endregion

        #region Special Characters and Edge Cases

        [Test]
        public void GetAllCookiesFromHeader_WithCookieValueContainingEquals_ParsesCorrectly()
        {
            // Arrange
            string header = "token=base64==encoded==value";

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("token", result[0].Name);
            Assert.AreEqual("base64==encoded==value", result[0].Value);
        }

        [Test]
        public void GetAllCookiesFromHeader_WithNewLinesAndCarriageReturns_HandlesCorrectly()
        {
            // Arrange
            string header = "sessionId=abc123\r\n,userId=user456\r\n";

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("sessionId", result[0].Name);
            Assert.AreEqual("userId", result[1].Name);
        }

        #endregion

        #region Mock-Based Tests

        /// <summary>
        /// This test demonstrates mocking by verifying that parsed cookies can be added to a mocked cookie container.
        /// While CookieParser doesn't use interfaces directly, we can mock the container that consumes the result.
        /// </summary>
        [Test]
        public void GetAllCookiesFromHeader_ResultCanBeAddedToMockedContainer_Mock1()
        {
            // Arrange
            string header = "sessionId=abc123";
            var mockContainer = new Mock<ICookieContainer>();
            mockContainer.Setup(c => c.Add(It.IsAny<Cookie>())).Verifiable();

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost);

            // Simulate adding to container
            foreach (Cookie cookie in result)
            {
                mockContainer.Object.Add(cookie);
            }

            // Assert
            mockContainer.Verify(c => c.Add(It.IsAny<Cookie>()), Times.Once);
        }

        /// <summary>
        /// This test uses a mock to verify cookie validation logic.
        /// </summary>
        [Test]
        public void GetAllCookiesFromHeader_WithValidator_ValidatesCorrectly_Mock2()
        {
            // Arrange
            string header = "sessionId=abc123; path=/api";
            var mockValidator = new Mock<ICookieValidator>();
            mockValidator.Setup(v => v.IsValid(It.IsAny<Cookie>())).Returns(true).Verifiable();

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost);

            // Simulate validation
            foreach (Cookie cookie in result)
            {
                mockValidator.Object.IsValid(cookie);
            }

            // Assert
            mockValidator.Verify(v => v.IsValid(It.Is<Cookie>(c => c.Name == "sessionId")), Times.Once);
        }

        /// <summary>
        /// This test uses a mock logger to verify parsing logs (simulating a logging scenario).
        /// </summary>
        [Test]
        public void GetAllCookiesFromHeader_WithLogger_LogsParsing_Mock3()
        {
            // Arrange
            string header = "sessionId=abc123,userId=user456";
            var mockLogger = new Mock<ICookieParserLogger>();
            mockLogger.Setup(l => l.LogParsing(It.IsAny<string>(), It.IsAny<int>())).Verifiable();

            // Act
            CookieCollection result = CookieParser.GetAllCookiesFromHeader(header, TestHost);

            // Simulate logging
            mockLogger.Object.LogParsing(header, result.Count);

            // Assert
            Assert.AreEqual(2, result.Count);
            mockLogger.Verify(l => l.LogParsing(header, 2), Times.Once);
        }

        #endregion

        #region Helper Interfaces for Mocking

        /// <summary>
        /// Mock interface representing a cookie container for demonstration purposes.
        /// </summary>
        public interface ICookieContainer
        {
            void Add(Cookie cookie);
        }

        /// <summary>
        /// Mock interface representing a cookie validator for demonstration purposes.
        /// </summary>
        public interface ICookieValidator
        {
            bool IsValid(Cookie cookie);
        }

        /// <summary>
        /// Mock interface representing a logger for demonstration purposes.
        /// </summary>
        public interface ICookieParserLogger
        {
            void LogParsing(string header, int cookieCount);
        }

        #endregion
    }
}
