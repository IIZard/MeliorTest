using Melior.InterviewQuestion.Data;
using Melior.InterviewQuestion.Interfaces;
using Melior.InterviewQuestion.Services;
using Melior.InterviewQuestion.StartUp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System.Reflection;

namespace Melior.InterviewQuestion.UnitTests.Tests.StartUp
{
    [TestFixture]
    public class ConfigurePaymentServiceServicesTests
    {
        private IServiceCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = Host.CreateEmptyApplicationBuilder(default).Services;
        }

        [Test]
        public void AddPaymentServices_ScopesAreValid()
        {
            var fakeConfigSettings = new Dictionary<string, string> {
                { "PaymentServiceOptions:DataStoreType", String.Empty },
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(fakeConfigSettings)
                .Build();
            _sut.AddPaymentServices(configuration);

            Assert.DoesNotThrow(() => _sut.BuildServiceProvider(true));
        }

        [Test]
        public void AddPaymentServices_ConfigurationTest_ExpectsBackup()
        {
            var fakeConfigSettings = new Dictionary<string, string> {
                {"PaymentServiceOptions:DataStoreType", ConfigurePaymentServiceServices.BackupDataStoreName},
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(fakeConfigSettings)
                .Build();
            _sut.AddPaymentServices(configuration);
            var services = _sut.BuildServiceProvider();

            var expectedservice = services.GetRequiredService<IPaymentService>();

            var propertyInfo = typeof(PaymentService).GetField("_accountDataStore", BindingFlags.NonPublic | BindingFlags.Instance);
            var injectedAccountDataStore = propertyInfo.GetValue(expectedservice);
            Assert.That(injectedAccountDataStore.GetType(), Is.EqualTo(typeof(BackupAccountDataStore)));
        }

        [Test]
        public void AddPaymentServices_ConfigurationTest_ExpectsLive()
        {
            var fakeConfigSettings = new Dictionary<string, string> {
                {"PaymentServiceOptions:DataStoreType", "LiveDataStore"},
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(fakeConfigSettings)
                .Build();
            _sut.AddPaymentServices(configuration);
            var services = _sut.BuildServiceProvider();

            var expectedservice = services.GetRequiredService<IPaymentService>();

            var propertyInfo = typeof(PaymentService).GetField("_accountDataStore", BindingFlags.NonPublic | BindingFlags.Instance);
            var injectedAccountDataStore = propertyInfo.GetValue(expectedservice);
            Assert.That(injectedAccountDataStore.GetType(), Is.EqualTo(typeof(AccountDataStore)));
        }
    }
}
