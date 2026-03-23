using GalleryApi.Application.DTOs;
using GalleryApi.Application.UseCases.Photos;
using GalleryApi.Domain.Entities;
using GalleryApi.Domain.Interfaces;
using Moq;
using Xunit;

namespace GalleryApi.Tests.UseCases;

/// <summary>
/// Testit UploadPhotoUseCase-luokalle.
///
/// Nämä testit tarkistavat että kuvan latauksen käyttötapaus toimii oikein
/// eri tilanteissa ilman oikeaa tietokantaa tai oikeaa tiedostotallennusta.
///
/// Käytetyt mockit:
///   - IAlbumRepository: tarkistaa löytyykö albumi
///   - IPhotoRepository: tallentaa Photo-entiteetin
///   - IStorageService: simuloi tiedoston tallennusta
/// </summary>
public class UploadPhotoUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_PalauttaaOnnistuneenTuloksen_KunLatausOnnistuu()
    {
        // Arrange
        var albumId = Guid.NewGuid();

        var album = new Album
        {
            Id = albumId,
            Name = "Testialbumi",
            Description = "Testikuvaus",
            CreatedAt = DateTime.UtcNow,
            Photos = []
        };

        var request = new UploadPhotoRequest(
            albumId,
            "Kesäkuva",
            new MemoryStream([1, 2, 3, 4]),
            "photo.jpg",
            "image/jpeg",
            1024
        );

        var mockAlbumRepository = new Mock<IAlbumRepository>();
        mockAlbumRepository
            .Setup(r => r.GetByIdAsync(albumId))
            .ReturnsAsync(album);

        var mockStorageService = new Mock<IStorageService>();
        mockStorageService
            .Setup(s => s.UploadAsync(
                request.FileStream,
                request.FileName,
                request.ContentType,
                request.AlbumId))
            .ReturnsAsync("/uploads/test-album/photo.jpg");

        var mockPhotoRepository = new Mock<IPhotoRepository>();
        mockPhotoRepository
            .Setup(r => r.CreateAsync(It.IsAny<Photo>()))
            .ReturnsAsync((Photo photo) => photo);

        var useCase = new UploadPhotoUseCase(
            mockPhotoRepository.Object,
            mockAlbumRepository.Object,
            mockStorageService.Object);

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.IsType<PhotoDto>(result.Value);

        Assert.Equal(albumId, result.Value.AlbumId);
        Assert.Equal("Kesäkuva", result.Value.Title);
        Assert.Equal("/uploads/test-album/photo.jpg", result.Value.ImageUrl);
        Assert.Equal("image/jpeg", result.Value.ContentType);
        Assert.Equal(1024, result.Value.FileSizeBytes);

        mockAlbumRepository.Verify(r => r.GetByIdAsync(albumId), Times.Once);
        mockStorageService.Verify(s => s.UploadAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            request.AlbumId), Times.Once);
        mockPhotoRepository.Verify(r => r.CreateAsync(It.IsAny<Photo>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PalauttaaVirheen_KunAlbumiaEiLoydy()
    {
        // Arrange
        var request = new UploadPhotoRequest(
            Guid.NewGuid(),
            "Puuttuva albumi",
            new MemoryStream([1, 2, 3]),
            "photo.jpg",
            "image/jpeg",
            1024
        );

        var mockAlbumRepository = new Mock<IAlbumRepository>();
        mockAlbumRepository
            .Setup(r => r.GetByIdAsync(request.AlbumId))
            .ReturnsAsync((Album?)null);

        var mockStorageService = new Mock<IStorageService>();
        var mockPhotoRepository = new Mock<IPhotoRepository>();

        var useCase = new UploadPhotoUseCase(
            mockPhotoRepository.Object,
            mockAlbumRepository.Object,
            mockStorageService.Object);

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("Albumia", result.Error);

        mockAlbumRepository.Verify(r => r.GetByIdAsync(request.AlbumId), Times.Once);
        mockStorageService.Verify(s => s.UploadAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>()), Times.Never);
        mockPhotoRepository.Verify(r => r.CreateAsync(It.IsAny<Photo>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_PalauttaaVirheen_KunTiedostoTyyppiOnVaarä()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        var album = new Album
        {
            Id = albumId,
            Name = "Testialbumi",
            Photos = []
        };

        var request = new UploadPhotoRequest(
            albumId,
            "PDF tiedosto",
            new MemoryStream([1, 2, 3]),
            "document.pdf",
            "application/pdf",
            1024
        );

        var mockAlbumRepository = new Mock<IAlbumRepository>();
        mockAlbumRepository
            .Setup(r => r.GetByIdAsync(albumId))
            .ReturnsAsync(album);

        var mockStorageService = new Mock<IStorageService>();
        var mockPhotoRepository = new Mock<IPhotoRepository>();

        var useCase = new UploadPhotoUseCase(
            mockPhotoRepository.Object,
            mockAlbumRepository.Object,
            mockStorageService.Object);

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("Tiedostotyyppi", result.Error);

        mockStorageService.Verify(s => s.UploadAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>()), Times.Never);
        mockPhotoRepository.Verify(r => r.CreateAsync(It.IsAny<Photo>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_PalauttaaVirheen_KunTiedostoOnLiianSuuri()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        var album = new Album
        {
            Id = albumId,
            Name = "Testialbumi",
            Photos = []
        };

        var request = new UploadPhotoRequest(
            albumId,
            "Liian iso kuva",
            new MemoryStream([1, 2, 3]),
            "big-image.jpg",
            "image/jpeg",
            11 * 1024 * 1024
        );

        var mockAlbumRepository = new Mock<IAlbumRepository>();
        mockAlbumRepository
            .Setup(r => r.GetByIdAsync(albumId))
            .ReturnsAsync(album);

        var mockStorageService = new Mock<IStorageService>();
        var mockPhotoRepository = new Mock<IPhotoRepository>();

        var useCase = new UploadPhotoUseCase(
            mockPhotoRepository.Object,
            mockAlbumRepository.Object,
            mockStorageService.Object);

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("liian suuri", result.Error);

        mockStorageService.Verify(s => s.UploadAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>()), Times.Never);
        mockPhotoRepository.Verify(r => r.CreateAsync(It.IsAny<Photo>()), Times.Never);
    }
}