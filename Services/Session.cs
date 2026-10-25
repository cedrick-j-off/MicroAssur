using MicroAssur.Models;
using System.Text.Json;

namespace MicroAssur.Services;

public enum PayMode { Subscription, Installment }

/// <summary>État en mémoire de l’application (parcours de souscription, contrats, sinistre).</summary>
public static class Session
{
    private const string UserPreferenceKey = "microassur.saved-user";
    private const string ContractsPreferenceKey = "microassur.saved-contracts";
    private static UserInfo? user;
    private static bool userLoaded;

    public static Category? Category { get; set; }
    public static Plan? Plan { get; set; }
    public static UserInfo? User
    {
        get
        {
            if (!userLoaded)
            {
                var savedUser = Preferences.Default.Get(UserPreferenceKey, string.Empty);
                user = string.IsNullOrWhiteSpace(savedUser)
                    ? null
                    : JsonSerializer.Deserialize<UserInfo>(savedUser);
                userLoaded = true;
            }

            return user;
        }
        set
        {
            user = value;
            userLoaded = true;
            if (value is null)
                Preferences.Default.Remove(UserPreferenceKey);
            else
                Preferences.Default.Set(UserPreferenceKey, JsonSerializer.Serialize(value));
        }
    }
    public static Claim? CurrentClaim { get; set; }
    public static PayMode PayMode { get; set; } = PayMode.Subscription;

    public static List<Contract> Contracts { get; } = LoadContracts();

    public static Category CurrentCategory => Category ?? Catalog.All[1];
    public static Plan CurrentPlan => Plan ?? CurrentCategory.Plans[1];
    public static Contract? ActiveContract => Contracts.LastOrDefault();

    public static int PayAmount => PayMode == PayMode.Subscription
        ? CurrentPlan.PriceFcfa
        : ActiveContract?.NextInstallment?.Amount ?? 0;

    public static Contract CreateContract()
    {
        var contract = Contract.Create(CurrentCategory, CurrentPlan);
        Contracts.Add(contract);
        SaveContracts();
        return contract;
    }

    public static void PayNextInstallment()
    {
        var next = ActiveContract?.NextInstallment;
        if (next is not null)
        {
            next.Paid = true;
            SaveContracts();
        }
    }

    private static List<Contract> LoadContracts()
    {
        var savedContracts = Preferences.Default.Get(ContractsPreferenceKey, string.Empty);
        if (string.IsNullOrWhiteSpace(savedContracts))
            return new List<Contract> { Contract.Demo() };

        return JsonSerializer.Deserialize<List<Contract>>(savedContracts)
            ?? throw new InvalidDataException("Les contrats enregistrés sur cet appareil sont illisibles.");
    }

    private static void SaveContracts() =>
        Preferences.Default.Set(ContractsPreferenceKey, JsonSerializer.Serialize(Contracts));
}
