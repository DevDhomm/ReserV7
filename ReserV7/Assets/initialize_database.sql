-- ============================================
-- SYSTÈME DE RÉSERVATION DE SALLES
-- Initialisation de la Base de Données
-- ============================================

-- Table des Utilisateurs
CREATE TABLE IF NOT EXISTS Users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT NOT NULL UNIQUE,
    email TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL,
    role TEXT NOT NULL CHECK(role IN ('User', 'Gestionnaire')),
    nom TEXT NOT NULL,
    dateCreation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Table des Salles
CREATE TABLE IF NOT EXISTS Salles (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nom TEXT NOT NULL UNIQUE,
    description TEXT,
    capacite INTEGER NOT NULL CHECK(capacite > 0),
    type TEXT NOT NULL,
    etage INTEGER NOT NULL,
    disponibilite INTEGER NOT NULL DEFAULT 1 CHECK(disponibilite IN (0, 1)),
    dateCreation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Table des Équipements
CREATE TABLE IF NOT EXISTS Equipements (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nom TEXT NOT NULL,
    description TEXT,
    type TEXT NOT NULL,
    estFonctionnel INTEGER NOT NULL DEFAULT 1 CHECK(estFonctionnel IN (0, 1)),
    salleId INTEGER NOT NULL,
    dateCreation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (salleId) REFERENCES Salles(id) ON DELETE CASCADE
);

-- Table des Créneaux Horaires
CREATE TABLE IF NOT EXISTS Creneaux (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    debut TEXT NOT NULL,
    fin TEXT NOT NULL,
    dateCreation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Table des Réservations
CREATE TABLE IF NOT EXISTS Reservations (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    dateReservation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    motif TEXT,
    statut TEXT NOT NULL DEFAULT 'Confirmée' CHECK(statut IN ('En attente', 'Confirmée', 'Annulée', 'Terminée')),
    userId INTEGER NOT NULL,
    salleId INTEGER NOT NULL,
    creneauId INTEGER,
    dateDebut TEXT NOT NULL,
    dateFin TEXT NOT NULL,
    heureDebut TEXT NOT NULL,
    heureFin TEXT NOT NULL,
    FOREIGN KEY (userId) REFERENCES Users(id) ON DELETE CASCADE,
    FOREIGN KEY (salleId) REFERENCES Salles(id) ON DELETE CASCADE,
    FOREIGN KEY (creneauId) REFERENCES Creneaux(id) ON DELETE CASCADE
);

-- Table de l'Historique
CREATE TABLE IF NOT EXISTS Historique (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    action TEXT NOT NULL,
    dateAction TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    reservationId INTEGER NOT NULL,
    FOREIGN KEY (reservationId) REFERENCES Reservations(id) ON DELETE CASCADE
);

-- ============================================
-- VUES
-- ============================================

-- Vue: Réservations Complètes
CREATE VIEW IF NOT EXISTS v_reservations_completes AS
SELECT 
    r.id,
    r.dateReservation,
    r.motif,
    r.statut,
    u.id AS user_id,
    u.nom AS user_nom,
    u.email AS user_email,
    u.role AS user_role,
    s.id AS salle_id,
    s.nom AS salle_nom,
    s.capacite,
    s.type AS salle_type,
    s.etage,
    s.disponibilite,
    r.creneauId,
    r.dateDebut,
    r.dateFin,
    r.heureDebut,
    r.heureFin,
    COALESCE(c.debut, datetime(r.dateDebut || ' ' || r.heureDebut)) AS creneau_debut,
    COALESCE(c.fin, datetime(r.dateFin || ' ' || r.heureFin)) AS creneau_fin,
    CAST((JULIANDAY(COALESCE(c.fin, datetime(r.dateFin || ' ' || r.heureFin))) - JULIANDAY(COALESCE(c.debut, datetime(r.dateDebut || ' ' || r.heureDebut)))) * 24 AS INTEGER) AS duree_heures
FROM Reservations r
JOIN Users u ON r.userId = u.id
JOIN Salles s ON r.salleId = s.id
LEFT JOIN Creneaux c ON r.creneauId = c.id;

-- Vue: Salles avec Équipements
CREATE VIEW IF NOT EXISTS v_salles_equipements AS
SELECT 
    s.id,
    s.nom,
    s.capacite,
    s.type,
    s.etage,
    s.disponibilite,
    COUNT(DISTINCT CASE WHEN e.estFonctionnel = 1 THEN e.id END) AS nb_equipements_fonctionnels,
    COUNT(DISTINCT e.id) AS nb_equipements_total,
    GROUP_CONCAT(DISTINCT CASE WHEN e.estFonctionnel = 1 THEN e.nom END, ', ') AS equipements
FROM Salles s
LEFT JOIN Equipements e ON s.id = e.salleId
GROUP BY s.id;

-- Vue: Statistiques Utilisateurs
CREATE VIEW IF NOT EXISTS v_statistiques_utilisateurs AS
SELECT 
    u.id,
    u.nom,
    u.email,
    u.role,
    COUNT(r.id) AS total_reservations,
    SUM(CASE WHEN r.statut = 'Confirmée' THEN 1 ELSE 0 END) AS confirmees,
    SUM(CASE WHEN r.statut = 'En attente' THEN 1 ELSE 0 END) AS en_attente,
    SUM(CASE WHEN r.statut = 'Annulée' THEN 1 ELSE 0 END) AS annulees,
    COALESCE(SUM(CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) * 24 AS INTEGER)), 0) AS heures_totales
