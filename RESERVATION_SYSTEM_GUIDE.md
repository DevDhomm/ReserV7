# 📋 Système de Réservation de Salles - Configuration Finale

## 🔐 Identifiants de Connexion

| Username | Password | Rôle | Accès |
|----------|----------|------|-------|
| `gestionnaire` | `gestionnaire` | Gestionnaire | ✅ Tous les menus : Réservations, Salles, Gestionnaire, Data |
| `user1` | `user1` | User | ✅ Mes Réservations seulement |
| `user2` | `user2` | User | ✅ Mes Réservations seulement |

---

## 👥 Rôles et Permissions

### 🔑 Gestionnaire (Administrateur)
**Menu visible :**
- ✅ Home
- ✅ Réservations (toutes les réservations)
- ✅ Salles (gestion complète)
- ✅ Gestionnaire (statistiques, gestion des statuts)
- ✅ Data (données)

**Actions :**
- 📝 Créer/Modifier/Supprimer des réservations
- 🏢 Créer/Modifier/Supprimer des salles
- ⚙️ Gérer les équipements
- 📊 Voir les statistiques
- 📈 Exporter les données

---

### 👤 User (Utilisateur Simple)
**Menu visible :**
- ✅ Home
- ✅ Mes Réservations

**Actions :**
- 📝 Créer une réservation
- 👁️ Voir ses propres réservations uniquement
- ❌ Impossible d'accéder aux salles
- ❌ Impossible d'accéder au gestionnaire
- ❌ Impossible de modifier les réservations des autres

---

## 🗄️ Données de Test Pré-chargées

### Salles
- **Salle A** : Réunion, 20 places, Étage 1 (Projecteur + Tableau blanc)
- **Salle B** : Formation, 30 places, Étage 2 (Climatisation)
- **Bureau 101** : Bureau, 1 place, Étage 1

### Créneaux Horaires
- 09:00 - 10:30
- 10:30 - 12:00
- 14:00 - 15:30
- 15:30 - 17:00

### Utilisateurs
- **Gestionnaire** : Accès complet au système
- **User1** : Accès simple (réservations perso)
- **User2** : Accès simple (réservations perso)

---

## 🔄 Flux de Réservation

1. **User** → Se connecte
2. **User** → Voit "Mes Réservations"
3. **User** → Crée une réservation
4. **Gestionnaire** → Voit toutes les réservations
5. **Gestionnaire** → Valide/Modifie le statut

---

## 📱 Statuts de Réservation

- ⏳ **En attente** : Réservation créée, en attente de validation
- ✅ **Confirmée** : Réservation validée
- ❌ **Annulée** : Réservation annulée
- 🏁 **Terminée** : Réservation complétée

---

## 🔒 Sécurité

- ✅ Authentification par username/password (SQLite)
- ✅ Contrôle d'accès basé sur les rôles (RBAC)
- ✅ Les Users ne voient que leurs réservations
- ✅ Seul le Gestionnaire peut gérer les salles
- ✅ Menu dynamique selon le rôle

---

## 📂 Structure Base de Données

```
Users (id, username, password, email, nom, role)
  ├─ Salles (id, nom, description, capacite, type, etage)
  │   └─ Equipements (id, nom, type, salle_id)
  │   └─ Reservations (id, date, motif, statut, user_id, salle_id)
  └─ Creneaux (id, debut, fin)
```

---

## 🚀 À Tester

1. **Login avec Gestionnaire** :
   - Username: `gestionnaire`
   - Password: `gestionnaire`
   - Doit voir tous les menus

2. **Login avec User** :
   - Username: `user1`
   - Password: `user1`
   - Doit voir seulement "Mes Réservations"

3. **Créer une réservation** (en tant qu'User):
   - Sélectionner une salle
   - Ajouter un motif
   - Créer la réservation

4. **Voir en tant que Gestionnaire** :
   - La réservation de l'User doit apparaître
   - Pouvoir modifier son statut

---

Bon test ! 🎉
