using NUnit.Framework;
using System;
using Setup.Models;

namespace ServerWebDevKDTests
{
    [TestFixture]
    public class ModelTests
    {
        [TestCase("Username", "Email.Address@gmail.com", "Password", "Password")]
        public void InputModelVallidation(string username, string email, string password, string confirmPassword)
        {
            // Arrange
            InputModel testModel = new InputModel();

            // Act
            testModel.Username = username;
            testModel.Email = email;
            testModel.Password = password;
            testModel.ConfirmPassword = confirmPassword;

            // Assert
            Assert.That(username, Is.EqualTo(testModel.Username));
            Assert.That(email, Is.EqualTo(testModel.Email));
            Assert.That(password, Is.EqualTo(testModel.Password));
            Assert.That(confirmPassword, Is.EqualTo(testModel.ConfirmPassword));
        }
    }
}