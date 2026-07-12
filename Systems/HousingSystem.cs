using CitySim.Components;
using CitySim.Data.Facts;
using CitySim.ECS;
using CitySim.Registries;
using CitySim.Scripts;

namespace CitySim.Systems;

public class HousingSystem(World world) : IUpdateSystem
{
    public void Update(double delta)
    {
        foreach (var entity in world.Entities.With<HomeComponent>())
        {
            var homeComp = entity.Get<HomeComponent>();
            var wallet = WalletRegistry.Get(entity.Id);

            if (homeComp.Cost != null && SimWorld.Instance.DateTime.Day == homeComp.Cost.DayOfMonth &&
                (homeComp.Cost.LastTransactionDate == null || homeComp.Cost.LastTransactionDate.Value.Month != SimWorld.Instance.DateTime.Month))
            {


                if (homeComp.HomeOwner)
                {
                    // adjust the balance of the mortgage by the inflation rate
                    homeComp.Mortgage *= SimWorld.Instance.InterestRate / 12;

                    if (homeComp.Cost!.Amount > homeComp.Mortgage)
                        homeComp.Cost.Amount = homeComp.Mortgage.Value;

                    if (wallet.Debit(homeComp.Cost.Amount))
                    {
                        homeComp.Mortgage -= homeComp.Cost.Amount;
                        homeComp.Cost.LastTransactionDate = SimWorld.Instance.DateTime;
                    }
                    else entity.Get<FactComponent>().Add(new MissedPaymentFact(homeComp.Cost.Amount));

                    if (homeComp.Cost!.Amount == 0)
                    {
                        homeComp.Cost = null;
                        entity.Get<FactComponent>().Add(new MortgagePaidOffFact());
                    }

                }
                else
                {
                    if (wallet.Debit(homeComp.Cost.Amount))
                    {
                        var landlordWallet = WalletRegistry.Get(homeComp.LandlordID!.Value);
                        landlordWallet.Credit(homeComp.Cost.Amount);
                        homeComp.Cost.LastTransactionDate = SimWorld.Instance.DateTime;
                    }
                    else entity.Get<FactComponent>().Add(new MissedPaymentFact(homeComp.Cost.Amount));
                }
            }
        }
    }
}
