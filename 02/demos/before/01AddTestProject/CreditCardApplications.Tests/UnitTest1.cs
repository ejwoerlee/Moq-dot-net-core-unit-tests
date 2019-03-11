using System;
using Xunit;
using Moq;
using Moq.Protected;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationEvaluatorShould
    {
        private Mock<IFrequentFlyerNumberValidator> mockValidator;
        private CreditCardApplicationEvaluator sut;

        /// <summary>
        /// ctor called before each and every test
        /// </summary>
        public CreditCardApplicationEvaluatorShould()
        {
            mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.SetupAllProperties();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            sut = new CreditCardApplicationEvaluator(mockValidator.Object);
        }
        
        [Fact]
        public void AcceptHightIncomeApplications()
        {
            // Arrange -> could be removed when using constructor
            Mock<IFrequentFlyerNumberValidator> mockValidator = 
                new Mock<IFrequentFlyerNumberValidator>();
            
            //could be removed when using constructor
            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            
            // Act
            var application = new CreditCardApplication() {GrossAnnualIncome = 100_000};
            CreditCardApplicationDecision result = sut.Evaluate(application);

            // Assert
            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, result);
        }

        [Fact]
        public void ReferYoungApplications()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator =
                new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            // mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey);
            mockValidator.DefaultValue = DefaultValue.Mock;
                         

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            
            var application = new CreditCardApplication { Age = 19 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }


        [Fact]
        public void DeclineLowIncomeApplications()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator =
                new Mock<IFrequentFlyerNumberValidator>();

            // mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            // mockValidator.Setup(x => x.IsValid(It.Is<string>(number => number.StartsWith('x'))))
            //              .Returns(true);
            //mockValidator.Setup(x => x.IsValid(It.IsIn("x", "y", "z")))
            //             .Returns(true);
            //mockValidator.Setup(x => x.IsValid(It.IsInRange("a", "z", Range.Inclusive)))
            //             .Returns(true);
            mockValidator.Setup(x => x.IsValid(It.IsRegex("[a-z]", System.Text.RegularExpressions.RegexOptions.None)))
                         .Returns(true);
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication {
                GrossAnnualIncome = 19_999,
                Age = 42,
                FrequentFlyerNumber = "x"
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void ReferInvalidFrequentFlyerApplications()
        {
            // MockBehavior.Strict => faalt als niet allles (elke property/method) is
            // geinitialiseerd middels Setup etc. Gebruikt in dat geval geen default waarden..
            //Mock<IFrequentFlyerNumberValidator> mockValidator =
            //    new Mock<IFrequentFlyerNumberValidator>(MockBehavior.Strict);

            Mock<IFrequentFlyerNumberValidator> mockValidator = 
                new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(false);
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        //   [Fact]
       // public void DeclineLowIncomeApplicationsOutDemo()
        //{
        //    Mock<IFrequentFlyerNumberValidator> mockValidator =
        //        new Mock<IFrequentFlyerNumberValidator>();

        //    bool isValid = true;
        //    mockValidator.Setup(x => x.IsValid(It.IsAny<string>(), out isValid));

        //    var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        //    var application = new CreditCardApplication {
        //        GrossAnnualIncome = 19_999,
        //        Age = 42,
        //        FrequentFlyerNumber = "a"
        //    };

        //    CreditCardApplicationDecision decision = sut.EvaluateUsingOut(application);

        //    Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        //}

        [Fact]
        public void ReferWhenLicenseKeyExpired()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            // mockValidator.Setup(x => x.LicenseKey).Returns(GetLicenseKeyExpiryString);

            //var mockLicenseData = new Mock<ILicenseData>();
            //mockLicenseData.Setup(x => x.LicenseKey).Returns(GetLicenseKeyExpiryString);

            //var mockServiceInfo = new Mock<IServiceInformation>();
            //mockServiceInfo.Setup(x => x.License).Returns(mockLicenseData.Object);

            //mockValidator.Setup(x => x.ServiceInformation).Returns(mockServiceInfo.Object);

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey)
                         .Returns(GetLicenseKeyExpiryString);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 42 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        string GetLicenseKeyExpiryString() => "EXPIRED";

        [Fact]
        public void UseDetailedLookupForOlderApplications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.SetupAllProperties(); // enable change tracking to all Mock properties
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            // mockValidator.SetupProperty(x => x.ValidationMode); // enable change tracking to this Mock property

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 30 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(ValidationMode.Detailed, mockValidator.Object.ValidationMode);
        }

        /// <summary>
        /// mockValidator.Verify()
        /// Verifies that a specific invocation matching the given expression was performed on the mock. 
        /// </summary>
        [Fact]
        public void ShouldValidateFrequentFlyerNumberForLowIncomeApplications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { FrequentFlyerNumber = "q" };

            sut.Evaluate(application);

            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), "Failure text");
        }

        /// <summary>
        /// mockValidator.Verify()
        /// Verifies that a specific invocation matching the given expression was NOT performed on the mock.
        /// Times.Never, Times.AtLeast, Times.Exactly(2) etc.
        /// </summary>
        [Fact]
        public void NotValidateFrequentFlyerNumberForHighIncomeApplications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { GrossAnnualIncome = 100_000 };

            sut.Evaluate(application);

            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Never, "IsValid should never have been called!");
        }

        // Verify property getters and setters were accessed..
        // mockValidator.VerifyGet()
        [Fact]
        public void CheckLicenseKeyForLowIncomeApplications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { GrossAnnualIncome = 99_000 };

            sut.Evaluate(application);

            mockValidator.VerifyGet(x => x.ServiceInformation.License.LicenseKey, Times.Once);
        }

        /// <summary>
        /// mockValidator.VerifySet()
        /// mockValidator.VerifySet(x => x.ValidationMode = It.IsAny<ValidationMode>(), Times.Once);
        /// </summary>
        [Fact]
        public void SetDetailedLookupForOlderApplications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 30 };

            sut.Evaluate(application);

            mockValidator.VerifySet(x => x.ValidationMode = It.IsAny<ValidationMode>(), Times.Once);
        }

        // mockValidator.Setup.Returns  -> .Setup.Throws(.....)
        [Fact]
        public void ReferWhenFrequentFlyerValidationError()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator
                = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                         .Throws(new Exception("Custom message"));

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 42 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        // mockValidator.Setup.Returns  -> .Setup.Raises(.....)
        [Fact]
        public void IncrementLookupCount()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                         .Returns(true)
                         .Raises(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { FrequentFlyerNumber = "x", Age = 25 };

            sut.Evaluate(application);

            //mockValidator.Raise(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);

            Assert.Equal(1, sut.ValidatorLookupCount);
        }

        /// <summary>
        /// Multiple returns/results
        /// mockValidator.SetupSequence()
        /// </summary>
        [Fact]
        public void ReferInvalidFrequentFlyerApplications_Sequence()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            mockValidator.SetupSequence(x => x.IsValid(It.IsAny<string>())) // chain multiple return methods for this setup
                         .Returns(false) // first invocation
                         .Returns(true); // second invocation

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 25 };

            CreditCardApplicationDecision firstDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, firstDecision);

            CreditCardApplicationDecision secondDecision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, secondDecision);
        }

        /// <summary>
        /// Mocking normal classes instead of Interfaces
        /// using Moq.Protected, mockFraudLookup.Protected().Setup, ItExpr.IsAny
        /// </summary>
        [Fact]
        public void ReferFraudRisk()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator =
                new Mock<IFrequentFlyerNumberValidator>();

            Mock<FraudLookup> mockFraudLookup = new Mock<FraudLookup>();
            //mockFraudLookup.Setup(x => x.IsFraudRisk(It.IsAny<CreditCardApplication>()))
            //               .Returns(true);

            mockFraudLookup.Protected()
                           .Setup<bool>("CheckApplication", ItExpr.IsAny<CreditCardApplication>())
                           .Returns(true);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object, mockFraudLookup.Object);

            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHumanFraudRisk, decision);
        }

        /// <summary>
        /// Mock.Of()
        /// </summary>
        [Fact]
        public void LinqToMocks()
        {
            //Mock<IFrequentFlyerNumberValidator> mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            //mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            //mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            IFrequentFlyerNumberValidator mockValidator
                = Mock.Of<IFrequentFlyerNumberValidator>
                (
                    validator =>
                        validator.ServiceInformation.License.LicenseKey == "OK" &&
                        validator.IsValid(It.IsAny<string>()) == true
                );


            // var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var sut = new CreditCardApplicationEvaluator(mockValidator);

            var application = new CreditCardApplication { Age = 25 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

    }
}
