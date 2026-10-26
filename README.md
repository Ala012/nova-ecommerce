# Nova — Boutique e-commerce

Application de boutique en ligne construite avec ASP.NET Core MVC, .NET 10, Entity Framework Core, Identity et MySQL.

## Prérequis
- SDK .NET 10
- MySQL 8

## Configuration et lancement
1. Configurez `ConnectionStrings:DefaultConnection` dans `appsettings.json` ou dans la configuration locale de votre environnement. Ne publiez jamais de mot de passe réel dans Git.
2. Depuis le dossier du projet : `dotnet restore`, puis `dotnet run` (ou ouvrez `EcommerceApp.slnx` dans Visual Studio).
3. Au premier démarrage, l’application crée les tables et ajoute les catégories et produits de démonstration.







## Fonctionnalités

- Catalogue de produits avec recherche, tri, pagination et filtres par catégorie
- Fiches détaillées et produits similaires
- Panier et commandes avec adresse, téléphone et mode de paiement
- Inscription, connexion et historique des commandes
- Espace d’administration protégé par rôle : gestion CRUD des produits et catégories, gestion des rôles des utilisateurs
- Téléversement d’images de produits avec vérification du format et de la taille
- Interface responsive et navigation par catégories

## Compte administrateur initial

Pour créer/promouvoir le compte administrateur initial, configurez des secrets locaux avant le démarrage (ne partagez pas ces valeurs) :

```powershell
dotnet user-secrets set "AdminUser:Email" "votre-adresse@example.com"
dotnet user-secrets set "AdminUser:Password" "choisissez-un-mot-de-passe-fort"
```

Configurez les deux secrets ensemble. Sans eux, aucun administrateur n’est créé automatiquement. Les utilisateurs qui s’inscrivent reçoivent le rôle `Customer`.

## Notes techniques

La base est initialisée avec `EnsureCreated()` pour simplifier le démarrage local. Pour une évolution de schéma en production, préférez des migrations EF Core et sauvegardez la base avant toute mise à jour.
