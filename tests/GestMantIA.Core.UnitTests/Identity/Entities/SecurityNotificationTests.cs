using FluentAssertions;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces; // Para SecurityNotificationType

namespace GestMantIA.Core.UnitTests.Identity.Entities
{
    public class SecurityNotificationTests
    {
        [Fact]
        public void Constructor_Should_Initialize_Properties_Correctly()
        {
            // Arrange & Act
            var testUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "testUser" };
            var notification = new SecurityNotification
            {
                UserId = testUser.Id,
                User = testUser,
                Title = "Test Notification",
                Message = "This is a test notification",
                NotificationType = SecurityNotificationType.SecurityAlert
            };

            // Assert
            // Propiedades de BaseEntity
            notification.Id.Should().NotBe(Guid.Empty);
            notification.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            notification.IsDeleted.Should().BeFalse();
            notification.UpdatedAt.Should().BeNull();
            notification.DeletedAt.Should().BeNull();
            notification.DeletedBy.Should().BeNull();

            // Propiedades de SecurityNotification
            notification.UserId.Should().NotBe(Guid.Empty);
            notification.User.Should().NotBeNull();
            notification.Title.Should().NotBeNull();
            notification.Message.Should().NotBeNull();
            notification.NotificationType.Should().Be(SecurityNotificationType.SecurityAlert);
            notification.RelatedEventId.Should().BeNull();
            notification.IsRead.Should().BeFalse();
            notification.ReadAt.Should().BeNull();
        }

        [Fact]
        public void Properties_Should_Set_And_Get_Correctly()
        {
            // Arrange
            var testUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "notifyUser" };
            var notification = new SecurityNotification
            {
                UserId = testUser.Id,
                User = testUser,
                Title = "Initial Title",
                Message = "Initial Message",
                NotificationType = SecurityNotificationType.SecurityAlert
            };
            
            var testDate = DateTimeOffset.UtcNow;
            var relatedEventGuid = Guid.NewGuid();

            // Act
            notification.Title = "Password Change Alert";
            notification.Message = "Your password was recently changed.";
            notification.NotificationType = SecurityNotificationType.SecuritySettingsChanged;
            notification.RelatedEventId = relatedEventGuid;
            notification.IsRead = true;
            notification.ReadAt = testDate;
            // Propiedades de BaseEntity
            notification.Id = Guid.NewGuid(); // Sobrescribir para prueba
            notification.CreatedAt = testDate.DateTime.AddMinutes(-10);
            notification.UpdatedAt = testDate.DateTime.AddMinutes(-5);
            notification.IsDeleted = true;
            notification.DeletedAt = testDate.DateTime;
            notification.DeletedBy = Guid.NewGuid();

            // Assert
            notification.UserId.Should().Be(testUser.Id);
            notification.User.Should().Be(testUser);
            notification.User?.UserName.Should().Be("notifyUser");
            notification.Title.Should().Be("Password Change Alert");
            notification.Message.Should().Be("Your password was recently changed.");
            notification.NotificationType.Should().Be(SecurityNotificationType.SecuritySettingsChanged);
            notification.RelatedEventId.Should().Be(relatedEventGuid);
            notification.IsRead.Should().BeTrue();
            notification.ReadAt.Should().Be(testDate);

            notification.Id.Should().NotBe(Guid.Empty);
            notification.CreatedAt.Should().Be(testDate.DateTime.AddMinutes(-10));
            notification.UpdatedAt.Should().Be(testDate.DateTime.AddMinutes(-5));
            notification.IsDeleted.Should().BeTrue();
            notification.DeletedAt.Should().Be(testDate.DateTime);
            notification.DeletedBy.Should().NotBeEmpty();
        }

        [Fact]
        public void MarkAsRead_Should_Update_Properties_When_Not_Read()
        {
            // Arrange
            var notification = new SecurityNotification
            {
                UserId = Guid.NewGuid(),
                Title = "Test Notification",
                Message = "This is a test notification",
                NotificationType = SecurityNotificationType.SecurityAlert,
                IsRead = false,
                ReadAt = null
            };

            // Act
            notification.MarkAsRead();

            // Assert
            notification.IsRead.Should().BeTrue();
            notification.ReadAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void MarkAsRead_Should_Not_Update_ReadAt_When_Already_Read()
        {
            // Arrange
            var originalReadAt = DateTimeOffset.UtcNow.AddHours(-1);
            var notification = new SecurityNotification
            {
                UserId = Guid.NewGuid(),
                Title = "Test Notification",
                Message = "This is a test notification",
                NotificationType = SecurityNotificationType.SecurityAlert,
                IsRead = true,
                ReadAt = originalReadAt
            };

            // Act
            notification.MarkAsRead();

            // Assert
            notification.IsRead.Should().BeTrue(); // Debe permanecer true
            notification.ReadAt.Should().Be(originalReadAt); // No debe cambiar
        }
    }
}
