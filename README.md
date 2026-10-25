# MicroAssur — frontend .NET MAUI (C# + XAML)

Refonte complète des 14 écrans de la maquette (ATELIER 5) en **XAML**, avec navigation fonctionnelle et
données de démonstration (aucun backend). Cible fournie : **Android** (.NET 8).

## Lancer
1. Visual Studio 2022 (17.8+) avec la charge de travail **.NET MAUI**, ou `dotnet workload install maui`.
2. Ouvrir `MicroAssur.csproj`, choisir un émulateur/appareil Android, F5.
   En ligne de commande : `dotnet build -t:Run -f net8.0-android`

> iOS / Windows : ajouter les cibles dans `<TargetFrameworks>` du `.csproj` et copier le dossier
> `Platforms/<plateforme>` d'un projet créé avec `dotnet new maui`.

## Correspondance maquettes → pages (arborescence de navigation)
| N° | Écran | Page |
|----|-------|------|
| 01 | Accueil / bienvenue | `SplashPage` |
| 02 | Catalogue des couvertures | `CatalogPage` (onglet Souscription) |
| 03 | Catégorie (Accident / Moto) | `CategoryPage` |
| 04 | Détail couverture (Garanties / Exclusions / Indemnisation) | `GuaranteesPage` |
| 05 | Formule | `PlansPage` |
| 06 | Informations personnelles | `PersonalInfoPage` |
| 07 | Récapitulatif | `SummaryPage` |
| 08 | Paiement Mobile Money (MTN / Orange) | `PaymentPage` |
| 09-10 | Confirmation / Attestation | `CertificatePage` |
| 11 | Mes contrats | `ContractsPage` (onglet Contrats) |
| 12 | Échéancier (+ paiement) | `SchedulePage` → `PaymentPage` |
| 13 | Déclarer un sinistre (4 étapes) | `ClaimPage` |
| 14 | Suivi du dossier | `ClaimTrackingPage` |
| 15 | Indemnisation | `CompensationPage` |
| — | Onglets Accueil et Profil (présents dans l'arborescence, sans maquette) | `HomePage`, `ProfilePage` |

## Structure
- `Resources/Styles` : couleurs et styles (bouton dégradé, cartes, champs) tirés de la maquette.
- `Resources/Images` : logo, moto, illustrations (extraits du PDF) + icônes SVG.
- `Models/` et `Services/` : modèles, catalogue de démonstration, état de session en mémoire.
- L’onglet Profil permet d’enregistrer localement les informations personnelles et d’afficher les contrats enregistrés sur l’appareil.
- `Views/` : une page XAML + code-behind par écran.
- `AppShell.xaml` : barre d'onglets (Accueil, Contrats, Souscription, Profil) et routes des pages.

## À savoir
- Parcours testable de bout en bout : Commencer → catégorie → formules → infos → récap → paiement → attestation → contrats → échéancier / sinistre → suivi → indemnisation.
- Textes **Exclusions** et **Indemnisation** (écran 04), étapes 2-3 du sinistre, pages Accueil/Profil : contenu de remplacement, absent de la maquette.
- Le paiement est simulé sans débit réel ; après confirmation, le reçu de transaction est généré en fichier texte et peut être téléchargé via le partage natif. Le téléchargement PDF de l’attestation et l’envoi du sinistre restent simulés.
- Brancher plus tard une API en remplaçant `Services/Session.cs` et `Services/Catalog.cs`.
