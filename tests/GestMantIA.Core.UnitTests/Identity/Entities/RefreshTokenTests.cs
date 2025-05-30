using FluentAssertions;
using GestMantIA.Core.Identity.Entities;

namespace GestMantIA.Core.UnitTests.Identity.Entities
{
    public class RefreshTokenTests
    {
        [Fact]
        public void Constructor_Should_Initialize_Default_And_Inherited_Properties()
        {
            // Arrange & Act
            var refreshToken = new RefreshToken();

            // Assert
            // Propiedades de BaseEntity (GestMantIA.Core.Identity.Entities.BaseEntity)
            refreshToken.Id.Should().NotBe(Guid.Empty);
            refreshToken.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1)); // BaseEntity inicializa a DateTime.UtcNow
            refreshToken.UpdatedAt.Should().BeNull();
            refreshToken.IsDeleted.Should().BeFalse(); // BaseEntity inicializa a false
            refreshToken.DeletedAt.Should().BeNull();
            refreshToken.DeletedBy.Should().BeNull();

            // Propiedades inicializadas en el constructor de RefreshToken
            refreshToken.Token.Should().Be(string.Empty);
            refreshToken.CreatedByIp.Should().Be(string.Empty);
            refreshToken.RevokedByIp.Should().Be(string.Empty);
            refreshToken.ReplacedByToken.Should().Be(string.Empty);
            refreshToken.UserId.Should().Be(Guid.Empty);
            refreshToken.User.Should().BeNull();

            // Propiedades no inicializadas explícitamente en el constructor (valores por defecto de C#)
            refreshToken.Expires.Should().Be(default(DateTime));
            refreshToken.IsRevoked.Should().BeFalse();
            refreshToken.Revoked.Should().BeNull();
            refreshToken.ReasonRevoked.Should().BeNull();

            // Propiedades calculadas (NotMapped)
            // Si Expires es default(DateTime), IsExpired será true.
            refreshToken.IsExpired.Should().BeTrue();
            // Si IsRevoked es false y IsExpired es true, IsActive será false.
            refreshToken.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Properties_Should_Set_And_Get_Correctly()
        {
            // Arrange
            var refreshToken = new RefreshToken();
            var testUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "testuser" };
            var testDate = DateTime.UtcNow;

            // Act
            refreshToken.Token = "test-token-value";
            refreshToken.Expires = testDate.AddDays(7);
            refreshToken.IsRevoked = true;
            refreshToken.CreatedByIp = "127.0.0.1";
            refreshToken.Revoked = testDate.AddDays(1);
            refreshToken.RevokedByIp = "192.168.1.1";
            refreshToken.ReasonRevoked = "User logged out";
            refreshToken.ReplacedByToken = "new-test-token";
            refreshToken.UserId = testUser.Id;
            refreshToken.User = testUser;

            // Propiedades de BaseEntity (para asegurar que también se pueden setear si es necesario)
            var baseEntityCreation = testDate.AddMinutes(-5);
            refreshToken.Id = Guid.NewGuid(); // Aunque BaseEntity lo genera, probamos que se puede setear si se quisiera (ej. al leer de DB)
            refreshToken.CreatedAt = baseEntityCreation;
            refreshToken.UpdatedAt = testDate.AddMinutes(-1);
            refreshToken.IsDeleted = true;
            refreshToken.DeletedAt = testDate;
            refreshToken.DeletedBy = Guid.NewGuid();


            // Assert
            refreshToken.Token.Should().Be("test-token-value");
            refreshToken.Expires.Should().Be(testDate.AddDays(7));
            refreshToken.IsRevoked.Should().BeTrue();
            refreshToken.CreatedByIp.Should().Be("127.0.0.1");
            refreshToken.Revoked.Should().Be(testDate.AddDays(1));
            refreshToken.RevokedByIp.Should().Be("192.168.1.1");
            refreshToken.ReasonRevoked.Should().Be("User logged out");
            refreshToken.ReplacedByToken.Should().Be("new-test-token");
            refreshToken.UserId.Should().Be(testUser.Id);
            refreshToken.User.Should().Be(testUser);
            refreshToken.User.UserName.Should().Be("testuser");

            refreshToken.Id.Should().NotBeEmpty();
            refreshToken.CreatedAt.Should().Be(baseEntityCreation);
            refreshToken.UpdatedAt.Should().Be(testDate.AddMinutes(-1));
            refreshToken.IsDeleted.Should().BeTrue();
            refreshToken.DeletedAt.Should().Be(testDate);
            refreshToken.DeletedBy.Should().NotBeEmpty();

            // Verificar propiedades calculadas con valores seteados
            // Expires es en el futuro, IsRevoked es true => IsExpired es false, IsActive es false
            refreshToken.IsExpired.Should().BeFalse();
            refreshToken.IsActive.Should().BeFalse();

            // Cambiar Expires para probar otro caso de IsActive/IsExpired
            refreshToken.Expires = testDate.AddDays(-1); // Expirado
            refreshToken.IsRevoked = false; // No revocado
            refreshToken.IsExpired.Should().BeTrue();
            refreshToken.IsActive.Should().BeFalse();

            refreshToken.Expires = testDate.AddDays(1); // No expirado
            refreshToken.IsRevoked = false; // No revocado
            refreshToken.IsExpired.Should().BeFalse();
            refreshToken.IsActive.Should().BeTrue();
        }
    }
}
