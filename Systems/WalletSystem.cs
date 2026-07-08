using CitySim.Components;
using CitySim.ECS;
using CitySim.Registries;
using CitySim.Scripts;

namespace CitySim.Systems;


public class WalletSystem(World world) : IUpdateSystem
{
    public void Update(double delta)
    {
        foreach (var entity in world.Entities.With<WalletComponent>())
        {
            if (WalletRegistry.TryGet(entity.Id, out var wallet))
            {
                foreach (var credit in wallet!.Credits)
                {
                    if (SimWorld.Instance.DateTime.Day == credit.DayOfMonth &&
                    (credit.LastTransactionDate == null || credit.LastTransactionDate.Value.Month != SimWorld.Instance.DateTime.Month))
                    {
                        wallet.Balance += credit.Amount;
                        credit.LastTransactionDate = SimWorld.Instance.DateTime;
                    }
                }

                foreach (var debit in wallet!.Debits)
                {
                    if (SimWorld.Instance.DateTime.Day == debit.DayOfMonth &&
                    (debit.LastTransactionDate == null || debit.LastTransactionDate.Value.Month != SimWorld.Instance.DateTime.Month))
                    {
                        wallet.Balance -= debit.Amount;
                        debit.LastTransactionDate = SimWorld.Instance.DateTime;
                    }
                }
            }
        }
    }
}
