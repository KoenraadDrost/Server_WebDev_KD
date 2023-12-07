using NUnit.Framework;
using System;
using Setup.Models;

namespace ServerWebDevKDTests
{
    [TestFixture]
    public class ModelTests
    {
        public static IEnumerable<TestCaseData> InputModelVallidationTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new List<string> { "Username", "Email.Address@gmail.com", "Password", "Password" },
                    new List<string> { "Username", "Email.Address@gmail.com", "Password", "Password" },
                    ""
                    ); // Test if valid user succeeds.
                yield return new TestCaseData(
                    new List<string> { "A", "Email.Address@gmail.com", "Password", "Password" },
                    new List<string> { null, null, null, null },
                    "Username doesn't have the required length, please try a different username."
                    ); // Test Username length requirement exception.
                yield return new TestCaseData(
                    new List<string> { "Username", "NotAnEmailAdress", "Password", "Password" },
                    new List<string> { "Username", null, null, null },
                    "The Email field is not a valid e-mail address."
                    ); // Test for propper Email exception.
                yield return new TestCaseData(
                    new List<string> { "Username", "Email.Address@gmail.com", "Pass", "Pass" },
                    new List<string> { "Username", "Email.Address@gmail.com", null, null },
                    "Password not strong enough to be secure, please try a different password."
                    ); // Test Password length requirement exception.
                yield return new TestCaseData(
                    new List<string> { "Username", "Email.Address@gmail.com", "Password", "NotPassword" },
                    new List<string> { "Username", "Email.Address@gmail.com", "Password", null },
                    "The password and confirmation password do not match."
                    ); // Test Password confirmation exception.
            }
        }

        [TestCaseSource("InputModelVallidationTestCases")]
        public void InputModelVallidation(List<string> inputParameters, List<string> expectedResults, string expectedException)
        {
            // Arrange
            InputModel testModel = new InputModel();
            string exceptionMessage = "";

            // Act
            try
            {
                testModel.Username = inputParameters[0];
                testModel.Email = inputParameters[1];
                testModel.Password = inputParameters[2];
                testModel.ConfirmPassword = inputParameters[3];
            }
            catch (Exception e)
            {
                exceptionMessage = e.Message;
            }

            // Assert
            Assert.That(expectedResults[0], Is.EqualTo(testModel.Username));
            Assert.That(expectedResults[1], Is.EqualTo(testModel.Email));
            Assert.That(expectedResults[2], Is.EqualTo(testModel.Password));
            Assert.That(expectedResults[3], Is.EqualTo(testModel.ConfirmPassword));

            Assert.That(expectedException, Is.EqualTo(exceptionMessage));
        }
    }
}