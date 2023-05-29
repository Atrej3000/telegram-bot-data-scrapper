using DataParserService.Controllers;
using DataParserService.Models;
using DataParserService.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using System.Net;

namespace DataParserServiceTests
{
    [TestFixture]
    public class DataParserControllerTests
    {
        private Mock<IProxyProviderService> _proxyProviderServiceMock;
        private Mock<IUserAgentProviderService> _userAgentProviderServiceMock;
        private Mock<IDataScrapperService> _dataScrapperServiceMock;
        private DataParserController _controller;

    }
}