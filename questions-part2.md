# Kysymykset — Osa 2: Azure-julkaisu

Vastaa kysymyksiin omin sanoin. Lyhyet, selkeät vastaukset riittävät.

---

## Azure Blob Storage

**1.** Mitä eroa on `LocalStorageService.UploadAsync`:n ja `AzureBlobStorageService.UploadAsync`:n palauttamilla URL-arvoilla? Miksi ne eroavat?

> Vastauksesi: LocalStorageService palauttaa URL joka on lokaali, esim: wwwroot/uploads/photo.jpg. AzureBlobStorageService palauttaa URL joka on Azure osoite: https://stgallerykhaled.blob.core.windows.net/avatars/d639bd14-5f09-40f7-9534-28aacdbf6398/avatar adapt.jpg. Ne eroaa koska toinen on paikallinen ja toinen on pilvessä.

---

**2.** `AzureBlobStorageService` luo `BlobServiceClient`:n käyttäen `DefaultAzureCredential()` eikä yhteysmerkkijonoa. Mitä etua tästä on? Mitä `DefaultAzureCredential` tekee eri ympäristöissä?

> Vastauksesi: DefaultAzureCredential on turvallisempi kuin connection string. Se toimii eri ympäristössä: - Lokaalisti käyttää Azure CLI kirjautuminen, 2- Azuressa käyttää Managed Identity. Ei tarvitse tallentaa salasanoja koodiin.

---

**3.** Blob Container luodaan `--public-access blob` -asetuksella. Mitä tämä tarkoittaa: mitä pystyy tekemään ilman tunnistautumista, ja mikä vaatii Managed Identityn?

> Vastauksesi: --public-access blob tarkoittaa: - Voi lukea kuvat URL kautta ilman login. - Ei voi muuttaa tai poistaa ilman tunnistautuminen. Kirjoitus ja poisto vaatii Managed Identity.

---

## Application Settings

**4.** Application Settings ylikirjoittavat `appsettings.json`:n arvot. Selitä tämä mekanismi: miten se toimii ja miksi se on hyödyllistä eri ympäristöjä varten?

> Vastauksesi: Application Settings ylikirjoittaa appsettings.json arvot. Azure käyttää nämä arvot ensin. Hyöty: - Eri ympäristöt voi käyttää eri arvot. - Ei tarvitse muuttaa koodi.

---

**5.** Application Settingsissa käytetään `Storage__Provider` (kaksi alaviivaa), mutta koodissa luetaan `configuration["Storage:Provider"]` (kaksoispiste). Miksi?

> Vastauksesi: Azure ei käytä kaksoispiste :, siksi käytetään Storage\_\_Provider, se muutetaan koodissa: Storage:Provider.

---

**6.** Mitkä konfiguraatioarvot soveltuvat Application Settingsiin, ja mitkä eivät? Anna esimerkki kummastakin tässä tehtävässä.

> Vastauksesi: Sopii Application Settings: - Storage:Provider. - Storage:AccountName. Ei sovi: - Kovakoodattu arvot koodissa. - Ei tarvitse tallentaa esim merkkijonoa.

---

## Managed Identity ja RBAC

**7.** Selitä omin sanoin: mitä tarkoittaa "System-assigned Managed Identity"? Mitä tapahtuu tälle identiteetille, jos App Service poistetaan?

> Vastauksesi: System-assigned Managed Identity on automaattinen identiteetti App Servicelle. Sitä käytetään kirjautumiseen Azure palveluihin. Jos app service poistetaan, identiteetti poistuu myös.

---

**8.** App Servicelle annettiin `Storage Blob Data Contributor` -rooli Storage Accountin tasolle — ei koko subscriptionin tasolle. Miksi tämä on parempi tapa? Mikä periaate tähän liittyy?

> Vastauksesi: Rooli annetaan vain Storage Account tasolle koska: Turvallisempi, ja ei anna liikaa oikeuksia. Tämä liittyy periaatteeseen: least privilege. Eli annetaan vain tarvittavat oikeudet.

---
