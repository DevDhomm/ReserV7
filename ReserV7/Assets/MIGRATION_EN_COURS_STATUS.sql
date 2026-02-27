-- ============================================
-- Migration: Ajouter le statut 'En cours' aux réservations
-- ============================================
-- NOTE: SQLite ne permet pas de modifier les contraintes CHECK existantes.
-- Vous devez soit:
-- 1. Supprimer le fichier app.db et redémarrer l'application (recommandé)
-- 2. Exécuter manuellement la migration ci-dessous

-- Pour mettre à jour la table existante (SQLite):
-- Cette opération nécessite de recréer la table car SQLite ne supporte pas ALTER TABLE DROP CONSTRAINT

-- Option 1: Supprimer et recréer manuellement (solution simple)
-- DELETE FROM app.db
-- Redémarrer l'application pour recréer la base de données avec le nouveau schéma

-- Option 2: Script de migration manuel (pour bases de données existantes)
-- PRAGMA foreign_keys=OFF;

-- BEGIN TRANSACTION;

-- Créer une table temporaire avec la nouvelle contrainte
-- CREATE TABLE Reservations_new (
--     id INTEGER PRIMARY KEY AUTOINCREMENT,
--     dateReservation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
--     motif TEXT,
--     statut TEXT NOT NULL DEFAULT 'Confirmée' CHECK(statut IN ('En attente', 'Confirmée', 'En cours', 'Annulée', 'Terminée')),
--     userId INTEGER NOT NULL,
--     salleId INTEGER NOT NULL,
--     creneauId INTEGER,
--     dateDebut TEXT NOT NULL,
--     dateFin TEXT NOT NULL,
--     heureDebut TEXT NOT NULL,
--     heureFin TEXT NOT NULL,
--     FOREIGN KEY (userId) REFERENCES Users(id) ON DELETE CASCADE,
--     FOREIGN KEY (salleId) REFERENCES Salles(id) ON DELETE CASCADE,
--     FOREIGN KEY (creneauId) REFERENCES Creneaux(id) ON DELETE CASCADE
-- );

-- Copier les données de l'ancienne table
-- INSERT INTO Reservations_new 
-- SELECT id, dateReservation, motif, statut, userId, salleId, creneauId, dateDebut, dateFin, heureDebut, heureFin 
-- FROM Reservations;

-- Supprimer l'ancienne table
-- DROP TABLE Reservations;

-- Renommer la nouvelle table
-- ALTER TABLE Reservations_new RENAME TO Reservations;

-- Recréer les index
-- CREATE INDEX idx_reservation_user ON Reservations(userId);
-- CREATE INDEX idx_reservation_salle ON Reservations(salleId);
-- CREATE INDEX idx_reservation_creneau ON Reservations(creneauId);
-- CREATE INDEX idx_reservation_statut ON Reservations(statut);

-- COMMIT;

-- PRAGMA foreign_keys=ON;
