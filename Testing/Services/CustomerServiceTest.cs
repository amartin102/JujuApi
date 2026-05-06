using Application.Dtos.Post;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Moq;
using Repository.Interface;
using Repository.Logger;
using Xunit;

namespace Testing.Services
{
    public  class CustomerServiceTest
    {
        private readonly Mock<IPostRepository> _postRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogInterface> _logServiceMock;
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly PostService _postService;

        public CustomerServiceTest()
        {
            _postRepositoryMock = new Mock<IPostRepository>();
            _mapperMock = new Mock<IMapper>();
            _logServiceMock = new Mock<ILogInterface>();
            _customerRepositoryMock = new Mock<ICustomerRepository>();

            _postService = new PostService(
                _postRepositoryMock.Object,
                _mapperMock.Object,
                _logServiceMock.Object,
                _customerRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Create_ShouldReturn201_WhenCustomerExists()
        {
            var dto = new CreatePostDto { CustomerId = 1, Body = "Test" };
            _customerRepositoryMock.Setup(r => r.GetById(1)).ReturnsAsync(new CustomerEntity { State = true });
            _mapperMock.Setup(m => m.Map<PostEntity>(dto)).Returns(new PostEntity());
            _postRepositoryMock.Setup(r => r.Create(It.IsAny<PostEntity>())).ReturnsAsync(new PostEntity());

            var result = await _postService.Create(dto);

            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
        }

        [Fact]
        public async Task Create_ShouldReturn404_WhenCustomerDoesNotExist()
        {           
            var dto = new CreatePostDto { CustomerId = 99 };
            _customerRepositoryMock.Setup(r => r.GetById(99)).ReturnsAsync((CustomerEntity)null);
                       
            var result = await _postService.Create(dto);
                      
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("does not exist", result.Message);
        }

        [Fact]
        public async Task Delete_ShouldReturn204_WhenPostDeleted()
        {
            _postRepositoryMock.Setup(r => r.Delete(1)).Returns(Task.FromResult(true));

            var result = await _postService.Delete(1);

            Assert.True(result.Data);
            Assert.Equal(204, result.StatusCode);
        }

        [Fact]
        public async Task Delete_ShouldReturn404_WhenPostNotFound()
        {
            _postRepositoryMock.Setup(r => r.Delete(1)).Returns(Task.FromResult(false));

            var result = await _postService.Delete(1);

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetById_ShouldReturnPost_WhenExists()
        {
            var entity = new PostEntity { PostId = 1 };
            _postRepositoryMock.Setup(r => r.GetById(1)).Returns(Task.FromResult(entity));
            _mapperMock.Setup(m => m.Map<GetPostDto>(entity)).Returns(new GetPostDto { PostId = 1 });

            var result = await _postService.GetById(1);

            Assert.NotNull(result.Data);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetById_ShouldReturn404_WhenNotExists()
        {
            _postRepositoryMock.Setup(r => r.GetById(1)).Returns(Task.FromResult((PostEntity)null));

            var result = await _postService.GetById(1);

            Assert.Null(result.Data);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task Update_ShouldReturnSuccess_WhenPostIsUpdated()
        {
            var dto = new UpdatePostDto { PostId = 1 };
            var entity = new PostEntity { PostId = 1 };
            _postRepositoryMock.Setup(r => r.GetById(1)).ReturnsAsync(entity);
            _postRepositoryMock.Setup(r => r.Update(entity)).ReturnsAsync((entity, true));
            _mapperMock.Setup(m => m.Map<GetPostDto>(entity)).Returns(new GetPostDto());

            var result = await _postService.Update(dto);

            Assert.True(result.Data.changed);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task Update_ShouldReturn404_WhenPostNotFound()
        {
            _postRepositoryMock.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync((PostEntity)null);

            var result = await _postService.Update(new UpdatePostDto { PostId = 1 });

            Assert.Equal(404, result.StatusCode);
            Assert.False(result.Success);
        }
    }
}
