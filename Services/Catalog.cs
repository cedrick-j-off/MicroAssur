using MicroAssur.Models;
using Microsoft.Maui.Graphics;

namespace MicroAssur.Services;

/// <summary>Données de démonstration (aucun backend).</summary>
public static class Catalog
{
    static List<Plan> StdPlans(int a, int b, int c) => new()
    {
        new Plan("Essentiel", a, "Protection de base"),
        new Plan("Confort", b, "Protection renforcée", recommended: true),
        new Plan("Sérénité", c, "Protection maximale"),
    };

    static List<string> StdExclusions(string extra) => new()
    {
        "Sinistre survenu avant la date de début du contrat",
        "Faute intentionnelle de l’assuré",
        extra,
        "Délai de carence de 7 jours après la souscription",
    };

    static readonly List<string> StdIndemnisation = new()
    {
        "Déclaration du sinistre dans les 5 jours",
        "Analyse du dossier sous 48 h",
        "Versement sur votre compte Mobile Money",
    };

    public static readonly List<Category> All = new()
    {
        new Category
        {
            Id = "sante", Name = "Santé", Description = "Soins médicaux et hospitalisation",
            Icon = "ic_cat_sante.png", Tint = Color.FromArgb("#E3F0FF"),
            Intro = "Une couverture santé simple pour vous et votre famille.",
            Advantages = new() { "Remboursement des soins courants", "Prise en charge de l’hospitalisation", "Assistance médicale 24/7" },
            Guarantees = new()
            {
                new("ic_clipboard.png", "Consultations et soins", "Jusqu’à 300 000 FCFA"),
                new("ic_contracts.png", "Hospitalisation", "Jusqu’à 1 000 000 FCFA"),
                new("ic_shield.png", "Médicaments", "Jusqu’à 150 000 FCFA"),
                new("ic_shield.png", "Assistance 24/7", "Orientation et ambulance"),
            },
            Exclusions = StdExclusions("Soins esthétiques et de confort"),
            Indemnisation = StdIndemnisation,
            Plans = StdPlans(1000, 2000, 3000),
        },
        new Category
        {
            Id = "moto", Name = "Accident / Moto", Description = "Protection en cas d’accident",
            Icon = "ic_cat_moto.png", Tint = Color.FromArgb("#FFF1E3"), Hero = "logo.png",
            Intro = "Une protection en cas d’accident pour vous et votre véhicule.",
            Advantages = new() { "Prise en charge des frais médicaux", "Indemnisation en cas d’invalidité", "Assistance et dépannage" },
            Guarantees = new()
            {
                new("ic_clipboard.png", "Frais médicaux", "Jusqu’à 500 000 FCFA"),
                new("ic_contracts.png", "Invalidité permanente", "Jusqu’à 2 000 000 FCFA"),
                new("ic_shield.png", "Décès accidentel", "Jusqu’à 3 000 000 FCFA"),
                new("ic_shield.png", "Assistance 24/7", "Dépannage et remorquage"),
            },
            Exclusions = StdExclusions("Conduite sans permis valide ou sous l’emprise de l’alcool"),
            Indemnisation = StdIndemnisation,
            Plans = StdPlans(500, 1000, 1500),
        },
        new Category
        {
            Id = "incendie", Name = "Incendie & Vol", Description = "Marchandises et biens",
            Icon = "ic_cat_incendie.png", Tint = Color.FromArgb("#FFE4EA"),
            Intro = "Protégez votre commerce, vos marchandises et vos biens.",
            Advantages = new() { "Couverture incendie", "Couverture vol et cambriolage", "Indemnisation rapide" },
            Guarantees = new()
            {
                new("ic_shield.png", "Incendie", "Jusqu’à 1 500 000 FCFA"),
                new("ic_shield.png", "Vol et cambriolage", "Jusqu’à 1 000 000 FCFA"),
                new("ic_clipboard.png", "Dégâts des eaux", "Jusqu’à 500 000 FCFA"),
            },
            Exclusions = StdExclusions("Marchandises non déclarées"),
            Indemnisation = StdIndemnisation,
            Plans = StdPlans(1000, 2000, 3500),
        },
        new Category
        {
            Id = "vehicule", Name = "Moto / Véhicule", Description = "Dommages et assistance",
            Icon = "ic_cat_vehicule.png", Tint = Color.FromArgb("#E6EEFF"),
            Intro = "Couvrez les dommages de votre moto ou véhicule.",
            Advantages = new() { "Dommages matériels", "Responsabilité civile", "Dépannage 24/7" },
            Guarantees = new()
            {
                new("ic_clipboard.png", "Dommages matériels", "Jusqu’à 1 000 000 FCFA"),
                new("ic_shield.png", "Vol du véhicule", "Jusqu’à 2 000 000 FCFA"),
                new("ic_contracts.png", "Responsabilité civile", "Jusqu’à 3 000 000 FCFA"),
                new("ic_shield.png", "Assistance 24/7", "Dépannage et remorquage"),
            },
            Exclusions = StdExclusions("Usage professionnel non déclaré"),
            Indemnisation = StdIndemnisation,
            Plans = StdPlans(1500, 2500, 4000),
        },
        new Category
        {
            Id = "deces", Name = "Décès & Obsèques", Description = "Protection des proches",
            Icon = "ic_cat_deces.png", Tint = Color.FromArgb("#ECE9FF"),
            Intro = "Protégez vos proches et préparez l’avenir en toute sérénité.",
            Advantages = new() { "Capital versé aux bénéficiaires", "Prise en charge des obsèques", "Accompagnement des proches" },
            Guarantees = new()
            {
                new("ic_shield.png", "Capital décès", "Jusqu’à 1 000 000 FCFA"),
                new("ic_clipboard.png", "Frais d’obsèques", "Jusqu’à 500 000 FCFA"),
                new("ic_shield.png", "Assistance aux proches", "Accompagnement 24/7"),
            },
            Exclusions = StdExclusions("Décès lié à une pratique à risque non déclarée"),
            Indemnisation = StdIndemnisation,
            Plans = StdPlans(500, 1000, 2000),
        },
    };
}
