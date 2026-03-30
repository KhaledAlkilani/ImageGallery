# Kysymykset — Osa 3: Key Vault ja Infrastructure as Code

Vastaa kysymyksiin omin sanoin. Lyhyet, selkeät vastaukset riittävät.

---

## Key Vault

**1.** Miksi `ModerationService:ApiKey` tallennettiin Key Vaultiin eikä Application Settingsiin? Mitä lisäarvoa Key Vault tuo Application Settingsiin verrattuna?

> Vastauksesi: Key Vault on turvallisempi paikka salaisuuksille. Application settings näkyy helpommin ja voi vuotaa. Key Vaultissa avain on erillään sovelluksesta ja käyttöä voi hallita rooleilla.

---

**2.** Key Vault -salaisuuden nimi on `ModerationService--ApiKey` (kaksi väliviivaa), mutta koodissa se luetaan `configuration["ModerationService:ApiKey"]` (kaksoispiste). Miksi käytetään `--`?

> Vastauksesi: Key Vault ei tue : merkkiä nimessä. Siksi käytetään --. ASP.NET muuttaa -- => : automaattisesti.

---

**3.** `Program.cs`:ssä Key Vault lisätään konfiguraatiolähteeksi `if (!string.IsNullOrEmpty(keyVaultUrl))`-ehdolla. Miksi tämä ehto on tärkeä? Mitä tapahtuisi ilman sitä?

> Vastauksesi: Ehto varmistaa, että Key Vault listätään vain jos URL on olemassa. Ilman ehtoa sovellus voi kaatua, jos Key Vaultia ei ole konfiguroitu.

---

**4.** Kun sovellus on käynnissä Azuressa, konfiguraation prioriteettijärjestys on: Key Vault → Application Settings → `appsettings.json`. Selitä millä arvolla `ModerationService:ApiKey` lopulta ladataan — ja käy läpi jokainen askel siitä, miten arvo päätyy sovelluksen `IOptions<ModerationServiceOptions>`:iin.

> Vastauksesi: Ensin haetaan arvo Key Vaultista. Jos löytyy, sitä käytetään. Jos ei, sitten Application Settings. Jos ei siellä, sitten appsettings.json. Lopuksi arvo menee IOptions<ModerationServiceOptions> käyttöön.

---

**5.** Mitä eroa on `Key Vault Secrets User` ja `Key Vault Secrets Officer` -roolien välillä? Miksi annettiin nimenomaan `Secrets User`?

> Vastauksesi: Secrets User voi lukea salaisuuksia. Secrets Officer voi luoda ja muokata salaisuuksia. Sovellus tarvitsee vain lukea, siksi käytetään Secrets User.

---

## Infrastructure as Code (Bicep)

**6.** Bicep-templatessa RBAC-roolimääritykset tehdään suoraan (`storageBlobRole`, `keyVaultSecretsRole`). Mitä etua tällä on verrattuna siihen, että ajat erilliset `az role assignment create` -komennot käsin?

> Vastauksesi: Kaikki tehdään automaattisesti yhdessä deployssa. Ei tarvitse ajaa komentoja käsin. Vähemmän virheitä ja sama tulos joka kerta.

---

**7.** Bicep-parametritiedostossa `main.bicepparam` on `param moderationApiKey = ''` — arvo jätetään tyhjäksi. Miksi? Miten oikea arvo annetaan?

> Vastauksesi: Arvo ei laiteta tiedostoon turvallisuuden vuoksi. Oikea arvo annetaan deploy komennossa --parameters.

---

**8.** Bicep-templatessa `webApp`-resurssin `identity`-lohkossa on `type: 'SystemAssigned'`. Mitä tämä tekee, ja mitä manuaalista komentoa se korvaa?

> Vastauksesi: Se luo automaattisesti Managed Identityn sovellukselle. Se korvaa manuaalisen komennon identityn lisäämiseksi.

---

**9.** RBAC-roolimäärityksen nimi generoidaan `guid()`-funktiolla:

```bicep
name: guid(storageAccount.id, webApp.identity.principalId, 'StorageBlobDataContributor')
```

Miksi nimi generoidaan näin eikä esimerkiksi kovakoodatulla merkkijonolla? Mitä tapahtuisi jos nimi olisi sama kaikissa deploymenteissa?

> Vastauksesi: guid() tekee uniikin nimen. Jos nimi olisi sama, deployment voi epäonnistua tai korvata vanhan. Uniikki nimi estää konfliktit.

---

**10.** Olet nyt rakentanut saman infrastruktuurin kahdella tavalla: manuaalisesti (Osat 2 & 3) ja Bicepillä (Osa 3). Kuvaile konkreettisesti yksi tilanne, jossa IaC-lähestymistapa on selvästi manuaalista parempi. Kuvaile myös tilanne, jossa manuaalinen tapa riittää.

> Vastauksesi: IaC on parempi kun halutaan tehdä sama ympäristö mmonta kertaa nopeasti. Esimerkiksi testi ja tuotanto. Manuaalinen tapa riittää pienessä kokeilussa tai oppimisessa.
