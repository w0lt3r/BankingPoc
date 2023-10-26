using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingPoc.Data.Entities;
using BankingPoc.Data.Interfaces;
using BankingPoc.Data.Options;
using BankingPoc.Services;
using BankingPoc.Services.Interfaces;
using BankingPoc.Services.Models;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BankingPoc.Tests;

public class AccountServiceTest
{
    [Fact]
    public async Task CreateAccount_UserIdIsInvalid_ThrowsInvalidOperationException()
    {
        int invalidUserId = 3;
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var accountCreationRequest = new AccountCreateRequestDto
        {
            UserId = invalidUserId
        };
        var service = Setup(unitOfWorkMock);
        unitOfWorkMock.Setup(x =>
                x.GetRepository<User>().Get(invalidUserId))
            .ReturnsAsync((User)null);

        var createTask = service.CreateAccount(accountCreationRequest);
        
        Assert.ThrowsAsync<InvalidOperationException>(()=> createTask);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<User>().Get(invalidUserId),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Insert(It.IsAny<Account>()),
                Times.Never);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Never);
    }
    
    [Fact]
    public async Task CreateAccount_UserIdIsValid_CallsInsertAndSaveChanges()
    {
        int validUserId = 3;
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var accountCreationRequest = new AccountCreateRequestDto
        {
            UserId = validUserId
        };
        var existentUser = new User
        {
            Id = validUserId
        };
        var service = Setup(unitOfWorkMock);
        var createdAccount = new Account
        {
            Id = 6
        };
        unitOfWorkMock.Setup(x =>
                x.GetRepository<User>().Get(validUserId))
            .ReturnsAsync(existentUser);
        unitOfWorkMock.Setup(x =>
                x.GetRepository<Account>().Insert(It.IsAny<Account>()))
            .ReturnsAsync(createdAccount);

        var newAccount = await service.CreateAccount(accountCreationRequest);
        
        unitOfWorkMock
            .Verify(x=> x.GetRepository<User>().Get(validUserId),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Insert(It.IsAny<Account>()),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Once);
        Assert.Equal(newAccount.AccountId, createdAccount.Id);
    }
    
    [Fact]
    public async Task DeleteAccount_AccountIdIsInvalid_ThrowsInvalidOperationException()
    {
        int invalidAccountId = 3;
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var service = Setup(unitOfWorkMock);

        var deleteTask = service.DeleteAccount(invalidAccountId);
        
        Assert.ThrowsAsync<InvalidOperationException>(()=> deleteTask);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Get(invalidAccountId),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Delete(It.IsAny<Account>()),
                Times.Never);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Never);
    }
    
    [Fact]
    public async Task DeleteAccount_AccountIdIsValid_CallsDeleteAndSaveChanges()
    {
        int validAccountId = 3;
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var existentAccount = new Account();
        
        var service = Setup(unitOfWorkMock);
        unitOfWorkMock.Setup(x =>
                x.GetRepository<Account>().Get(validAccountId))
            .ReturnsAsync(existentAccount);

        await service.DeleteAccount(validAccountId);
        
        unitOfWorkMock.Verify(x=> x.GetRepository<Account>().Get(validAccountId),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Delete(It.IsAny<Account>()),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Once);
    }
    
    [Fact]
    public async Task Deposit_AmountIsGreaterThanMaxDepositAmount_ThrowsArgumentOutOfRangeException()
    {
        decimal maxDepositAmount = 10000;
        var options = new AccountConfigOptions
        {
            MaxDepositAmount = maxDepositAmount
        };
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var service = Setup(unitOfWorkMock, options);
        decimal excesiveDepositAmount = 10001;
        var request = new AccountAmountUpdateRequestDto
        {
            Amount = excesiveDepositAmount
        };

        var updateTask =service.Deposit(request);
        
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(()=> updateTask);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Get(It.IsAny<int>()),
                Times.Never);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Update(It.IsAny<Account>()),
                Times.Never);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Never);
    }
    
    [Fact]
    public async Task Deposit_AccountIdIsInvalid_ThrowsInvalidOperationException()
    {
        var maxDepositAmount = 10000;
        var options = new AccountConfigOptions
        {
            MaxDepositAmount = maxDepositAmount
        };
        int invalidAccountId = 3;
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var service = Setup(unitOfWorkMock, options);
        unitOfWorkMock.Setup(x =>
                x.GetRepository<Account>().Get(invalidAccountId))
            .ReturnsAsync((Account)null);
        var request = new AccountAmountUpdateRequestDto
        {
            AccountId = invalidAccountId
        };

        var updateTask =service.Deposit(request);
        
        await Assert.ThrowsAsync<InvalidOperationException>(()=> updateTask);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Get(invalidAccountId),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Update(It.IsAny<Account>()),
                Times.Never);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Never);
    }
    
    [Fact]
    public async Task Deposit_AccountAndAmountAreValid_CallsUpdateAndSaveChanges()
    {
        var maxDepositAmount = 10000;
        var options = new AccountConfigOptions
        {
            MaxDepositAmount = maxDepositAmount
        };
        int validAccountId = 3;
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        decimal previousAmount = 120;
        var existentAccount = new Account
        {
            Amount = previousAmount
        };
        var service = Setup(unitOfWorkMock, options);
        unitOfWorkMock.Setup(x =>
                x.GetRepository<Account>().Get(validAccountId))
            .ReturnsAsync(existentAccount);
        var request = new AccountAmountUpdateRequestDto
        {
            AccountId = validAccountId,
            Amount = 350
        };

        var updatedAccount =await service.Deposit(request);
        
        unitOfWorkMock.Verify(x=> x.GetRepository<Account>().Get(validAccountId),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Update(It.IsAny<Account>()),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Once);
        Assert.Equal(updatedAccount.Amount,  previousAmount + request.Amount);
    }
    
    [Fact]
    public async Task Withdraw_AccountIdIsInvalid_ThrowsInvalidOperationException()
    {
        int invalidAccountId = 3;
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var service = Setup(unitOfWorkMock);
        unitOfWorkMock.Setup(x =>
                x.GetRepository<Account>().Get(invalidAccountId))
            .ReturnsAsync((Account)null);
        var request = new AccountAmountUpdateRequestDto
        {
            AccountId = invalidAccountId
        };

        var updateTask = service.Withdraw(request);
        
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(()=> updateTask);
        Assert.Equal(exception.Message, "The account does not exist");
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Get(invalidAccountId),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Update(It.IsAny<Account>()),
                Times.Never);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Never);
    }
    
    [Fact]
    public async Task Withdraw_AmountRepresentsMoreThanMaxWithdrawPercentage_ThrowsInvalidOperationException()
    {
        int validAccountId = 3;
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var maxWithdrawPercentage = 90;
        var options = new AccountConfigOptions
        {
            MaxWithdrawPercentage = maxWithdrawPercentage
        };
        var service = Setup(unitOfWorkMock, options);
        var existentAccount = new Account
        {
            Amount = 100
        };
        unitOfWorkMock.Setup(x =>
                x.GetRepository<Account>().Get(validAccountId))
            .ReturnsAsync(existentAccount);
        var request = new AccountAmountUpdateRequestDto
        {
            AccountId = validAccountId,
            Amount = 91
        };

        var updateTask = service.Withdraw(request);
        
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(()=> updateTask);
        Assert.StartsWith($"The amount cannot exceed the {options.MaxWithdrawPercentage}% of the balance", exception.Message);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Get(validAccountId),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Update(It.IsAny<Account>()),
                Times.Never);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Never);
    }
    
    [Fact]
    public async Task Withdraw_AmountLeavesLessThanMinAccountAmount_ThrowsInvalidOperationException()
    {
        int validAccountId = 3;
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var minAccountAmount = 80;
        var options = new AccountConfigOptions
        {
            MinAccountAmount = minAccountAmount,
            MaxWithdrawPercentage = 90
        };
        var service = Setup(unitOfWorkMock, options);
        var existentAccount = new Account
        {
            Amount = 100
        };
        unitOfWorkMock.Setup(x =>
                x.GetRepository<Account>().Get(validAccountId))
            .ReturnsAsync(existentAccount);
        var request = new AccountAmountUpdateRequestDto
        {
            AccountId = validAccountId,
            Amount = 23
        };

        var updateTask = service.Withdraw(request);
        
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(()=> updateTask);
        Assert.StartsWith($"The remaining balance cannot be less than {options.MinAccountAmount}", exception.Message);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Get(validAccountId),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Update(It.IsAny<Account>()),
                Times.Never);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Never);
    }
    
    [Fact]
    public async Task Withdraw_AmountIsValid_CallsUpdateAndSaveChanges()
    {
        int validAccountId = 3;
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var minAccountAmount = 80;
        var options = new AccountConfigOptions
        {
            MinAccountAmount = minAccountAmount,
            MaxWithdrawPercentage = 90
        };
        var service = Setup(unitOfWorkMock, options);
        decimal previousAmount = 100;
        var existentAccount = new Account
        {
            Amount = previousAmount
        };
        unitOfWorkMock.Setup(x =>
                x.GetRepository<Account>().Get(validAccountId))
            .ReturnsAsync(existentAccount);
        var request = new AccountAmountUpdateRequestDto
        {
            AccountId = validAccountId,
            Amount = 5
        };

        var updatedAccount = await service.Withdraw(request);
        
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Get(validAccountId),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.GetRepository<Account>().Update(It.IsAny<Account>()),
                Times.Once);
        unitOfWorkMock
            .Verify(x=> x.SaveChanges(), Times.Once);
        Assert.Equal(updatedAccount.Amount,  previousAmount - request.Amount);
    }

    private IAccountService Setup(IMock<IUnitOfWork> unitOfWorkMock = null,
        AccountConfigOptions options = null)
    {
        if (unitOfWorkMock is null)
        {
            unitOfWorkMock = new Mock<IUnitOfWork>();
        }
        var accountRepositoryMock = new Mock<IGenericRepository<Account>>();
        ((Mock<IUnitOfWork>)unitOfWorkMock).Setup(
                x=> x.GetRepository<Account>())
            .Returns(accountRepositoryMock.Object);
        
        IOptions<AccountConfigOptions> someOptions = 
            Options.Create(options is null? new AccountConfigOptions(): options);
        
        return new AccountService(
            unitOfWorkMock.Object,
            someOptions
            );
    }
}