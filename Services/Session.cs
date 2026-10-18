using MicroAssur.Models;

namespace MicroAssur.Services;

public enum PayMode { Subscription, Installment }

/// <summary>État en mémoire de l’application (parcours de souscription, contrats, sinistre).</summary>
public static class Session
{
    public static Category? Category { get; set; }
    public static Plan? Plan { get; set; }
    public static UserInfo? User { get; set; }
    public static Claim? CurrentClaim { get; set; }
    public static PayMode PayMode { get; set; } = PayMode.Subscription;

    public static List<Contract> Contracts { get; } = new() { Contract.Demo() };

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
        return contract;
    }

    public static void PayNextInstallment()
    {
        var next = ActiveContract?.NextInstallment;
        if (next is not null) next.Paid = true;
    }
}