FROM Users u
LEFT JOIN Reservations r ON u.id = r.userId
LEFT JOIN Creneaux c ON r.creneauId = c.id
GROUP BY u.id;

-- ============================================
-- INDEX
-- ============================================

CREATE INDEX IF NOT EXISTS idx_reservation_user ON Reservations(userId);
CREATE INDEX IF NOT EXISTS idx_reservation_salle ON Reservations(salleId);
CREATE INDEX IF NOT EXISTS idx_reservation_creneau ON Reservations(creneauId);
CREATE INDEX IF NOT EXISTS idx_reservation_statut ON Reservations(statut);
CREATE INDEX IF NOT EXISTS idx_equipement_salle ON Equipements(salleId);
CREATE INDEX IF NOT EXISTS idx_historique_reservation ON Historique(reservationId);
CREATE INDEX IF NOT EXISTS idx_creneau_debut ON Creneaux(debut);
CREATE INDEX IF NOT EXISTS idx_creneau_fin ON Creneaux(fin);

-- ============================================
-- DONNÉES INITIALES
-- ============================================

-- Utilisateurs (doit être inséré en premier pour les clés étrangères)
INSERT OR IGNORE INTO Users (id, username, email, password, role, nom, dateCreation)
VALUES 
    (1, 'user1', 'user1@ecole.fr', 'user1', 'User', 'Utilisateur 1', CURRENT_TIMESTAMP),
    (2, 'gestionnaire', 'gestionnaire@ecole.fr', 'gestionnaire', 'Gestionnaire', 'Gestionnaire', CURRENT_TIMESTAMP);

-- Salles
INSERT OR IGNORE INTO Salles (id, nom, description, capacite, type, etage, disponibilite, dateCreation)
VALUES 
    (1, 'Amphithéâtre A101', 'Grand amphithéâtre', 150, 'Amphithéâtre', 1, 1, CURRENT_TIMESTAMP),
    (2, 'Salle de Cours B201', 'Salle de cours standard', 35, 'Salle de cours', 2, 1, CURRENT_TIMESTAMP),
    (3, 'Salle de Cours B202', 'Salle de cours standard', 35, 'Salle de cours', 2, 1, CURRENT_TIMESTAMP),
    (4, 'Laboratoire C301', 'Laboratoire informatique', 25, 'Laboratoire', 3, 1, CURRENT_TIMESTAMP),
    (5, 'Laboratoire C302', 'Laboratoire scientifique', 25, 'Laboratoire', 3, 1, CURRENT_TIMESTAMP),
    (6, 'Salle de Séminaire D102', 'Salle de séminaire', 50, 'Salle de séminaire', 1, 1, CURRENT_TIMESTAMP),
    (7, 'Salle de Réunion E103', 'Salle de réunion', 15, 'Salle de réunion', 1, 1, CURRENT_TIMESTAMP),
    (8, 'Salle de Travail Collaboratif F104', 'Espace collaboration', 20, 'Salle de travail collaboratif', 1, 1, CURRENT_TIMESTAMP);

