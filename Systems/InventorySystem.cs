using System.Linq;
using CitySim.Components;
using CitySim.Data.Facts;
using CitySim.ECS;
using CitySim.Registries;

namespace CitySim.Systems
{
    public class InventorySystem(World world) : IUpdateSystem
    {
        private readonly World _world = world;

        public void Update(double delta)
        {
            #region ItemTransferRequest
            foreach (var entity in _world.Entities.With<ItemTransferRequestComponent>().ToList())
            {
                var request = _world.Get<ItemTransferRequestComponent>(entity);
                bool shouldTransfer = true;

                InventoryRegistry.TryGet(request.SourceEntityID, out var sourceInventory);
                InventoryRegistry.TryGet(entity.Id, out var destInventory);

                if (sourceInventory == null || destInventory == null) shouldTransfer = false;
                else if (sourceInventory.GetAmount(request.Item) < request.Amount)
                {
                    shouldTransfer = false;
                }


                if (shouldTransfer && request.Cost != null)
                {
                    WalletRegistry.TryGet(entity.Id, out var buyerWallet);
                    WalletRegistry.TryGet(request.SourceEntityID, out var sellerWallet);

                    if (buyerWallet != null && sellerWallet != null && buyerWallet!.Balance >= request.Cost.Value)
                    {
                        buyerWallet.Balance -= request.Cost.Value;
                        sellerWallet.Balance += request.Cost.Value;
                    }
                    else
                    {
                        shouldTransfer = false;
                    }
                }

                if (shouldTransfer)
                {
                    sourceInventory!.Remove(request.Item, request.Amount);
                    destInventory!.Add(request.Item, request.Amount);
                }

                // TODO: Explore this more, since the reason for failure should impact the memory generated
                // entity.Get<FactComponent>().Facts.Enqueue(new ItemTransferResultFact()
                // {
                //     Succeeded = shouldTransfer,
                //     Item = request.Item,
                //     EntityID = request.SourceEntityID
                // });


                _world.Detach<ItemTransferRequestComponent>(entity);
                #endregion
            }

        }
    }
}
