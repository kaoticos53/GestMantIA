using FluentAssertions;
using GestMantIA.Core.Identity.Entities;

namespace GestMantIA.Core.UnitTests.Identity.Entities
{
    public class ApplicationPermissionTests
    {
        [Fact]
        public void Constructor_Should_Initialize_Properties_Correctly()
        {
            // Arrange & Act
            var permission = new ApplicationPermission();

            // Assert
            permission.Id.Should().Be(Guid.Empty); // No se inicializa en el constructor
            permission.Name.Should().Be(string.Empty);
            permission.Description.Should().Be(string.Empty);
            permission.Category.Should().Be(string.Empty);
            permission.ClaimType.Should().Be(string.Empty);
            permission.ClaimValue.Should().Be(string.Empty);
            permission.RolePermissions.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void Properties_Should_Set_And_Get_Correctly()
        {
            // Arrange
            var permission = new ApplicationPermission();
            var testId = Guid.NewGuid();
            var rolePermissions = new HashSet<ApplicationRolePermission>
            {
                new ApplicationRolePermission { PermissionId = testId, RoleId = Guid.NewGuid() }
            };

            // Act
            permission.Id = testId;
            permission.Name = "Permissions.ReadWrite";
            permission.Description = "Allows reading and writing permissions.";
            permission.Category = "Administration";
            permission.ClaimType = "permission";
            permission.ClaimValue = "Permissions.ReadWrite";
            permission.RolePermissions = rolePermissions;

            // Assert
            permission.Id.Should().Be(testId);
            permission.Name.Should().Be("Permissions.ReadWrite");
            permission.Description.Should().Be("Allows reading and writing permissions.");
            permission.Category.Should().Be("Administration");
            permission.ClaimType.Should().Be("permission");
            permission.ClaimValue.Should().Be("Permissions.ReadWrite");
            permission.RolePermissions.Should().BeSameAs(rolePermissions);
            permission.RolePermissions.Should().HaveCount(1);
        }
    }
}
