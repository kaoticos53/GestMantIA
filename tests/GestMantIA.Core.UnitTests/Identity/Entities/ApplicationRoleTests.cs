using FluentAssertions;
using GestMantIA.Core.Identity.Entities;
using System;
using System.Collections.Generic;
using Xunit;

namespace GestMantIA.Core.UnitTests.Identity.Entities
{
    public class ApplicationRoleTests
    {
        [Fact]
        public void ParameterlessConstructor_Should_Initialize_Properties_Correctly()
        {
            // Arrange & Act
            var role = new ApplicationRole();

            // Assert
            role.Id.Should().NotBe(Guid.Empty);
            role.Name.Should().BeNull(); // IdentityRole() base constructor doesn't set Name
            role.NormalizedName.Should().BeNull(); // Not set by constructor
            role.Description.Should().Be(string.Empty);
            role.UserRoles.Should().NotBeNull().And.BeEmpty();
            role.RolePermissions.Should().NotBeNull().And.BeEmpty();
            role.CreatedAt.Should().NotBe(default);
            role.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
            role.UpdatedAt.Should().BeNull();
            role.ConcurrencyStamp.Should().NotBeNullOrEmpty(); // From IdentityRole
        }

        [Fact]
        public void Constructor_WithRoleName_Should_Initialize_Properties_Correctly()
        {
            // Arrange
            string roleName = "TestRole";

            // Act
            var role = new ApplicationRole(roleName);

            // Assert
            role.Id.Should().NotBe(Guid.Empty);
            role.Name.Should().Be(roleName);
            role.NormalizedName.Should().BeNull(); // Not set by constructor, only by NormalizeName() or by Identity framework
            role.Description.Should().Be(string.Empty);
            role.UserRoles.Should().NotBeNull().And.BeEmpty();
            role.RolePermissions.Should().NotBeNull().And.BeEmpty();
            role.CreatedAt.Should().NotBe(default);
            role.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
            role.UpdatedAt.Should().BeNull();
            role.ConcurrencyStamp.Should().NotBeNullOrEmpty(); // From IdentityRole
        }

        [Fact]
        public void Properties_Should_Set_And_Get_Correctly()
        {
            // Arrange
            var role = new ApplicationRole("InitialRole");
            var testDate = DateTime.UtcNow.AddDays(-1);
            var userRoles = new HashSet<ApplicationUserRole> { new ApplicationUserRole { UserId = Guid.NewGuid(), RoleId = role.Id } };
            var rolePermissions = new HashSet<ApplicationRolePermission> { new ApplicationRolePermission { RoleId = role.Id, PermissionId = Guid.NewGuid() } };

            // Act
            role.Name = "UpdatedRoleName";
            role.NormalizedName = "UPDATEDROLENAME"; // Manually set for testing get
            role.Description = "This is an updated description.";
            role.UserRoles = userRoles;
            role.RolePermissions = rolePermissions;
            // CreatedAt is set by constructor, test setting UpdatedAt
            role.UpdatedAt = testDate;
            role.ConcurrencyStamp = "new-concurrency-stamp";

            // Assert
            role.Name.Should().Be("UpdatedRoleName");
            role.NormalizedName.Should().Be("UPDATEDROLENAME");
            role.Description.Should().Be("This is an updated description.");
            role.UserRoles.Should().BeSameAs(userRoles); // Check collection instance
            role.UserRoles.Should().HaveCount(1);
            role.RolePermissions.Should().BeSameAs(rolePermissions);
            role.RolePermissions.Should().HaveCount(1);
            role.UpdatedAt.Should().Be(testDate);
            role.ConcurrencyStamp.Should().Be("new-concurrency-stamp");
        }

        [Fact]
        public void NormalizeName_Should_Set_NormalizedName_To_Uppercase()
        {
            // Arrange
            var role = new ApplicationRole("MixedCaseRole");

            // Act
            role.NormalizeName();

            // Assert
            role.NormalizedName.Should().Be("MIXEDCASEROLE");
        }

        [Fact]
        public void NormalizeName_WithNullName_Should_Set_NormalizedName_To_EmptyString()
        {
            // Arrange
            var role = new ApplicationRole(); // Name will be null
            role.Name = null; // Explicitly ensure Name is null

            // Act
            role.NormalizeName();

            // Assert
            role.NormalizedName.Should().Be(string.Empty);
        }

        [Fact]
        public void NormalizeName_WithEmptyName_Should_Set_NormalizedName_To_EmptyString()
        {
            // Arrange
            var role = new ApplicationRole(string.Empty);

            // Act
            role.NormalizeName();

            // Assert
            role.NormalizedName.Should().Be(string.Empty);
        }
    }
}
