using FluentAssertions;
using GestMantIA.Core.Entities.Identity; // Namespace de UserProfile
using GestMantIA.Core.Identity.Entities; // Namespace de ApplicationUser

namespace GestMantIA.Core.UnitTests.Entities; // El namespace del archivo de prueba puede quedar así

public class UserProfileTests
{
    [Fact]
    public void Constructor_WithValidUserId_Should_InitializePropertiesCorrectly()
    {
        // Arrange
        var appUser = new ApplicationUser(); // UserId vendrá de un ApplicationUser existente
        Guid testUserId = appUser.Id;

        // Act
        var userProfile = new UserProfile(testUserId);

        // Assert
        userProfile.Id.Should().NotBe(Guid.Empty); // Heredado de BaseEntity y autogenerado
        userProfile.UserId.Should().Be(testUserId);
        userProfile.User.Should().BeNull(); // La propiedad de navegación no se carga automáticamente

        // Propiedades opcionales deben ser null por defecto
        userProfile.FirstName.Should().BeNull();
        userProfile.LastName.Should().BeNull();
        userProfile.DateOfBirth.Should().BeNull();
        userProfile.AvatarUrl.Should().BeNull();
        userProfile.Bio.Should().BeNull();
        userProfile.PhoneNumber.Should().BeNull();
        userProfile.StreetAddress.Should().BeNull();
        userProfile.City.Should().BeNull();
        userProfile.StateProvince.Should().BeNull();
        userProfile.PostalCode.Should().BeNull();
        userProfile.Country.Should().BeNull();

        // Propiedades de IAuditableEntity (de BaseEntity)
        // CreatedAt se establece en BaseEntity si no se sobrescribe. BaseEntity no lo inicializa.
        // userProfile.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5)); // Esto dependerá de si BaseEntity lo inicializa
        userProfile.IsDeleted.Should().BeFalse(); // BaseEntity no inicializa IsDeleted, el valor por defecto de bool es false.
    }

    // [Fact] // Comentado temporalmente para resolver errores de compilación, revisar lógica si UserProfile(Guid userId)
    // public void Constructor_WithNullUserId_Should_ThrowArgumentNullException()
    // {
    //     // Arrange
    //     // string nullUserId = null!; // Guid no puede ser null directamente

    //     // Act & Assert
    //     // var exception = Assert.Throws<ArgumentNullException>(() => new UserProfile(nullUserId)); // No compilará si el constructor espera Guid
    //     // exception.ParamName.Should().Be("userId");
    // }

    [Fact]
    public void Properties_Should_Set_And_Get_Correctly()
    {
        // Arrange
        var appUser = new ApplicationUser();
        var userProfile = new UserProfile(appUser.Id);
        var testDate = DateTime.UtcNow.AddYears(-20);
        var testUserNav = new ApplicationUser { UserName = "navUser" };

        // Act
        userProfile.FirstName = "TestFirstName";
        userProfile.LastName = "TestLastName";
        userProfile.DateOfBirth = testDate;
        userProfile.AvatarUrl = "http://example.com/avatar.jpg";
        userProfile.Bio = "This is a test bio.";
        userProfile.PhoneNumber = "0987654321";
        userProfile.StreetAddress = "123 Test St";
        userProfile.City = "TestCity";
        userProfile.StateProvince = "TestState";
        userProfile.PostalCode = "12345";
        userProfile.Country = "TestCountry";
        userProfile.User = testUserNav; // Asignar propiedad de navegación

        // Propiedades de BaseEntity
        userProfile.CreatedAt = testDate.AddDays(-1);
        userProfile.CreatedBy = "creator";
        userProfile.UpdatedAt = testDate;
        userProfile.UpdatedBy = "updater";
        userProfile.IsDeleted = true;
        userProfile.DeletedAt = testDate.AddDays(1);
        userProfile.DeletedBy = Guid.NewGuid();

        // Assert
        userProfile.FirstName.Should().Be("TestFirstName");
        userProfile.LastName.Should().Be("TestLastName");
        userProfile.DateOfBirth.Should().Be(testDate);
        userProfile.AvatarUrl.Should().Be("http://example.com/avatar.jpg");
        userProfile.Bio.Should().Be("This is a test bio.");
        userProfile.PhoneNumber.Should().Be("0987654321");
        userProfile.StreetAddress.Should().Be("123 Test St");
        userProfile.City.Should().Be("TestCity");
        userProfile.StateProvince.Should().Be("TestState");
        userProfile.PostalCode.Should().Be("12345");
        userProfile.Country.Should().Be("TestCountry");
        userProfile.User.Should().Be(testUserNav);
        userProfile.User.UserName.Should().Be("navUser");

        userProfile.CreatedAt.Should().Be(testDate.AddDays(-1));
        userProfile.CreatedBy.Should().Be("creator");
        userProfile.UpdatedAt.Should().Be(testDate);
        userProfile.UpdatedBy.Should().Be("updater");
        userProfile.IsDeleted.Should().BeTrue();
        userProfile.DeletedAt.Should().Be(testDate.AddDays(1));
        userProfile.DeletedBy.Should().NotBeNull();
    }
}
