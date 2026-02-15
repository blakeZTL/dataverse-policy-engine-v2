using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.FakeMessageExecutors;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using FakeXrmEasy.Middleware.Messages;
using FakeXrmEasy.Plugins.Middleware.CustomApis;
using Microsoft.Xrm.Sdk;
using System.Reflection;

namespace DataversePolicyEngine.Tests
{
    public class FakeXrmEasyTestBase
    {
        protected readonly IOrganizationService _service;
        protected readonly IXrmFakedContext _context;

        public FakeXrmEasyTestBase()
        {
            _context = MiddlewareBuilder
                .New()
                .AddCrud()
                .AddFakeMessageExecutors(
                    Assembly.GetAssembly(typeof(AddListMembersListRequestExecutor))
                )
                //.AddCustomApiFakeMessageExecutors(
                //    Assembly.GetAssembly(
                //        typeof(CustomApiSetTraineeWeekAndShiftStatusFakeMessageExecutor)
                //    )
                //)
                .UseCrud()
                .UseMessages()
                .SetLicense(FakeXrmEasyLicense.RPL_1_5)
                .Build();
            _service = _context.GetOrganizationService();
        }
    }
}
