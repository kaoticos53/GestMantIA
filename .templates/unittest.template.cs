using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using GestMantIA.Core.Application.Features.{{FeatureName}}.Commands;
using GestMantIA.Core.Application.Features.{{FeatureName}}.Queries;
using GestMantIA.Core.Domain.Entities;

namespace GestMantIA.UnitTests.Application.Features.{{FeatureName}}
{
    public class {{TestName}}Tests
    {
        private readonly Mock<IRepository<{{EntityName}}>> _mockRepository;
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<{{HandlerName}}>> _mockLogger;

        public {{TestName}}Tests()
        {
            _mockRepository = new Mock<IRepository<{{EntityName}}>>();
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<{{HandlerName}}>>();
        }

        [Fact]
        public async Task {{TestName}}_Should{{ExpectedBehavior}}_When{{Condition}}()
        {
            // Arrange
            var command = new {{CommandName}}
            {
                // Inicializar propiedades del comando
            };

            var handler = new {{HandlerName}}(
                _mockRepository.Object,
                _mockMediator.Object,
                _mockLogger.Object);

            // Configurar mocks
            _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new {{EntityName}}());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            // Verificar el resultado esperado
            Assert.NotNull(result);
            // Agregar más aserciones según sea necesario
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task {{TestName}}_ShouldThrowValidationException_When{{InvalidCondition}}(string invalidValue)
        {
            // Arrange
            var command = new {{CommandName}}
            {
                // Inicializar con valor inválido
                PropertyName = invalidValue
            };

            var handler = new {{HandlerName}}(
                _mockRepository.Object,
                _mockMediator.Object,
                _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => 
                handler.Handle(command, CancellationToken.None));
        }
    }
}
