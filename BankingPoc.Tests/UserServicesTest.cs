using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingPoc.Data.Entities;
using BankingPoc.Data.Interfaces;
using BankingPoc.Services;
using BankingPoc.Services.Interfaces;
using BankingPoc.Services.Models;
using Moq;
using Xunit;

namespace BankingPoc.Tests;

public class UserServicesTest
{
    [Fact]
    public async Task GetUser_UserIdIsInvalid_ReturnsNull()
    {
        int invalidUserId = 1;
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x =>
                x.GetRepository<User>().Get(
                    u => u.Id == invalidUserId,
                    null,
                    "Accounts"
                ))
            .ReturnsAsync((IEnumerable<User>)null);
        var service = Setup(unitOfWorkMock);

        var user= await service.GetUser(invalidUserId);
        
        Assert.Null(user);
    }
    
    [Fact]
    public async Task GetUser_UserIdIsValid_ReturnsSingleElement()
    {
        int validUserId = 1;
        User testUser = new User
        {
            Id = 1,
            Name = "AnyName",
            LastName = "AnyLastName",
        };
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x =>
                x.GetRepository<User>().Get(
                    u => u.Id == validUserId,
                    null,
                    "Accounts"
                ))
            .ReturnsAsync(new User[1]{testUser});
        var service = Setup(unitOfWorkMock);

        var user= await service.GetUser(validUserId);
        
        Assert.Equal(user.Id, testUser.Id);
        Assert.Equal(user.Name, testUser.Name);
        Assert.Equal(user.LastName, testUser.LastName);
    }
    
    [Fact]
    public async Task DeleteUser_UserIdIsValid_CallsDeleteAndSaveChanges()
    {
        int validUserId = 1;
        User testUser = new User
        {
            Id = 1,
            Name = "AnyName",
            LastName = "AnyLastName",
            Accounts = new List<Account>
            {
                new Account()
            }
        };
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x =>
                x.GetRepository<User>().Get(
                    u => u.Id == validUserId,
                    null,
                    "Accounts"
                ))
            .ReturnsAsync(new User[1]{testUser});
        var service = Setup(unitOfWorkMock);

        await service.DeleteUser(validUserId);

        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().DeleteRange(testUser.Accounts), Times.Once);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<User>().Delete(It.IsAny<User>()), Times.Once);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Once);
    }
    
    [Fact]
    public async Task DeleteUser_UserIdIsInvalid_ThrowsInvalidOperationException()
    {
        int invalidUserId = 1;
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x =>
                x.GetRepository<User>().Get(
                    u => u.Id == invalidUserId,
                    null,
                    "Accounts"
                ))
            .ReturnsAsync((IEnumerable<User>)null);
        var service = Setup(unitOfWorkMock);

        var deleteTask = service.DeleteUser(invalidUserId);

        await Assert.ThrowsAsync<InvalidOperationException>(()=> deleteTask);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().DeleteRange(It.IsAny<Account[]>()), Times.Never);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<User>().Delete(It.IsAny<User>()), Times.Never);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Never);
    }
    
    [Fact]
    public async Task UpsertUser_UserIdIsNullOrZero_CallsInsertAndSaveChanges()
    {
        var upsertUserInput = new UserUpsertRequestDto
        {
            UserId = null,
            Name = "AnyName",
            LastName = "AnyLastName",
        };
        var createdUser = new User
        {
            Id = 2,
            Name = upsertUserInput.Name,
            LastName = upsertUserInput.LastName,
        };
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x =>
                x.GetRepository<User>().Insert(It.IsAny<User>()))
            .ReturnsAsync(createdUser);
        var service = Setup(unitOfWorkMock);

        var newUser = await service.UpsertUser(upsertUserInput);

        unitOfWorkMock
            .Verify(x=> x.GetRepository<User>().Insert(It.IsAny<User>()),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Once);
        Assert.Equal(createdUser.Id, newUser.Id);
        Assert.Equal(createdUser.Name, newUser.Name);
        Assert.Equal(createdUser.LastName, newUser.LastName);
    }
    
    [Fact]
    public async Task UpsertUser_UserIdIsNotNull_CallsUpdateAndSaveChanges()
    {
        var upsertUserInput = new UserUpsertRequestDto
        {
            UserId = 3,
            Name = "AnyName",
            LastName = "AnyLastName",
        };
        var existentUser = new User
        {
            Id = 3,
            Name = "PreviousName",
            LastName = "PreviousLastName",
        };
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x =>
                x.GetRepository<User>().Get(upsertUserInput.UserId))
            .ReturnsAsync(existentUser);
        var service = Setup(unitOfWorkMock);

        var newUser = await service.UpsertUser(upsertUserInput);

        unitOfWorkMock
            .Verify(x=> x.GetRepository<User>().Get(upsertUserInput.UserId),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<User>().Update(It.IsAny<User>()),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Once);
        Assert.Equal(upsertUserInput.UserId, newUser.Id);
        Assert.Equal(upsertUserInput.Name, newUser.Name);
        Assert.Equal(upsertUserInput.LastName, newUser.LastName);
    }
    
    [Fact]
    public async Task UpsertUser_UserIdIsNotNullAndInvalid_CallsOnlyGetUser()
    {
        int invalidUserId = 4;
        var upsertUserInput = new UserUpsertRequestDto
        {
            UserId = invalidUserId,
            Name = "AnyName",
            LastName = "AnyLastName",
        };
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x =>
                x.GetRepository<User>().Get(upsertUserInput.UserId))
            .ReturnsAsync((User)null);
        var service = Setup(unitOfWorkMock);

        var updateTask = service.UpsertUser(upsertUserInput);

        await Assert.ThrowsAsync<InvalidOperationException>(()=> updateTask);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<User>().Get(upsertUserInput.UserId),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<User>().Update(It.IsAny<User>()),
                Times.Never);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Never);
    }

    private IUserService Setup(IMock<IUnitOfWork> unitOfWorkMock)
    {
        if (unitOfWorkMock is null)
        {
            unitOfWorkMock = new Mock<IUnitOfWork>();
        }

        var accountRepositoryMock = new Mock<IGenericRepository<Account>>();
        ((Mock<IUnitOfWork>)unitOfWorkMock).Setup(
                x=> x.GetRepository<Account>())
            .Returns(accountRepositoryMock.Object);
        return new UserService(
            unitOfWorkMock.Object
            );
    }
}