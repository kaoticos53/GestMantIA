using FluentAssertions;
using GestMantIA.Core.Identity.Entities;
using GestMantIA.Core.Identity.Interfaces; // Para SecurityAlertSeverity

namespace GestMantIA.Core.UnitTests.Identity.Entities
{
    public class SecurityAlertTests
    {
        [Fact]
        public void Constructor_Should_Initialize_Properties_Correctly()
        {
            // Arrange & Act
            var alert = new SecurityAlert();

            // Assert
            // Propiedades de BaseEntity
            alert.Id.Should().NotBe(Guid.Empty);
            alert.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            alert.IsDeleted.Should().BeFalse();
            alert.UpdatedAt.Should().BeNull();
            alert.DeletedAt.Should().BeNull();
            alert.DeletedBy.Should().BeNull();

            // Propiedades de SecurityAlert inicializadas en el constructor
            alert.Title.Should().Be(string.Empty);
            alert.Message.Should().Be(string.Empty);
            alert.ResolutionNotes.Should().Be(string.Empty);

            // Propiedades de SecurityAlert con valores por defecto de C#
            alert.Severity.Should().Be(default(SecurityAlertSeverity)); // El valor por defecto de un enum es su primer miembro (0)
            alert.RelatedEventId.Should().BeNull();
            alert.IsResolved.Should().BeFalse();
            alert.ResolvedAt.Should().BeNull();
            alert.ResolvedById.Should().BeNull();
            alert.ResolvedBy.Should().BeNull();
        }

        [Fact]
        public void Properties_Should_Set_And_Get_Correctly()
        {
            // Arrange
            var alert = new SecurityAlert();
            var testUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "resolverUser" };
            var testDate = DateTimeOffset.UtcNow;
            var relatedEventGuid = Guid.NewGuid();

            // Act
            alert.Title = "Suspicious Login Attempt";
            alert.Message = "A suspicious login attempt was detected from IP 1.2.3.4.";
            alert.Severity = SecurityAlertSeverity.High; // Asumiendo que High es un valor del enum
            alert.RelatedEventId = relatedEventGuid;
            alert.IsResolved = true;
            alert.ResolvedAt = testDate;
            alert.ResolutionNotes = "User confirmed it was not them. Account secured.";
            alert.ResolvedById = testUser.Id;
            alert.ResolvedBy = testUser;
            // Propiedades de BaseEntity
            alert.UpdatedAt = testDate.DateTime.AddHours(-1);
            alert.IsDeleted = true;
            alert.DeletedAt = testDate.DateTime;
            alert.DeletedBy = Guid.NewGuid();

            // Assert
            alert.Title.Should().Be("Suspicious Login Attempt");
            alert.Message.Should().Be("A suspicious login attempt was detected from IP 1.2.3.4.");
            alert.Severity.Should().Be(SecurityAlertSeverity.High);
            alert.RelatedEventId.Should().Be(relatedEventGuid);
            alert.IsResolved.Should().BeTrue();
            alert.ResolvedAt.Should().Be(testDate);
            alert.ResolutionNotes.Should().Be("User confirmed it was not them. Account secured.");
            alert.ResolvedById.Should().Be(testUser.Id);
            alert.ResolvedBy.Should().Be(testUser);
            alert.ResolvedBy.UserName.Should().Be("resolverUser");

            alert.UpdatedAt.Should().Be(testDate.DateTime.AddHours(-1));
            alert.IsDeleted.Should().BeTrue();
            alert.DeletedAt.Should().Be(testDate.DateTime);
            alert.DeletedBy.Should().NotBeEmpty();
        }

        [Fact]
        public void MarkAsResolved_Should_Update_Properties_When_Not_Resolved()
        {
            // Arrange
            var alert = new SecurityAlert { IsResolved = false };
            var resolverId = Guid.NewGuid();
            var notes = "Issue resolved by applying patch XYZ.";

            // Act
            alert.MarkAsResolved(resolverId, notes);

            // Assert
            alert.IsResolved.Should().BeTrue();
            alert.ResolvedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
            alert.ResolvedById.Should().Be(resolverId);
            alert.ResolutionNotes.Should().Be(notes);
        }

        [Fact]
        public void MarkAsResolved_Should_Handle_Null_Notes_Correctly()
        {
            // Arrange
            var alert = new SecurityAlert { IsResolved = false };
            var resolverId = Guid.NewGuid();

            // Act
            alert.MarkAsResolved(resolverId, null); // notes es null

            // Assert
            alert.IsResolved.Should().BeTrue();
            alert.ResolvedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
            alert.ResolvedById.Should().Be(resolverId);
            alert.ResolutionNotes.Should().BeNull(); // Debe ser null según la lógica del método
        }

        [Fact]
        public void MarkAsResolved_Should_Not_Update_Properties_When_Already_Resolved()
        {
            // Arrange
            var originalResolvedAt = DateTimeOffset.UtcNow.AddHours(-1);
            var originalResolverId = Guid.NewGuid();
            var originalNotes = "Already resolved.";

            var alert = new SecurityAlert
            {
                IsResolved = true,
                ResolvedAt = originalResolvedAt,
                ResolvedById = originalResolverId,
                ResolutionNotes = originalNotes
            };

            var newResolverId = Guid.NewGuid();
            var newNotes = "Attempting to re-resolve.";

            // Act
            alert.MarkAsResolved(newResolverId, newNotes);

            // Assert
            alert.IsResolved.Should().BeTrue();
            alert.ResolvedAt.Should().Be(originalResolvedAt);
            alert.ResolvedById.Should().Be(originalResolverId);
            alert.ResolutionNotes.Should().Be(originalNotes);
        }

        [Fact]
        public void Reopen_Should_Update_Properties_When_Resolved()
        {
            // Arrange
            var alert = new SecurityAlert
            {
                IsResolved = true,
                ResolvedAt = DateTimeOffset.UtcNow,
                ResolvedById = Guid.NewGuid(),
                ResolutionNotes = "Initial resolution notes."
            };

            // Act
            alert.Reopen();

            // Assert
            alert.IsResolved.Should().BeFalse();
            alert.ResolvedAt.Should().BeNull();
            alert.ResolvedById.Should().BeNull();
            alert.ResolutionNotes.Should().Be(string.Empty); // Según la implementación, se establece a string.Empty
        }

        [Fact]
        public void Reopen_Should_Not_Update_Properties_When_Not_Resolved()
        {
            // Arrange
            var alert = new SecurityAlert
            {
                IsResolved = false, // Ya no está resuelta
                ResolvedAt = null,
                ResolvedById = null,
                ResolutionNotes = string.Empty
            };
            var originalNotes = alert.ResolutionNotes; // Capturar el estado original

            // Act
            alert.Reopen();

            // Assert
            alert.IsResolved.Should().BeFalse();
            alert.ResolvedAt.Should().BeNull();
            alert.ResolvedById.Should().BeNull();
            alert.ResolutionNotes.Should().Be(originalNotes); // Debe permanecer igual
        }
    }
}
