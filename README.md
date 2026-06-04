# Mormors Bageri – REST API

Ett backend-API för ett bageri, byggt i ASP.NET Core. API:et hanterar bageriets data och användare, med säker inloggning via token-baserad autentisering och e-postnotifieringar.

> Utvecklat 2025 som ett backend-projekt med fokus på ren arkitektur och säker utveckling.

## ✨ Funktioner
- REST API för bageriets resurser (CRUD-operationer)
- Användarhantering och inloggning med JWT-autentisering
- Säker lösenordshantering med BCrypt-hashning
- Roll- och behörighetsstyrning (authorization)
- E-postnotifieringar via MailKit
- Tydlig lagerindelad arkitektur: Controllers, Services, DTOs och datamodeller

## 🔐 Säkerhet
- **JWT Bearer-autentisering** för säkra, token-baserade anrop
- **BCrypt** för hashning av lösenord (aldrig klartext)
- **Authorization** för att skydda endpoints baserat på behörighet

## 🛠️ Tech stack
- **Språk & ramverk:** C# · ASP.NET Core Web API · .NET 8
- **Databas:** MySQL med Entity Framework Core 8 (Code First + Migrations)
- **Autentisering:** JWT Bearer, BCrypt.Net
- **E-post:** MailKit
- **Arkitektur:** Controllers · Services · DTOs · Enums (lagerindelad design)

## 🚀 Kom igång
Förutsätter .NET 8 SDK och en MySQL-databas.

(kodexempel finns i filen README_MormorsBageri.md)

## 🧠 Vad jag lärde mig
- Att designa ett REST API med en ren, lagerindelad arkitektur (Controllers/Services/DTOs)
- Säker autentisering med JWT och lösenordshashning med BCrypt
- Integration av e-postutskick med MailKit
- Datamodellering med Entity Framework Core och migrations

## 👤 Författare
Naveen Bangalore Lakshminarayan · Malmö · naveenbl1202@gmail.com
