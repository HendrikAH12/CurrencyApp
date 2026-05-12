using CurrencyApp.Application.Common.Pagination;
using CurrencyApp.Application.Common.Results;
using CurrencyApp.Application.DTOs.Currencies;
using CurrencyApp.Application.Services;
using CurrencyApp.Domain.Entities;
using CurrencyApp.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Reflection;

namespace CurrencyApp.Application.Tests;

public class CurrencyServiceTests
{
    #region Setup

    private readonly Mock<ICurrencyRepository> _currencyRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly CurrencyService _service;

    public CurrencyServiceTests()
    {
        _currencyRepositoryMock = new Mock<ICurrencyRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new CurrencyService(
            _currencyRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #endregion

    #region Get

    [Fact]
    public async Task GetByIdAsync_Should_Return_NotFound_When_Currency_Does_Not_Exist()
    {
        _currencyRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Currency?)null);

        var result = await _service.GetByIdAsync(1);

        result.Type.Should().Be(ResultType.NotFound);
        result.Error.Should().Be("Currency not found.");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Currency_When_Currency_Exists()
    {
        var currency = new Currency("USD", "US Dollar");
        SetEntityId(currency, 1);

        _currencyRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(currency);

        var result = await _service.GetByIdAsync(1);

        result.Type.Should().Be(ResultType.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Code.Should().Be("USD");
        result.Data.Name.Should().Be("US Dollar");
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_First_Page_When_Cursor_Is_Null()
    {
        var currency = new Currency("USD", "US Dollar");
        SetEntityId(currency, 1);

        _currencyRepositoryMock
            .Setup(x => x.GetPageAsync(null, 11))
            .ReturnsAsync(new List<Currency> { currency });

        var result = await _service.GetAllAsync(null);

        result.Type.Should().Be(ResultType.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items[0].Id.Should().Be(1);
        result.Data.Items[0].Code.Should().Be("USD");
        result.Data.Items[0].Name.Should().Be("US Dollar");
        result.Data.NextCursor.Should().BeNull();
        _currencyRepositoryMock.Verify(x => x.GetPageAsync(null, 11), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_NextCursor_When_More_Than_One_Page_Exists()
    {
        var currencies = Enumerable.Range(1, 11)
            .Select(i =>
            {
                var code = new string((char)('A' + i), 3);
                var c = new Currency(code, $"US Dollar");
                SetEntityId(c, i);
                return c;
            })
            .ToList();

        _currencyRepositoryMock
            .Setup(x => x.GetPageAsync(null, 11))
            .ReturnsAsync(currencies);

        var result = await _service.GetAllAsync(null);

        result.Type.Should().Be(ResultType.Success);
        result.Data!.Items.Should().HaveCount(10);
        result.Data.NextCursor.Should().Be(CursorTokenCodec.Encode(10));
    }

    [Fact]
    public async Task GetAllAsync_Should_Pass_Decoded_Cursor_To_Repository()
    {
        var cursor = CursorTokenCodec.Encode(1);

        _currencyRepositoryMock
            .Setup(x => x.GetPageAsync(1, 11))
            .ReturnsAsync(Array.Empty<Currency>());

        var result = await _service.GetAllAsync(cursor);

        result.Type.Should().Be(ResultType.Success);
        result.Data!.Items.Should().BeEmpty();
        _currencyRepositoryMock.Verify(x => x.GetPageAsync(1, 11), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Invalid_When_Cursor_Token_Is_Invalid()
    {
        var result = await _service.GetAllAsync("not-a-valid-cursor-token");

        result.Type.Should().Be(ResultType.Invalid);
        result.Error.Should().Be("Invalid cursor token.");

        _currencyRepositoryMock.Verify(x => x.GetPageAsync(It.IsAny<int?>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Write — Create

    [Fact]
    public async Task CreateAsync_Should_Return_Conflict_When_Code_Already_Exists()
    {
        var code = "USD";
        var existing = new Currency(code, "US Dollar");
        SetEntityId(existing, 1);

        _currencyRepositoryMock
            .Setup(x => x.GetByCodeAsync(code))
            .ReturnsAsync(existing);

        var request = new CreateCurrencyRequest
        {
            Code = code,
            Name = "Other"
        };

        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.Type.Should().Be(ResultType.Conflict);
        result.Error.Should().Be($"Currency code '{code}' already exists.");
        _currencyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Currency>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_Should_Return_Created_And_Commit_When_Code_Is_New()
    {
        _currencyRepositoryMock
            .Setup(x => x.GetByCodeAsync("USD"))
            .ReturnsAsync((Currency?)null);

        _currencyRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Currency>()))
            .Callback<Currency>(c => SetEntityId(c, 1))
            .Returns(Task.CompletedTask);

        var request = new CreateCurrencyRequest
        {
            Code = "USD",
            Name = "US Dollar"
        };

        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.Type.Should().Be(ResultType.Created);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Code.Should().Be("USD");
        result.Data.Name.Should().Be("US Dollar");
        _currencyRepositoryMock.Verify(x => x.AddAsync(It.Is<Currency>(c => c.Code == "USD" && c.Name == "US Dollar")), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_ArgumentException_When_Code_Is_Invalid()
    {
        var request = new CreateCurrencyRequest
        {
            Code = "not-a-valid-code",
            Name = "US Dollar"
        };

        var act = () => _service.CreateAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Code must be a 3-letter ISO currency code.");
        _currencyRepositoryMock.Verify(x => x.GetByCodeAsync(It.IsAny<string>()), Times.Never);
        _currencyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Currency>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_ArgumentException_When_Name_Is_Empty()
    {
        var request = new CreateCurrencyRequest
        {
            Code = "USD",
            Name = "   "
        };

        var act = () => _service.CreateAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Name cannot be empty.");
        _currencyRepositoryMock.Verify(x => x.GetByCodeAsync(It.IsAny<string>()), Times.Never);
        _currencyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Currency>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteAsync_Should_Return_NotFound_When_Currency_Does_Not_Exist()
    {
        _currencyRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Currency?)null);

        var result = await _service.DeleteAsync(1, CancellationToken.None);

        result.Type.Should().Be(ResultType.NotFound);
        result.Error.Should().Be("Currency not found.");
        _currencyRepositoryMock.Verify(x => x.Delete(It.IsAny<Currency>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Currency_And_Commit()
    {
        var currency = new Currency("USD", "US Dollar");
        SetEntityId(currency, 1);

        _currencyRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(currency);

        var result = await _service.DeleteAsync(1, CancellationToken.None);

        result.Type.Should().Be(ResultType.Success);
        result.Data.Should().Be(Unit.Value);
        _currencyRepositoryMock.Verify(x => x.Delete(currency), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helpers

    private static void SetEntityId<T>(T entity, int id)
    {
        var idProperty = typeof(T).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public);
        idProperty.Should().NotBeNull("entity should expose an Id property");
        idProperty!.SetValue(entity, id);
    }

    #endregion
}
