using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Business_Logic;
using MTCG.Data_Access.Interfaces;
using MTCG.Http;
using MTCG.Http.Endpoints;
using NSubstitute;

namespace MTCG.Test.Endpoints
{
    [TestFixture]
    public class UserEPTests
    {
        private UserEP _userEP;
        private UserHandler _userHandler;
        private IUserRepo _userRepo;

        [SetUp]
        public void Setup()
        {
            _userRepo = Substitute.For<IUserRepo>();
            _userHandler = new UserHandler(_userRepo);
            _userEP = new UserEP(_userHandler);
        }

        [Test]
        public void HandleRequest_ValidRegistration_Returns201()
        {
            // Arrange
            var requestContent = "{\"Username\":\"testuser\",\"Password\":\"password123\"}";
            var request = CreateHttpRequest(MTCG.Http.HttpMethod.POST, new[] { "", "users" }, requestContent);
            var response = CreateHttpResponse();
            _userRepo.RegisterUser("testuser", "password123").Returns(true);

            // Act
            var result = _userEP.HandleRequest(request, response);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(response.ResponseCode, Is.EqualTo(201));
            Assert.That(response.ResponseMessage, Is.EqualTo("User created"));
        }

        [Test]
        public void HandleRequest_EmptyContent_Returns400()
        {
            // Arrange
            var request = CreateHttpRequest(MTCG.Http.HttpMethod.POST, new[] { "", "users" }, "");
            var response = CreateHttpResponse();

            // Act
            var result = _userEP.HandleRequest(request, response);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(response.ResponseCode, Is.EqualTo(400));
            Assert.That(response.ResponseMessage, Is.EqualTo("No content provided"));
        }

        [Test]
        public void HandleRequest_DuplicateUser_Returns409()
        {
            // Arrange
            var requestContent = "{\"Username\":\"testuser\",\"Password\":\"password123\"}";
            var request = CreateHttpRequest(MTCG.Http.HttpMethod.POST, new[] { "", "users" }, requestContent);
            var response = CreateHttpResponse();
            _userRepo.RegisterUser("testuser", "password123").Returns(false);

            // Act
            var result = _userEP.HandleRequest(request, response);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(response.ResponseCode, Is.EqualTo(409));
            Assert.That(response.ResponseMessage, Is.EqualTo("User already exists"));
        }

        private HttpRequest CreateHttpRequest(MTCG.Http.HttpMethod method, string[] path, string content)
        {
            var reader = new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)));
            var request = new HttpRequest(reader);
            request.GetType().GetProperty("Method")?.SetValue(request, method);
            request.GetType().GetProperty("Path")?.SetValue(request, path);
            request.GetType().GetProperty("Content")?.SetValue(request, content);
            return request;
        }

        private HttpResponse CreateHttpResponse()
        {
            var writer = new StreamWriter(new MemoryStream());
            return new HttpResponse(writer);
        }
    }
}
