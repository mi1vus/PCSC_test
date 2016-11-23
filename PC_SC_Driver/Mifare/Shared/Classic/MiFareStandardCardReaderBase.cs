﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiFare.PcSc;
using MiFare.PcSc.Iso7816;
using MiFare.PcSc.MiFareStandard;
using ApduResponse = MiFare.PcSc.Iso7816.ApduResponse;
using GeneralAuthenticate = MiFare.PcSc.GeneralAuthenticate;
using GetUid = MiFare.PcSc.MiFareStandard.GetUid;

namespace MiFare.Classic
{
    abstract class MiFareStandardCardReaderBase : ICardReader, IDisposable
    {
        protected abstract Task<byte[]> GetAnswerToResetAsync();

        protected abstract Task<ApduResponse> TransceiveAsync(ApduCommand apduCommand);
        
        private static readonly byte[] DefaultKey = Defaults.KeyA;
       
        private Dictionary<SectorKey, byte[]> keyMap = new Dictionary<SectorKey, byte[]>();

        private byte nextKeySlot;

        private Dictionary<byte[], byte> keyToLocationMap = new Dictionary<byte[], byte>(KeyEqualityComparer.Default);

        protected MiFareStandardCardReaderBase(ICollection<SectorKeySet> keys)
        {
            if (!keys.All(set => set.IsValid))
            {
                var key = keys.First(k => !k.IsValid);
                throw new ArgumentException($"KeySet with Sector {key.Sector}, KeyType {key.KeyType} is invalid", nameof(keys));
            }


            PopulateKeyMap(keys);

        }

        private void PopulateKeyMap(IEnumerable<SectorKeySet> keys)
        {
            foreach (var keySet in keys)
            {
                keyMap.Add(new SectorKey(keySet.Sector, (InternalKeyType)keySet.KeyType), keySet.Key);
            }
        }

        public async Task<IccDetection> GetCardInfo()
        {
          
            var atrbytes = await GetAnswerToResetAsync();
            var cardIdentification = new IccDetection(atrbytes);

            return cardIdentification;
        }

        public void AddOrUpdateSectorKeySet(SectorKeySet keySet)
        {
            if (keySet == null) throw new ArgumentNullException(nameof(keySet));
            if (!keySet.IsValid)
            {
                throw new ArgumentException($"KeySet with Sector {keySet.Sector}, KeyType {keySet.KeyType} is invalid", nameof(keySet));
            }

            // Add or update the sector key in the map
            keyMap[new SectorKey(keySet.Sector, (InternalKeyType)keySet.KeyType)] = keySet.Key;
        }

        public async Task<bool> Login(int sector, InternalKeyType key)
        {
            LoadKeys.LoadKeysStorageType storage = LoadKeys.LoadKeysStorageType.NonVolatile;
            if (this.GetType() == typeof(MiFareWin32CardReader)
                && (this as MiFareWin32CardReader).SmartCard.ReaderName.Contains("1252"))
            {
                storage = LoadKeys.LoadKeysStorageType.Volatile;
                System.Windows.Forms.MessageBox.Show("1251");
            }



//            storage = LoadKeys.LoadKeysStorageType.Volatile;



            var keyTypeToUse = key;
            byte[] keyToUse;

            if (key == InternalKeyType.KeyDefaultF)
            {
                // If it's a default request, load the default key and use KeyA
                keyToUse = DefaultKey;
                keyTypeToUse = InternalKeyType.KeyA;
            }
            else
            {
                //try to find the right key for the sector
                if (!keyMap.TryGetValue(new SectorKey(sector, key), out keyToUse))
                {
                    return false; // No provided key type for the sector
                }
            }

            var gaKeyType = (GeneralAuthenticate.GeneralAuthenticateKeyType)keyTypeToUse;

            // Get the first block for the sector
            var blockNumber = SectorToBlock(sector, 0);

            // if reader have only one KeySlot - not store key slots to collection (not cache)
            byte location = 0;
            if (this.GetType() == typeof(MiFareWin32CardReader)
                && (this as MiFareWin32CardReader).SmartCard.ReaderName.Contains("FEIG"))
            {
                var r = await TransceiveAsync(new LoadKey(keyToUse, location, storage));
                if (!r.Succeeded)
                    return false; // could not load the key
            }
            // see if we have the key loaded already
            else if (!keyToLocationMap.TryGetValue(keyToUse, out location))
            {
                location = nextKeySlot;
                nextKeySlot++;
                keyToLocationMap[keyToUse] = location;

                //Load the key to the location
                var r = await TransceiveAsync(new LoadKey(keyToUse, location, storage));
                if (!r.Succeeded)
                    return false; // could not load the key
            }

            if (storage == LoadKeys.LoadKeysStorageType.Volatile)
            {
                ApduResponse res;
                for (int z = 0; z < 5; z++)
                {
                    res = await TransceiveAsync(new PcSc.MiFareStandard.GeneralAuthenticate(blockNumber, location, gaKeyType));
                    if (res.Succeeded)
                        return true;
                    System.Threading.Thread.Sleep(500);
                }
                return false;
            }
            else
                return (await TransceiveAsync(new PcSc.MiFareStandard.GeneralAuthenticate(blockNumber, location, gaKeyType))).Succeeded;

            //return res.Succeeded;
        }

