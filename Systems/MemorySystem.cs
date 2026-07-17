using System.Linq;
using CitySim.Components;
using CitySim.Data;
using CitySim.Data.Facts;
using CitySim.ECS;
using CitySim.Registries;

namespace CitySim.Systems;


public class MemorySystem(World world) : IUpdateSystem
{
    public void Update(double delta)
    {
        foreach (var entity in world.Entities.With<FactComponent>().With<MemoryComponent>().ToList())
        {
            var factComp = entity.Get<FactComponent>();
            var memoryComp = entity.Get<MemoryComponent>();
            var needsComp = entity.Get<NeedsComponent>();
            var wallet = WalletRegistry.Get(entity.Id);

            while (factComp.Facts.Any())
            {
                var newFact = factComp.Facts.Dequeue();

                switch (newFact)
                {
                    case ItemTransferResultFact itr:
                        if (!itr.Succeeded)
                        {
                            var itemDef = ItemRegistry.Get(itr.Item);

                            memoryComp.Memories.Add(new ShopQueryMemory()
                            {
                                Available = false,
                                EntityID = itr.EntityID,
                                Item = itr.Item,
                                //TODO: This needs inflating
                                Satisfaction = -ComputeMoodFromItem(entity, needsComp, itemDef.Type)
                            });
                        }
                        break;

                    case BrowseResultFact br:
                        memoryComp.Memories.Add(new ShopQueryMemory()
                        {
                            Available = br.Success,
                            EntityID = br.EntityID,
                            Tag = br.Tag,
                            ItemType = br.ItemType,
                            Satisfaction = br.TotalScore
                        });
                        break;

                    case JobInterviewFact jif:
                        memoryComp.Memories.Add(new ConfidenceMemory()
                        {
                            // TODO: bullshit guess numbers
                            Satisfaction = jif.Success ? 20 : -5,
                            Type = ActivityType.Interview
                        });
                        break;

                    case MortgagePaidOffFact mpof:
                        memoryComp.Memories.Add(new FinancialMemory()
                        {
                            Satisfaction = 40
                        });
                        break;

                    case MissedPaymentFact mpf:
                        memoryComp.Memories.Add(new FinancialMemory()
                        {
                            Satisfaction = -((float)mpf.Amount * .1f)
                        });
                        break;

                    case FiredFromJobFact ffjf:
                        memoryComp.Memories.Add(new JobMemory()
                        {
                            Employer = ffjf.Employer,
                            Satisfaction = -100
                        });
                        break;

                    case ItemCostFact icf:

                        var satisfactionSign = icf.CostFactor > 1 ? -1 : 1;

                        memoryComp.Memories.Add(new ItemCostMemory()
                        {
                            Item = icf.Item,
                            EntityID = icf.ShopID,
                            //TODO: Yet more bullshit maths
                            Satisfaction = (float)((Globals.ComfortableBalance / wallet.Balance) * icf.CostFactor) * 5f * satisfactionSign
                        });
                        break;

                    case SocialInteractionFact sif:

                        memoryComp.Memories.Add(new SocialInteractionMemory()
                        {
                            OtherPersonID = sif.OtherPersonID,
                            Satisfaction = (sif.Positive ? 1 : -1) * (float)sif.Duration
                        });

                        break;
                }
            }
        }
    }

    private float ComputeMoodFromItem(Entity entity, NeedsComponent needsComp, ItemType itemType)
    {
        // TODO: This is complete crap
        return itemType switch
        {
            ItemType.Food => 1 - needsComp.Satiety,
            _ => .2f,
        };
    }
}