-- Créneaux horaires prédéfinis (journée standard)
INSERT OR IGNORE INTO Creneaux (id, debut, fin, dateCreation)
VALUES
    (1, '08:00:00', '10:00:00', CURRENT_TIMESTAMP),
    (2, '10:00:00', '12:00:00', CURRENT_TIMESTAMP),
    (3, '12:00:00', '14:00:00', CURRENT_TIMESTAMP),
    (4, '14:00:00', '16:00:00', CURRENT_TIMESTAMP),
    (5, '16:00:00', '18:00:00', CURRENT_TIMESTAMP),
    (6, '08:00:00', '10:00:00', CURRENT_TIMESTAMP),
    (7, '10:00:00', '12:00:00', CURRENT_TIMESTAMP),
    (8, '12:00:00', '14:00:00', CURRENT_TIMESTAMP),
    (9, '14:00:00', '16:00:00', CURRENT_TIMESTAMP),
    (10, '16:00:00', '18:00:00', CURRENT_TIMESTAMP),
    (11, '08:00:00', '10:00:00', CURRENT_TIMESTAMP),
    (12, '10:00:00', '12:00:00', CURRENT_TIMESTAMP),
    (13, '12:00:00', '14:00:00', CURRENT_TIMESTAMP),
    (14, '14:00:00', '16:00:00', CURRENT_TIMESTAMP),
    (15, '16:00:00', '18:00:00', CURRENT_TIMESTAMP);

-- Équipements
INSERT OR IGNORE INTO Equipements (id, nom, description, type, estFonctionnel, salleId, dateCreation)
VALUES
    (1, 'Vidéoprojecteur HD', 'Projecteur haute définition', 'Vidéoprojecteur', 1, 1, CURRENT_TIMESTAMP),
    (2, 'Tableau blanc interactif', 'Écran tactile 65 pouces', 'Tableau interactif', 1, 1, CURRENT_TIMESTAMP),
    (3, 'Caméra HD', 'Logitech 4K pour visioconférence', 'Visioconférence', 1, 2, CURRENT_TIMESTAMP),
    (4, 'Micro sans fil', 'Système de microphone sans fil', 'Audio', 1, 2, CURRENT_TIMESTAMP),
    (5, 'Écran tactile 55p', 'Écran tactile interactif', 'Affichage', 1, 3, CURRENT_TIMESTAMP),
    (6, 'Ordinateurs (10)', '10 postes de travail', 'Informatique', 1, 4, CURRENT_TIMESTAMP),
    (7, 'Microscopes (8)', '8 microscopes optiques', 'Équipement scientifique', 1, 4, CURRENT_TIMESTAMP),
    (8, 'Microscopes (8)', '8 microscopes optiques', 'Équipement scientifique', 1, 5, CURRENT_TIMESTAMP),
    (9, 'Vidéoprojecteur HD', 'Projecteur haute définition', 'Vidéoprojecteur', 1, 6, CURRENT_TIMESTAMP),
    (10, 'Tableau blanc', 'Tableau blanc traditionnel', 'Tableau', 1, 7, CURRENT_TIMESTAMP),
    (11, 'Caméra HD', 'Caméra pour enregistrement', 'Enregistrement', 1, 8, CURRENT_TIMESTAMP);

-- Réservations de démonstration
INSERT OR IGNORE INTO Reservations (id, dateReservation, motif, statut, userId, salleId, creneauId, dateDebut, dateFin, heureDebut, heureFin)
VALUES
    (1, CURRENT_TIMESTAMP, 'Cours de Programmation Avancée', 'Confirmée', 1, 1, 1, '2026-02-12', '2026-02-12', '08:00', '10:00'),
    (2, CURRENT_TIMESTAMP, 'TP Chimie Organique', 'Confirmée', 1, 4, 2, '2026-02-12', '2026-02-12', '10:00', '12:00'),
    (3, CURRENT_TIMESTAMP, 'Réunion d''équipe pédagogique', 'Confirmée', 1, 7, 3, '2026-02-12', '2026-02-12', '12:00', '14:00');

-- Historique des réservations
INSERT OR IGNORE INTO Historique (id, action, dateAction, reservationId)
VALUES
    (1, 'Réservation créée', CURRENT_TIMESTAMP, 1),
    (2, 'Réservation confirmée', CURRENT_TIMESTAMP, 1),
    (3, 'Réservation créée', CURRENT_TIMESTAMP, 2),
    (4, 'Réservation confirmée', CURRENT_TIMESTAMP, 2),
    (5, 'Réservation créée', CURRENT_TIMESTAMP, 3),
    (6, 'Réservation confirmée', CURRENT_TIMESTAMP, 3);
