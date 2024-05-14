using Melior.InterviewQuestion.StartUp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Melior.InterviewQuestion.UnitTests.Tests.StartUp
{
    [TestFixture]
    public class ConfigurePaymentServiceServicesTests
    {
        public IServiceCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = Host.CreateEmptyApplicationBuilder(default).Services;
        }

        [Test]
        public void AddPaymentServices_ScopesAreValid()
        {
            _sut.AddPaymentServices();

            Assert.DoesNotThrow(() => _sut.BuildServiceProvider(true));
        }
    }
}
