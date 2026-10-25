# MicroAssur API (simulation)

API ASP.NET Core indépendante du frontend MAUI. Les opérateurs MTN Mobile Money et Orange Money sont **simulés** : aucun appel opérateur et aucun débit réel n’a lieu.

## Démarrer

Depuis la racine du dépôt, avec le SDK .NET 10 :

```powershell
dotnet run --project Backend\MicroAssur.Api
```

L’API écoute sur `http://localhost:5080`. La base SQLite est créée dans `Backend\MicroAssur.Api\Data\microassur.db`. La documentation OpenAPI est disponible en mode développement à `/openapi/v1.json`.

## Parcours

1. `GET /api/plans` retourne le catalogue et les montants de référence.
2. `POST /api/payments` crée une demande en attente. Chaque requête doit inclure un en-tête `Idempotency-Key`. Une répétition identique retourne le même paiement ; réutiliser la clé pour une autre demande donne `409 Conflict`.
3. En développement uniquement, `POST /api/simulator/payments/{id}/status` accepte `{"outcome":"Succeeded"}`, `{"outcome":"Failed"}` ou `{"outcome":"Pending"}`.
4. `GET /api/payments/{id}` ou `GET /api/payments` permet de suivre le résultat. Un premier paiement de souscription confirmé crée le contrat de manière atomique.
5. `GET /api/subscriptions` et `GET /api/subscriptions/{id}` consultent les contrats persistés.

Les montants des nouvelles souscriptions sont vérifiés par rapport au catalogue côté serveur. Les états finaux ne peuvent pas être modifiés. Les erreurs sont renvoyées en Problem Details.

Le parcours de paiement MAUI utilise cette API. L’émulateur Android contacte le poste hôte via `10.0.2.2`; le serveur écoute sur toutes les interfaces pour permettre ce trafic. Pour un téléphone physique, remplacer l’adresse du client dans `MauiProgram.cs` par l’adresse IP locale du poste et placer les deux appareils sur un réseau de confiance.

**Ne pas déployer ce simulateur comme un service de paiement réel.** Les routes de contrôle du simulateur sont volontairement disponibles uniquement en environnement `Development`; aucun mécanisme d’authentification ou d’intégration opérateur réel n’est inclus.
