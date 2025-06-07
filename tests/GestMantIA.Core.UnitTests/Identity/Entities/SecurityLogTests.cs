using FluentAssertions;
using GestMantIA.Core.Identity.Entities;

namespace GestMantIA.Core.UnitTests.Identity.Entities
{
    public class SecurityLogTests
    {
        [Fact]
        public void Constructor_Should_Initialize_Properties_Correctly()
        {
            // Arrange & Act
            var log = new SecurityLog
            {
                EventType = SecurityEventTypes.LoginSucceeded,
                Description = "Test log entry"
            };

            // Assert
            // Propiedades de BaseEntity
            log.Id.Should().NotBe(Guid.Empty);
            log.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            log.IsDeleted.Should().BeFalse();
            log.UpdatedAt.Should().BeNull();
            log.DeletedAt.Should().BeNull();
            log.DeletedBy.Should().BeNull();

            // Propiedades de SecurityLog
            log.UserId.Should().BeNull();
            log.User.Should().BeNull();
            log.EventType.Should().NotBeNull();
            log.Description.Should().NotBeNull();
            log.IpAddress.Should().BeNull();
            log.UserAgent.Should().BeNull();
            log.AdditionalData.Should().BeNull();
            log.Succeeded.Should().BeTrue();
        }

        [Fact]
        public void Properties_Should_Set_And_Get_Correctly()
        {
            // Arrange
            var log = new SecurityLog
            {
                EventType = SecurityEventTypes.LoginSucceeded,
                Description = "Test log entry"
            };
            
            var testUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "testUser" };
            var testDate = DateTime.UtcNow;
            var userId = Guid.NewGuid();

            // Act
            log.UserId = userId;
            log.User = testUser;
            log.EventType = SecurityEventTypes.LoginSucceeded;
            log.Description = "User logged in successfully.";
            log.IpAddress = "192.168.1.1";
            log.UserAgent = "TestBrowser/1.0";
            log.AdditionalData = "{\"key\":\"value\"}";
            log.Succeeded = true;
            // Propiedades de BaseEntity
            log.Id = Guid.NewGuid(); // Sobrescribir para prueba
            log.CreatedAt = testDate.AddMinutes(-5);
            log.UpdatedAt = testDate;
            log.IsDeleted = true;
            log.DeletedAt = testDate.AddMinutes(1);
            log.DeletedBy = Guid.NewGuid();

            // Assert
            log.UserId.Should().Be(userId);
            log.User.Should().Be(testUser);
            log.User?.UserName.Should().Be("testUser");
            log.EventType.Should().Be(SecurityEventTypes.LoginSucceeded);
            log.Description.Should().Be("User logged in successfully.");
            log.IpAddress.Should().Be("192.168.1.1");
            log.UserAgent.Should().Be("TestBrowser/1.0");
            log.AdditionalData.Should().Be("{\"key\":\"value\"}");
            log.Succeeded.Should().BeTrue();

            log.Id.Should().NotBe(Guid.Empty);
            log.CreatedAt.Should().Be(testDate.AddMinutes(-5));
            log.UpdatedAt.Should().Be(testDate);
            log.IsDeleted.Should().BeTrue();
            log.DeletedAt.Should().Be(testDate.AddMinutes(1));
            log.DeletedBy.Should().NotBeEmpty();
        }
    }
}
