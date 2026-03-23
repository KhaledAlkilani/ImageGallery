# Kysymykset — Osa 1: Lokaali kehitys

Vastaa kysymyksiin omin sanoin. Lyhyet, selkeät vastaukset riittävät — tarkoitus on osoittaa, että olet ymmärtänyt konseptit.

---

## Clean Architecture

**1.** Selitä omin sanoin: mitä tarkoittaa, että `UploadPhotoUseCase` "ei tiedä" tallennetaanko kuva paikalliselle levylle vai Azureen? Näytä koodirivit, jotka osoittavat tämän.

> Vastauksesi: UploadPhotoUseCase ei tiedä mihin kuva tallennetaan, koska se käyttää vain rajapinta. Koodissa se käyttää: private readonly IStorageService \_storageService; ja \_storageService.UploadAsync() Se ei käytä LocalStorageService tai AzureBlobStorageService suoraan. Eli se ei tiedä onko levy vai Azure, vain rajapinta käytössä

---

**2.** Miksi `IStorageService`-rajapinta on määritelty `GalleryApi.Domain`-kerroksessa, mutta `LocalStorageService` on `GalleryApi.Infrastructure`-kerroksessa? Mitä hyötyä tästä jaosta on?

> Vastauksesi: IStorageService on Domain kerroksessa koska se on sopimus. LocalStorage on Infrastructure kerroksessa koska se on toteus. Hyöty on: Voidaan vaihtaa tallennus helposti, koodi ei riippuu konkreettinen toteus, parempi testaus.

---

**3.** Testit käyttävät `Mock<IAlbumRepository>`. Mitä mock-objekti tarkoittaa, ja miksi Clean Architecture tekee tämän testaustavan mahdolliseksi?

> Vastauksesi: Mock objekti on fake objekti testsissä. Se ei ole oikea tietokanta, mutta toimii samalla tavalla. Clean Architecture auttaa koska: riippuvuudet on rajapinnat, voidaan antaa mock helposti.

---

## Salaisuuksien hallinta

**4.** Kovakoodattu API-avain on ongelma, vaikka repositorio olisi yksityinen. Selitä kaksi eri syytä miksi.

> Vastauksesi: 1- Avaimet voi vuotaa jos joku saa repo. 2- Avaimet jää git historia vaikka poistetaan.

---

**5.** Riittääkö se, että poistat kovakoodatun avaimen myöhemmässä commitissa? Perustele vastauksesi.

> Vastauksesi: Ei riitä, koska git historia säilyttää vanha commit. Eli avain on vielä siellä vaikka poistettu.

---

**6.** Minne User Secrets tallennetaan käyttöjärjestelmässä? (Mainitse sekä Windows- että Linux/macOS-polut.) Miksi tämä sijainti on turvallinen?

> Vastauksesi: Windows: %APPDATA%\Microsoft\UserSecrets\ | Linux/macOS: ~/.microsoft/usersecrets/ Se on turvallinen koska: Ei ole repo sisällä, ei mene GitHubiin.

---

## Options Pattern ja konfiguraatio

**7.** Mitä hyötyä on `IOptions<ModerationServiceOptions>`:n käyttämisestä verrattuna siihen, että luetaan arvo suoraan `IConfiguration`-rajapinnalta (`configuration["ModerationService:ApiKey"]`)?

> Vastauksesi: IOptions on parempi koska: Tyyppi on turvallinen, ei tarvitse kirjoittaa string avaimia, helpompi käyttää. Configuration[""] voi tulla virhe helposti.

---

**8.** ASP.NET Core lukee konfiguraation useista lähteistä prioriteettijärjestyksessä. Listaa lähteet korkeimmasta matalimpaan ja selitä, mikä arvo lopulta käytetään, kun sama avain on sekä `appsettings.json`:ssa että User Secretsissä.

> Vastauksesi: Järjestys korkea => matala: User Seecrets => appsettings.Development.json => appsettings.json. Jos sama avain on, niin User Secrets voittaa, eli sen arvo käytetään.

---

**9.** `DependencyInjection.cs`:ssä valitaan tallennustoteutus näin:

```csharp
var provider = configuration["Storage:Provider"] ?? "local";
if (provider == "azure")
    services.AddScoped<IStorageService, AzureBlobStorageService>();
else
    services.AddScoped<IStorageService, LocalStorageService>();
```

Miksi käytetään konfiguraatioarvoa `env.IsDevelopment()`-tarkistuksen sijaan? Mitä haittaa olisi `if (env.IsDevelopment()) { käytä lokaalia }`-lähestymistavassa?

> Vastauksesi: Konfiguraatio on parempi kuin env.IsDevelopment() koska: Voidaan valita Azure ja myös development, enemmän joustava. Jos käyttää env.IsDevelopment() niin ei voi helposti vaihtaa ilman koodi muutosta.

---

## Tiedostotallennus

**10.** Kun lataat kuvan, `imageUrl`-kentän arvo on `/uploads/abc123-..../photo.jpg`. Miten tähän URL:iin pääsee selaimella? Mihin koodiin tämä perustuu?

> Vastauksesi: Selaimelle menee: http://localhost:port/uploads/abc123/photo.jpg Tämä toimii koska app.UseStaticFiles(); ja tiedostot on wwwroot/uploads kansiossa.

---

**11.** Mitä tapahtuu jos yrität ladata tiedoston jonka MIME-tyyppi on `application/pdf`? Missä tiedostossa ja millä koodirivillä tämä käyttäytyminen on määritelty?

> Vastauksesi: Jos MIME on application/pdf, upload epäonnistu, se on määritelty validaatio koodissa: if (!AllowedContentTypes.Contains(request.ContentType)) return Result<PhotoDto>.Failure("Tiedostotyyppi ei ole sallittu."); ja sallitut tyypit ovat: private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"]; tiedostossa: UploadPhotoUseCase.

---

**12.** `DeletePhotoUseCase` poistaa tiedoston kutsumalla `_storageService.DeleteAsync(photo.FileName, photo.AlbumId)` — ei `photo.ImageUrl`:lla. Miksi?

> Vastauksesi: Koska käytetään FileName eikä ImageUrl koska ImageUrl on vain URL, ja FileName on oikea tiedoston nimi. Poisto tarvitsee oikea tiedosto järjestelmässä.
