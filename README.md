# MicroAssur

**Application mobile d'assurance pour les travailleurs du secteur informel.**

## Frontend .NET MAUI (C# + XAML)

Refonte des écrans de la maquette (ATELIER 5) en XAML, avec navigation fonctionnelle et données de démonstration. Le frontend fonctionne sans connexion obligatoire à un backend.

**Plateforme cible : Android — .NET 8.**

## Lancer l'application

1. Installer Visual Studio 2022 (17.8 ou supérieur) avec la charge de travail .NET MAUI, ou installer MAUI avec `dotnet workload install maui`.
2. Ouvrir `MicroAssur.csproj`.
3. Choisir un émulateur ou un appareil Android, puis lancer l'application avec F5.

En ligne de commande :

```powershell
dotnet build -t:Run -f net8.0-android
```

Pour iOS ou Windows, ajouter les cibles correspondantes dans `TargetFrameworks` du fichier `.csproj` et configurer les plateformes nécessaires.

## Correspondance maquettes → pages

| N° | Écran | Page |
|---|---|---|
| 01 | Accueil / bienvenue | `SplashPage` |
| 02 | Catalogue des couvertures | `CatalogPage` |
| 03 | Catégorie (Accident / Moto) | `CategoryPage` |
| 04 | Détail couverture | `GuaranteesPage` |
| 05 | Formule | `PlansPage` |
| 06 | Informations personnelles | `PersonalInfoPage` |
| 07 | Récapitulatif | `SummaryPage` |
| 08 | Paiement Mobile Money (MTN / Orange) | `PaymentPage` |
| 09-10 | Confirmation / Attestation | `CertificatePage` |
| 11 | Mes contrats | `ContractsPage` |
| 12 | Échéancier et paiement | `SchedulePage` → `PaymentPage` |
| 13 | Déclarer un sinistre | `ClaimPage` |
| 14 | Suivi du dossier | `ClaimTrackingPage` |
| 15 | Indemnisation | `CompensationPage` |
| — | Accueil et Profil | `HomePage`, `ProfilePage` |

## Structure du projet

- `Resources/Styles` : couleurs et styles de l'interface.
- `Resources/Images` : logo, illustrations et icônes.
- `Models/` et `Services/` : modèles, catalogue de démonstration et état de session en mémoire.
- `Views/` : pages XAML et code associé.
- `AppShell.xaml` : navigation par onglets et routes des pages.

L'onglet Profil permet d'enregistrer localement les informations personnelles et d'afficher les contrats enregistrés sur l'appareil.

## Fonctionnalités et limites

- Parcours de démonstration : catégorie → formules → informations personnelles → récapitulatif → paiement → attestation → contrats → échéancier et sinistres.
- Le paiement est simulé : aucun débit réel n'est effectué.
- Un reçu de transaction est généré en fichier texte et peut être partagé.
- Le téléchargement PDF de l'attestation et l'envoi du sinistre restent simulés.
- Certains contenus, notamment les exclusions, l'indemnisation et certaines étapes du sinistre, sont des contenus de remplacement.

## Évolutions prévues

Le frontend pourra être connecté à une API en remplaçant ou en adaptant les services de démonstration, notamment `Services/Session.cs` et `Services/Catalog.cs`.
