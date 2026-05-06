using Application.Common.GenericResponse;
using Application.Common.Messages;
using Application.Dtos.Customer;
using Application.Services;
using Application.Services.Interface;
using AutoMapper;
using Domain.Entities;
using Moq;
using Repository.Interface;
using Repository.Logger;
using Xunit;

namespace Testing.Services
{
    public class CustomerServiceTest
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogInterface> _logServiceMock;
        private readonly Mock<IPostService> _postServiceMock;

        private readonly CustomerService _customerService;

        public CustomerServiceTest()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _mapperMock = new Mock<IMapper>();
            _logServiceMock = new Mock<ILogInterface>();
            _postServiceMock = new Mock<IPostService>();

            _customerService = new CustomerService(
                _customerRepositoryMock.Object,
                _mapperMock.Object,
                _logServiceMock.Object,
                _postServiceMock.Object
            );
        }

        [Fact]
        public async Task Create_ShouldReturn409_WhenCustomerAlreadyExists()
        {
            var dto = new CreateCustomerDto { Name = "Juan" };

            _customerRepositoryMock
                .Setup(r => r.GetByName(dto.Name))
                .ReturnsAsync(new CustomerEntity());

            var result = await _customerService.Create(dto);

            Assert.False(result.Success);
            Assert.Equal(409, result.StatusCode);
            Assert.Equal(ErrorMessages.AlreadyExists("Customer", "nombre"), result.Message);
        }

        [Fact]
        public async Task Create_ShouldReturn201_WhenCustomerCreated()
        {
            var dto = new CreateCustomerDto { Name = "Juan" };

            _customerRepositoryMock
                .Setup(r => r.GetByName(dto.Name))
                .ReturnsAsync((CustomerEntity)null);

            _mapperMock
                .Setup(m => m.Map<CustomerEntity>(dto))
                .Returns(new CustomerEntity());

            var createdEntity = new CustomerEntity();

            _customerRepositoryMock
                .Setup(r => r.Create(It.IsAny<CustomerEntity>()))
                .ReturnsAsync(createdEntity);

            _mapperMock
                .Setup(m => m.Map<GetCustomerDto>(createdEntity))
                .Returns(new GetCustomerDto());

            var result = await _customerService.Create(dto);

            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal(ErrorMessages.Created("Customer"), result.Message);
        }

        [Fact]
        public async Task Delete_ShouldReturn404_WhenCustomerDoesNotExist()
        {
            int id = 1;

            _customerRepositoryMock
                .Setup(r => r.GetById(id))
                .ReturnsAsync((CustomerEntity)null);

            var result = await _customerService.Delete(id);

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal(ErrorMessages.NotFound("Customer", id), result.Message);
        }

        [Fact]
        public async Task Delete_ShouldReturn404_WhenDeletePostsFails()
        {
            int id = 1;

            _customerRepositoryMock
                .Setup(r => r.GetById(id))
                .ReturnsAsync(new CustomerEntity());

            _postServiceMock
                .Setup(s => s.DeleteAll(id))
                .ReturnsAsync(new GenericResponse<bool> { Success = false });

            var result = await _customerService.Delete(id);

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal(
                ErrorMessages.DeleteFailedByParent("Posts", "Customer", id),
                result.Message);
        }

        [Fact]
        public async Task Delete_ShouldReturn204_WhenCustomerDeleted()
        {
            int id = 1;

            _customerRepositoryMock
                .Setup(r => r.GetById(id))
                .ReturnsAsync(new CustomerEntity());

            _postServiceMock
                .Setup(s => s.DeleteAll(id))
                .ReturnsAsync(new GenericResponse<bool> { Success = true });

            _customerRepositoryMock
                .Setup(r => r.Delete(id))
                .ReturnsAsync(true);

            var result = await _customerService.Delete(id);

            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.Equal(204, result.StatusCode);
            Assert.Equal(ErrorMessages.Deleted("Customer"), result.Message);
        }

        [Fact]
        public async Task GetById_ShouldReturn404_WhenNotExists()
        {
            int id = 1;

            _customerRepositoryMock
                .Setup(r => r.GetById(id))
                .ReturnsAsync((CustomerEntity)null);

            var result = await _customerService.GetById(id);

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal(ErrorMessages.NotFound("Customer", id), result.Message);
        }

        [Fact]
        public async Task GetById_ShouldReturn200_WhenExists()
        {
            int id = 1;
            var entity = new CustomerEntity();

            _customerRepositoryMock
                .Setup(r => r.GetById(id))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<GetCustomerDto>(entity))
                .Returns(new GetCustomerDto());

            var result = await _customerService.GetById(id);

            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task Update_ShouldReturn404_WhenCustomerNotFound()
        {
            var dto = new UpdateCustomerDto { CustomerId = 1 };

            _customerRepositoryMock
                .Setup(r => r.GetById(dto.CustomerId))
                .ReturnsAsync((CustomerEntity)null);

            var result = await _customerService.Update(dto);

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal(ErrorMessages.NotFound("Customer", dto.CustomerId), result.Message);
        }

        [Fact]
        public async Task Update_ShouldReturn200_WhenUpdated()
        {
            var dto = new UpdateCustomerDto { CustomerId = 1 };
            var entity = new CustomerEntity();

            _customerRepositoryMock
                .Setup(r => r.GetById(dto.CustomerId))
                .ReturnsAsync(entity);

            _customerRepositoryMock
                .Setup(r => r.Update(entity))
                .ReturnsAsync((entity, true));

            _mapperMock
                .Setup(m => m.Map<GetCustomerDto>(entity))
                .Returns(new GetCustomerDto());

            var result = await _customerService.Update(dto);

            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(ErrorMessages.Updated("Customer"), result.Message);
        }

        [Fact]
        public async Task Update_ShouldReturn404_WhenNoChanges()
        {
            var dto = new UpdateCustomerDto { CustomerId = 1 };
            var entity = new CustomerEntity();

            _customerRepositoryMock
                .Setup(r => r.GetById(dto.CustomerId))
                .ReturnsAsync(entity);

            _customerRepositoryMock
                .Setup(r => r.Update(entity))
                .ReturnsAsync((entity, false));

            var result = await _customerService.Update(dto);

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal(ErrorMessages.NoChanges("Customer"), result.Message);
        }
    }
}