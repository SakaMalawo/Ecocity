-- Script pour corriger le problème du champ RejectionReason
-- Rendre le champ RejectionReason nullable dans la table Initiatives

USE ecocity;

-- Modifier la colonne pour permettre les valeurs NULL
ALTER TABLE Initiatives MODIFY COLUMN RejectionReason VARCHAR(500) NULL;

-- Mettre à jour les enregistrements existants qui ont une chaîne vide
UPDATE Initiatives SET RejectionReason = NULL WHERE RejectionReason = '';

-- Afficher le résultat
SELECT 'Champ RejectionReason modifié avec succès' as Result;
