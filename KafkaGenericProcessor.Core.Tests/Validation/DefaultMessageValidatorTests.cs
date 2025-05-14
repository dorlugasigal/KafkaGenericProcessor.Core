using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using KafkaGenericProcessor.Core.Validation;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace KafkaGenericProcessor.Core.Tests.Validation
{
    public class DefaultMessageValidatorTests
    {
        private readonly ILogger<DefaultMessageValidator<TestModel>> _logger;
        private readonly string _correlationId = "test-correlation-id";

        public DefaultMessageValidatorTests()
        {
            _logger = Substitute.For<ILogger<DefaultMessageValidator<TestModel>>>();
        }

        [Fact]
        public async Task ValidateAsync_WithValidMessage_ShouldReturnTrue()
        {
            // Arrange
            var validator = new DefaultMessageValidator<TestModel>(_logger);
            var message = new TestModel
            {
                Id = 1,
                Name = "Test Name",
                Email = "test@example.com"
            };

            // Act
            var isValid = await validator.ValidateAsync(message, _correlationId);

            // Assert
            isValid.ShouldBeTrue();
        }

        [Fact]
        public async Task GetValidationErrorsAsync_WithValidMessage_ShouldReturnEmptyList()
        {
            // Arrange
            var validator = new DefaultMessageValidator<TestModel>(_logger);
            var message = new TestModel
            {
                Id = 1,
                Name = "Test Name",
                Email = "test@example.com"
            };

            // Act
            var errors = await validator.GetValidationErrorsAsync(message, _correlationId);

            // Assert
            errors.ShouldBeEmpty();
        }

        #region Test Models

        public class TestModel
        {
            [Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
            public int Id { get; set; }
            
            [Required(ErrorMessage = "Name is required")]
            public string? Name { get; set; }
            
            [EmailAddress(ErrorMessage = "Invalid email format")]
            public string? Email { get; set; }
        }


        #endregion
    }
}