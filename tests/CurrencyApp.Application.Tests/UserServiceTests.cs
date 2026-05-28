using CurrencyApp.Application.Common.Pagination;
using CurrencyApp.Application.Common.Results;
using CurrencyApp.Application.Contracts;
using CurrencyApp.Application.DTOs.UserCurrencies;
using CurrencyApp.Application.DTOs.Users;
using CurrencyApp.Application.Services;
using CurrencyApp.Domain.Entities;
using CurrencyApp.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Reflection;

namespace CurrencyApp.Application.Tests;

public class UserServiceTests
{
    #region Setup

    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrencyRepository> _currencyRepositoryMock;
    private readonly Mock<IExchangeRateService> _exchangeRateServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly UserService _service;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _currencyRepositoryMock = new Mock<ICurrencyRepository>();
        _exchangeRateServiceMock = new Mock<IExchangeRateService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new UserService(
            _userRepositoryMock.Object,
            _currencyRepositoryMock.Object,
            _exchangeRateServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #endregion

    #region Get

    [Fact]
    public async Task GetByIdAsync_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((User?)null);

        var result = await _service.GetByIdAsync(1);

        result.Type.Should().Be(ResultType.NotFound);
        result.Error.Should().Be("User not found.");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_User_When_User_Exists()
    {
        var user = new User("Hendrik", "hendrik@test.com");
        SetEntityId(user, 1);
        var currency = CreateCurrency(10, "USD", "US Dollar");
        user.AddOrUpdateCurrency(currency, 250);
        user.SetMainCurrency(currency);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        _exchangeRateServiceMock
            .Setup(x => x.GetRateAsync("USD", "USD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _service.GetByIdAsync(1);

        result.Type.Should().Be(ResultType.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Name.Should().Be("Hendrik");
        result.Data.Email.Should().Be("hendrik@test.com");
        result.Data.MainCurrencyId.Should().Be(10);
        result.Data.MainCurrencyCode.Should().Be("USD");
        result.Data.Holdings.Should().HaveCount(1);
        result.Data.Holdings[0].CurrencyId.Should().Be(10);
        result.Data.Holdings[0].CurrencyCode.Should().Be("USD");
        result.Data.Holdings[0].Amount.Should().Be(250);
        result.Data.TotalInMainCurrency.Should().Be(250);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_First_Page_When_Cursor_Is_Null()
    {
        var user = new User("Hendrik", "hendrik@test.com");
        SetEntityId(user, 1);

        _userRepositoryMock
            .Setup(x => x.GetPageAsync(null, 11))
            .ReturnsAsync(new List<User> { user });

        var result = await _service.GetAllAsync(null);

        result.Type.Should().Be(ResultType.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items[0].Id.Should().Be(1);
        result.Data.Items[0].Name.Should().Be("Hendrik");
        result.Data.Items[0].Email.Should().Be("hendrik@test.com");
        result.Data.Items[0].MainCurrencyId.Should().BeNull();
        result.Data.Items[0].MainCurrencyCode.Should().BeNull();
        result.Data.Items[0].Holdings.Should().BeEmpty();
        result.Data.NextCursor.Should().BeNull();
        _userRepositoryMock.Verify(x => x.GetPageAsync(null, 11), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_NextCursor_When_More_Than_One_Page_Exists()
    {
        var users = Enumerable.Range(1, 11)
            .Select(i =>
            {
                var u = new User($"Hendrik{i}", $"hendrik{i}@test.com");
                SetEntityId(u, i);
                return u;
            })
            .ToList();

        _userRepositoryMock
            .Setup(x => x.GetPageAsync(null, 11))
            .ReturnsAsync(users);

        var result = await _service.GetAllAsync(null);

        result.Type.Should().Be(ResultType.Success);
        result.Data!.Items.Should().HaveCount(10);
        result.Data.NextCursor.Should().Be(CursorTokenCodec.Encode(10));
    }

    [Fact]
    public async Task GetAllAsync_Should_Pass_Decoded_Cursor_To_Repository()
    {
        var cursor = CursorTokenCodec.Encode(1);

        _userRepositoryMock
            .Setup(x => x.GetPageAsync(1, 11))
            .ReturnsAsync(Array.Empty<User>());

        var result = await _service.GetAllAsync(cursor);

        result.Type.Should().Be(ResultType.Success);
        result.Data!.Items.Should().BeEmpty();
        _userRepositoryMock.Verify(x => x.GetPageAsync(1, 11), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Invalid_When_Cursor_Token_Is_Invalid()
    {
        var result = await _service.GetAllAsync("not-a-valid-cursor-token");

        result.Type.Should().Be(ResultType.Invalid);
        result.Error.Should().Be("Invalid cursor token.");

        _userRepositoryMock.Verify(x => x.GetPageAsync(It.IsAny<int?>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Write — Create

    [Fact]
    public async Task CreateAsync_Should_Return_Conflict_When_Email_Already_Exists()
    {
        var existing = new User("Hendrik", "hendrik@test.com");
        SetEntityId(existing, 1);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("hendrik@test.com"))
            .ReturnsAsync(existing);

        var request = new CreateUserRequest
        {
            Name = "Other",
            Email = "hendrik@test.com"
        };

        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.Type.Should().Be(ResultType.Conflict);
        result.Error.Should().Be("A user with this email already exists.");
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_Should_Return_Created_And_Commit_When_Email_Is_New()
    {
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("hendrik@test.com"))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => SetEntityId(u, 1))
            .Returns(Task.CompletedTask);

        var request = new CreateUserRequest
        {
            Name = "Hendrik",
            Email = "hendrik@test.com"
        };

        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.Type.Should().Be(ResultType.Created);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Name.Should().Be("Hendrik");
        result.Data.Email.Should().Be("hendrik@test.com");
        result.Data.MainCurrencyId.Should().BeNull();
        result.Data.Holdings.Should().BeEmpty();
        _userRepositoryMock.Verify(x => x.AddAsync(It.Is<User>(u => u.Name == "Hendrik" && u.Email == "hendrik@test.com")), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_ArgumentException_When_Name_Is_Empty()
    {
        var request = new CreateUserRequest
        {
            Name = "   ",
            Email = "hendrik@test.com"
        };

        var act = () => _service.CreateAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Name cannot be empty.");
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_ArgumentException_When_Email_Is_Invalid()
    {
        var request = new CreateUserRequest
        {
            Name = "Hendrik",
            Email = "not-a-valid-email"
        };

        var act = () => _service.CreateAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid email: not-a-valid-email");
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Write — Update

    [Fact]
    public async Task UpdateAsync_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((User?)null);

        var request = new UpdateUserRequest
        {
            Name = "New Name"
        };

        var result = await _service.UpdateAsync(1, request, CancellationToken.None);

        result.Type.Should().Be(ResultType.NotFound);
        result.Error.Should().Be("User not found.");
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_Invalid_When_Clear_And_Set_MainCurrency()
    {
        var user = new User("Hendrik", "hendrik@test.com");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        var request = new UpdateUserRequest
        {
            ClearMainCurrency = true,
            MainCurrencyId = 10
        };

        var result = await _service.UpdateAsync(1, request, CancellationToken.None);

        result.Type.Should().Be(ResultType.Invalid);
        result.Error.Should().Be("Cannot clear and set MainCurrencyId in the same request.");
        _currencyRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);

    }

    [Fact]
    public async Task UpdateAsync_Should_Return_NotFound_When_Currency_Does_Not_Exist()
    {
        var user = new User("Hendrik", "hendrik@test.com");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        _currencyRepositoryMock
            .Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync((Currency?)null);

        var request = new UpdateUserRequest
        {
            MainCurrencyId = 10
        };

        var result = await _service.UpdateAsync(1, request, CancellationToken.None);

        result.Type.Should().Be(ResultType.NotFound);
        result.Error.Should().Be("Currency not found.");
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Name()
    {
        var user = new User("Hendrik", "hendrik@test.com");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        var request = new UpdateUserRequest
        {
            Name = "Updated Name"
        };

        var result = await _service.UpdateAsync(1, request, CancellationToken.None);

        result.Type.Should().Be(ResultType.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Updated Name");
        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Clear_MainCurrency()
    {
        var currency = CreateCurrency(10, "USD", "US Dollar");
        var user = new User("Hendrik", "hendrik@test.com");
        user.AddOrUpdateCurrency(currency, 100);
        user.SetMainCurrency(currency);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        var request = new UpdateUserRequest
        {
            ClearMainCurrency = true
        };

        var result = await _service.UpdateAsync(1, request, CancellationToken.None);

        result.Type.Should().Be(ResultType.Success);
        result.Data.Should().NotBeNull();
        result.Data!.MainCurrencyId.Should().BeNull();
        result.Data.MainCurrencyCode.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_Should_Set_MainCurrency()
    {
        var currency = CreateCurrency(10, "USD", "US Dollar");
        var user = new User("Hendrik", "hendrik@test.com");
        user.AddOrUpdateCurrency(currency, 100);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        _currencyRepositoryMock
            .Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(currency);

        var request = new UpdateUserRequest
        {
            MainCurrencyId = 10
        };

        var result = await _service.UpdateAsync(1, request, CancellationToken.None);

        result.Type.Should().Be(ResultType.Success);
        result.Data.Should().NotBeNull();
        result.Data!.MainCurrencyId.Should().Be(10);
        result.Data.MainCurrencyCode.Should().Be("USD");
    }

    [Fact]
    public async Task UpdateAsync_Should_Commit_Changes()
    {
        var user = new User("Hendrik", "hendrik@test.com");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        var request = new UpdateUserRequest
        {
            Name = "Updated Name"
        };

        var result = await _service.UpdateAsync(1, request, CancellationToken.None);

        result.Type.Should().Be(ResultType.Success);
        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Succeed_When_Request_Is_Empty()
    {
        var user = new User("Hendrik", "hendrik@test.com");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        var request = new UpdateUserRequest();

        var result = await _service.UpdateAsync(1, request, CancellationToken.None);

        result.Type.Should().Be(ResultType.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Hendrik");
        result.Data.MainCurrencyId.Should().BeNull();
        _currencyRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_ArgumentException_When_Name_Is_Empty()
    {
        var user = new User("Hendrik", "hendrik@test.com");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        var request = new UpdateUserRequest
        {
            Name = "   "
        };

        var act = () => _service.UpdateAsync(1, request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Name cannot be empty.");
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Write — Holdings

    [Fact]
    public async Task AddOrUpdateCurrencyAsync_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((User?)null);

        var request = new CreateUserCurrencyRequest { Amount = 100 };

        var result = await _service.AddOrUpdateCurrencyAsync(1, 10, request, CancellationToken.None);

        result.Type.Should().Be(ResultType.NotFound);
        result.Error.Should().Be("User not found.");
        _currencyRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddOrUpdateCurrencyAsync_Should_Return_NotFound_When_Currency_Does_Not_Exist()
    {
        var user = new User("Hendrik", "hendrik@test.com");
        SetEntityId(user, 1);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        _currencyRepositoryMock
            .Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync((Currency?)null);

        var request = new CreateUserCurrencyRequest { Amount = 100 };

        var result = await _service.AddOrUpdateCurrencyAsync(1, 10, request, CancellationToken.None);

        result.Type.Should().Be(ResultType.NotFound);
        result.Error.Should().Be("Currency not found.");
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddOrUpdateCurrencyAsync_Should_Add_Holding_And_Commit()
    {
        var user = new User("Hendrik", "hendrik@test.com");
        SetEntityId(user, 1);
        var currency = CreateCurrency(10, "USD", "US Dollar");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        _currencyRepositoryMock
            .Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(currency);

        var request = new CreateUserCurrencyRequest { Amount = 250 };

        var result = await _service.AddOrUpdateCurrencyAsync(1, 10, request, CancellationToken.None);

        result.Type.Should().Be(ResultType.Success);
        result.Data!.Holdings.Should().HaveCount(1);
        result.Data.Holdings[0].CurrencyId.Should().Be(10);
        result.Data.Holdings[0].CurrencyCode.Should().Be("USD");
        result.Data.Holdings[0].Amount.Should().Be(250);
        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddOrUpdateCurrencyAsync_Should_Update_Existing_Holding_Amount()
    {
        var currency = CreateCurrency(10, "USD", "US Dollar");
        var user = new User("Hendrik", "hendrik@test.com");
        SetEntityId(user, 1);
        user.AddOrUpdateCurrency(currency, 100);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        _currencyRepositoryMock
            .Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(currency);

        var request = new CreateUserCurrencyRequest { Amount = 500 };

        var result = await _service.AddOrUpdateCurrencyAsync(1, 10, request, CancellationToken.None);

        result.Type.Should().Be(ResultType.Success);
        result.Data!.Holdings.Should().HaveCount(1);
        result.Data.Holdings[0].Amount.Should().Be(500);
        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveCurrency_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((User?)null);

        var result = await _service.RemoveCurrency(1, 10, CancellationToken.None);

        result.Type.Should().Be(ResultType.NotFound);
        result.Error.Should().Be("User not found.");
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveCurrency_Should_Remove_Holding_And_Commit()
    {
        var currency = CreateCurrency(10, "USD", "US Dollar");
        var user = new User("Hendrik", "hendrik@test.com");
        SetEntityId(user, 1);
        user.AddOrUpdateCurrency(currency, 100);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        var result = await _service.RemoveCurrency(1, 10, CancellationToken.None);

        result.Type.Should().Be(ResultType.Success);
        result.Data!.Holdings.Should().BeEmpty();
        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveCurrency_Should_Clear_MainCurrency_When_Removing_That_Holding()
    {
        var currency = CreateCurrency(10, "USD", "US Dollar");
        var user = new User("Hendrik", "hendrik@test.com");
        SetEntityId(user, 1);
        user.AddOrUpdateCurrency(currency, 100);
        user.SetMainCurrency(currency);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        var result = await _service.RemoveCurrency(1, 10, CancellationToken.None);

        result.Type.Should().Be(ResultType.Success);
        result.Data!.MainCurrencyId.Should().BeNull();
        result.Data.MainCurrencyCode.Should().BeNull();
        result.Data.Holdings.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveCurrency_Should_Succeed_When_Currency_Not_In_Holdings()
    {
        var user = new User("Hendrik", "hendrik@test.com");
        SetEntityId(user, 1);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        var result = await _service.RemoveCurrency(1, 10, CancellationToken.None);

        result.Type.Should().Be(ResultType.Success);
        result.Data!.Holdings.Should().BeEmpty();
        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteAsync_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((User?)null);

        var result = await _service.DeleteAsync(1, CancellationToken.None);

        result.Type.Should().Be(ResultType.NotFound);
        result.Error.Should().Be("User not found.");
        _userRepositoryMock.Verify(x => x.Delete(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_User_And_Commit()
    {
        var user = new User("Hendrik", "hendrik@test.com");
        SetEntityId(user, 1);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(user);

        var result = await _service.DeleteAsync(1, CancellationToken.None);

        result.Type.Should().Be(ResultType.Success);
        result.Data.Should().Be(Unit.Value);
        _userRepositoryMock.Verify(x => x.Delete(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helpers

    private static Currency CreateCurrency(int id, string code, string name)
    {
        var currency = new Currency(code, name);
        SetEntityId(currency, id);
        return currency;
    }

    private static void SetEntityId<T>(T entity, int id)
    {
        var idProperty = typeof(T).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public);
        idProperty.Should().NotBeNull("entity should expose an Id property");
        idProperty!.SetValue(entity, id);
    }

    #endregion
}
