using SYT.Fiskaly.SignDE.Clients.ValueObjects;
using SYT.Fiskaly.SignDE.Transactions.Requests;
using SYT.Fiskaly.SignDE.Transactions.Schemas;
using Receipt = SYT.Fiskaly.SignDE.Transactions.Schemas.Receipt;

namespace SYT.Fiskaly.SignDE.Transactions.Extensions;

public static class ReceiptExtensions
{
    public static FinishTransactionRequest ToFiskalyRequest(
        this Aggregates.Receipt receipt,
        ClientId clientId)
    {
        ArgumentNullException.ThrowIfNull(receipt);

        List<VatRateAmount> amountsPerVatRate = receipt.Items
            .GroupBy(item => item.VatRate)
            .Select(group => new VatRateAmount
            {
                VatRate = group.Key,
                Amount = group
                    .Select(item => item.Amount)
                    .Aggregate((acc, amount) => acc + amount)
            })
            .ToList();

        List<PaymentTypeAmount> amountsPerPaymentType = receipt.Payments
            .GroupBy(payment => payment.Type)
            .Select(group => new PaymentTypeAmount
            {
                PaymentType = group.Key,
                Amount = group
                    .Select(payment => payment.Amount)
                    .Aggregate((acc, amount) => acc + amount)
            })
            .ToList();

        return FinishTransactionRequest.CreateReceipt(
            clientId,
            new Receipt
            {
                ReceiptType = receipt.Type,
                AmountsPerVatRate = amountsPerVatRate,
                AmountsPerPaymentType = amountsPerPaymentType
            });
    }
}