        public async Task<Tuple<bool, byte[]>> Read(int sector, int datablock)
        {
            var blockNumber = SectorToBlock(sector, datablock);

            var readRes = await TransceiveAsync(new Read(blockNumber));

            //card.Reader
            if (this.GetType() == typeof(MiFareWin32CardReader)
                && (this as MiFareWin32CardReader).SmartCard.ReaderName.Contains("FEIG"))
            {
                //TODO 
                Array.Reverse(readRes.ResponseData);
            }

            return Tuple.Create(readRes.Succeeded, readRes.ResponseData);
        }

        public async Task<bool> Write(int sector, int datablock, byte[] data)
        {
            var blockNumber = SectorToBlock(sector, datablock);
            byte[] local_data = new byte[data.Length];
            Array.Copy(data, local_data, data.Length);
            //card.Reader
            if (this.GetType() == typeof(MiFareWin32CardReader)
                && (this as MiFareWin32CardReader).SmartCard.ReaderName.Contains("FEIG"))
            {
                //TODO 
                Array.Reverse(local_data);
            }

            var write = new Write(blockNumber, ref local_data);
            var adpuRes = await TransceiveAsync(write);

            return adpuRes.Succeeded;
        }

        /// <summary>
        ///     Wrapper method get the Mifare Standard ICC UID
        /// </summary>
        /// <returns>byte array UID</returns>
        public async Task<byte[]> GetUid()
        {
            var apduRes = await TransceiveAsync(new GetUid());
            if (!apduRes.Succeeded)
            {
                throw new Exception("Failure getting UID of MIFARE Standard card, " + apduRes);
            }

            return apduRes.ResponseData;
        }

        // Sector to block

        private static byte SectorToBlock(int sector, int dataBlock)
        {
            if (sector >= 40 || sector < 0)
                throw new ArgumentOutOfRangeException(nameof(sector), "sector must be between 0 and 39");

            if (dataBlock < 0)
                throw new ArgumentOutOfRangeException(nameof(dataBlock), "value must be greater or equal to 0");

            if (sector < 32 && dataBlock >= 4)
                throw new ArgumentOutOfRangeException(nameof(dataBlock), "Sectors 0-31 only have data blocks 0-3");
            if (dataBlock >= 16)
                throw new ArgumentOutOfRangeException(nameof(dataBlock), "Sectors 32-39 have data blocks 0-15");

            int block;
            // first 32 sectors are 4 blocks
            // last 8 are 16 blocks
            if (sector < 32)
            {
                block = (sector * 4) + dataBlock;
            }
            else
            {
                const int startingBlock = 32 * 4; // initial block number
                var largeSectors = sector - 32; // number of 16 block sectors
                block = (largeSectors * 16) + dataBlock + startingBlock;
            }

            return (byte)block;
        }

        protected virtual void Dispose(bool disposing)
        {
            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MiFareStandardCardReaderBase()
        {
            Dispose(false);
        }

        private class KeyEqualityComparer : IEqualityComparer<byte[]>
        {
            public static readonly KeyEqualityComparer Default = new KeyEqualityComparer(); 
            public bool Equals(byte[] x, byte[] y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(byte[] obj)
            {
                unchecked
                {
                    var start = obj.Aggregate(obj[0] * 397, (current, b) => current ^ b.GetHashCode());
                    return start;
                }
            }
        }
    }
}
