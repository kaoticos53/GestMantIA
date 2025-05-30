using FluentAssertions;
using GestMantIA.Core.Identity.Entities; // Cambiado para usar ApplicationUser

namespace GestMantIA.Core.UnitTests.Entities; // El namespace del archivo de prueba puede quedar así por ahora

public class UserTests // Considerar renombrar a ApplicationUserTests
{
    [Fact]
    public void Constructor_Should_Initialize_Default_Values()
    {
        // Arrange & Act
        var user = new ApplicationUser();

        // Assert
        user.Id.Should().NotBe(Guid.Empty); // IdentityUser lo genera
        user.IsActive.Should().BeTrue();
        user.LockoutReason.Should().BeEmpty();
        user.LockoutDate.Should().BeNull();
        user.FullName.Should().BeEmpty();
        user.FirstName.Should().BeEmpty();
        user.LastName.Should().BeEmpty();
        user.UserRoles.Should().NotBeNull().And.BeEmpty();
        user.RefreshTokens.Should().NotBeNull().And.BeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5)); // Permite una pequeña diferencia por el tiempo de ejecución
        user.UpdatedAt.Should().BeNull();
        user.IsDeleted.Should().BeFalse();
        user.LastLoginDate.Should().BeNull();
        user.DeletedAt.Should().BeNull();
        user.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Properties_Should_Set_And_Get_Correctly()
    {
        // Arrange
        var user = new ApplicationUser();
        var testGuid = Guid.NewGuid();
        var testDate = DateTime.UtcNow.AddDays(-1);

        // Act
        user.UserName = "test.user";
        user.Email = "test.user@example.com";
        user.PhoneNumber = "1234567890";
        user.IsActive = false;
        user.LockoutReason = "Test Lockout";
        user.LockoutDate = testDate;
        user.FullName = "Test User FullName";
        user.FirstName = "Test";
        user.LastName = "User";
        user.UpdatedAt = testDate;
        user.IsDeleted = true;
        user.LastLoginDate = testDate;
        user.DeletedAt = testDate;
        user.DeletedBy = testGuid;

        // Assert
        user.UserName.Should().Be("test.user");
        user.Email.Should().Be("test.user@example.com");
        user.PhoneNumber.Should().Be("1234567890");
        user.IsActive.Should().BeFalse();
        user.LockoutReason.Should().Be("Test Lockout");
        user.LockoutDate.Should().Be(testDate);
        user.FullName.Should().Be("Test User FullName"); // StringLength(100) - la validación de longitud es generalmente manejada por EF Core / ASP.NET Identity, no en el POCO.
        user.FirstName.Should().Be("Test");             // StringLength(50)
        user.LastName.Should().Be("User");             // StringLength(50)
        user.UpdatedAt.Should().Be(testDate);
        user.IsDeleted.Should().BeTrue();
        user.LastLoginDate.Should().Be(testDate);
        user.DeletedAt.Should().Be(testDate);
        user.DeletedBy.Should().Be(testGuid);
    }
}
