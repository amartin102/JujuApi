using Application.Dtos.Post;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Moq;
using Repository.Interface;
using Repository.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing.Services
{
    public class PostServiceTest
    {
        private readonly Mock<IPostRepository> _postRepoMock;
        private readonly Mock<ICustomerRepository> _customerRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogInterface> _logMock;
        private readonly PostService _service;

        public PostServiceTest()
        {
            _postRepoMock = new Mock<IPostRepository>();
            _customerRepoMock = new Mock<ICustomerRepository>();
            _mapperMock = new Mock<IMapper>();
            _logMock = new Mock<ILogInterface>();

            _service = new PostService(
                _postRepoMock.Object,
                _mapperMock.Object,
                _logMock.Object,
                _customerRepoMock.Object
            );
        }

        [Fact]
        public async Task Create_ShouldReturn404_WhenCustomerDoesNotExist()
        {
            var dto = new CreatePostDto { CustomerId = 99 };
            _customerRepoMock.Setup(x => x.GetById(99)).ReturnsAsync((CustomerEntity)null);

            var result = await _service.Create(dto);

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            _postRepoMock.Verify(x => x.Create(It.IsAny<PostEntity>()), Times.Never);
        }

        [Fact]
        public async Task Create_ShouldReturn201_WhenSuccess()
        {
            var dto = new CreatePostDto { CustomerId = 1, Body = "Valid Body" };
            var entity = new PostEntity { PostId = 10, Body = "Formatted Body" };

            _customerRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(new CustomerEntity { State = true });
            _mapperMock.Setup(x => x.Map<PostEntity>(dto)).Returns(entity);
            _postRepoMock.Setup(x => x.Create(entity)).ReturnsAsync(entity);
            _mapperMock.Setup(x => x.Map<GetPostDto>(entity)).Returns(new GetPostDto { PostId = 10 });

            var result = await _service.Create(dto);

            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task Delete_ShouldReturn204_WhenPostIsDeleted()
        {
            int id = 1;
            _postRepoMock.Setup(x => x.Delete(id)).Returns(Task.FromResult(true));

            var result = await _service.Delete(id);

            Assert.True(result.Success);
            Assert.Equal(204, result.StatusCode);
        }

        [Fact]
        public async Task Delete_ShouldReturn404_WhenPostNotFound()
        {
            int id = 1;
            _postRepoMock.Setup(x => x.Delete(id)).Returns(Task.FromResult(false));

            var result = await _service.Delete(id);

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetById_ShouldReturn200_WhenPostExists()
        {
            var entity = new PostEntity { PostId = 5 };
            _postRepoMock.Setup(x => x.GetById(5)).Returns(Task.FromResult(entity));
            _mapperMock.Setup(x => x.Map<GetPostDto>(entity)).Returns(new GetPostDto { PostId = 5 });

            var result = await _service.GetById(5);

            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetById_ShouldReturn404_WhenPostDoesNotExist()
        {
            _postRepoMock.Setup(x => x.GetById(It.IsAny<int>())).Returns(Task.FromResult((PostEntity)null));

            var result = await _service.GetById(1);

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Update_ShouldReturn200_WhenChangesAreApplied()
        {
            var dto = new UpdatePostDto { PostId = 1 };
            var entity = new PostEntity { PostId = 1 };
            _postRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(entity);
            _postRepoMock.Setup(x => x.Update(entity)).ReturnsAsync((entity, true));

            var result = await _service.Update(dto);

            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task Update_ShouldReturn404_WhenNoChangesWereMade()
        {
            var dto = new UpdatePostDto { PostId = 1 };
            var entity = new PostEntity { PostId = 1 };
            _postRepoMock.Setup(x => x.GetById(1)).ReturnsAsync(entity);
            _postRepoMock.Setup(x => x.Update(entity)).ReturnsAsync((entity, false));

            var result = await _service.Update(dto);

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }
    }
}
