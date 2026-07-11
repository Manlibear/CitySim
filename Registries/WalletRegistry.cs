using System;
using System.Collections.Generic;
using CitySim.Data;

namespace CitySim.Registries
{
    public static class WalletRegistry
    {
        private static Dictionary<Guid, Wallet> _wallets = null!;
        public static void Initialize() => _wallets = [];

        public static void Register(Guid id) => _wallets[id] = new Wallet();
        public static void Register(Guid id, Wallet wallet) => _wallets[id] = wallet;

        public static bool TryGet(Guid id, out Wallet? wallet) => _wallets.TryGetValue(id, out wallet);
        public static Wallet Get(Guid id) => _wallets[id];

        public static Dictionary<Guid, Wallet> Get() => _wallets;


    }
}
