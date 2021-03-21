using Xunit;
using Moq;
using System;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationEvaluatorShould
    {
        [Fact]
        public void AcceptHighIncomeAppications()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator =
                new Mock<IFrequentFlyerNumberValidator>();

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { GrossAnnualIncome = 100_000 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void ReferYoungApplicationEvaluator()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.DefaultValue = DefaultValue.Mock;

            mockValidator.Setup(X => X.IsValid(It.IsAny<string>())).Returns(true);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 19 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplication()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("Ok");

            // mockValidator.Setup(x => x.IsValid("x")).Returns(true);
            // mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            // mockValidator.Setup(x => x.IsValid(It.Is<string>(number => number.StartsWith("y")))).Returns(true);
            // mockValidator.Setup(x => x.IsValid(It.IsInRange("a", "z", Range.Inclusive))).Returns(true);
            // mockValidator.Setup(x => x.IsValid(It.IsIn("z", "y", "x"))).Returns(true);
            mockValidator.Setup(x => x.IsValid(It.IsRegex("[a-z]"))).Returns(true);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 19_999,
                Age = 42,
                FrequentFlyerNumber = "y"
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void ReferInvalidFrequentFlyerApplications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("Ok");

            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(false);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication();

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        //[Fact]
        // public void DeclineLowIncomeApplicationOutDemo()
        // {
        //     var mockValidator = new Mock<IFrequentFlyerNumberValidator>(MockBehavior.Strict);

        //     bool isValid = true;

        //     mockValidator.Setup(x => x.IsValid(It.IsAny<string>(), out isValid));

        //     var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

        //     var application = new CreditCardApplication
        //     {
        //         GrossAnnualIncome = 19_999,
        //         Age = 42
        //     };

        //     CreditCardApplicationDecision decision = sut.EvaluateUsingOut(application);

        //     Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        // }

        [Fact]
        public void ReferWhenLicenceKeyExpired()
        {
            // var mokLicenseData = new Mock<ILicenseData>();
            // mokLicenseData.Setup(x => x.LicenseKey).Returns("EXPIRED");

            // var mokServiceInfo = new Mock<IServiceInformation>();
            // mokServiceInfo.Setup(x => x.License).Returns(mokLicenseData.Object);

            // var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            // mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            // mockValidator.Setup(x => x.ServiceInformation).Returns(mokServiceInfo.Object);

            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("EXPIRED");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 42 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        private string GetLicenceKeyExpiryString()
        {
            // E.g. read from vendor-supplied constant file
            return "EXPIRED";
        }

        [Fact]
        public void UseDetailedLookupForOlderApplications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            //mockValidator.SetupProperty(x => x.ValidationMode);
            mockValidator.SetupAllProperties();

            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("Ok");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 30 };

            sut.Evaluate(application);

            Assert.Equal(ValidationMode.Detailed, mockValidator.Object.ValidationMode);
        }
    }
}
