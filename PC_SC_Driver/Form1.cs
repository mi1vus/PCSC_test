using MiFare;
using MiFare.Classic;
using MiFare.Devices;
using MiFare.PcSc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PC_SC_Driver
{
    public partial class Form1 : Form
    {
        SmartCardReader reader;
        private MiFareCard card;
        private MiFareCard localCard;

        private readonly byte[] _key1Key = {0x27, 0xA2, 0x9C, 0x10, 0xF8, 0xC7};
        private readonly byte[] _keyDefault = {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
        private readonly byte[] _keyMultiKey = {0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5};

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var readers = CardReader.GetReaderNames().ToArray();
            comboBox1.Items.AddRange(readers);
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(readers.FirstOrDefault(t => t.Contains("CL")) ?? readers.FirstOrDefault(t => t.Contains("PICC")));

            //TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!
            connect.PerformClick();
        }

        private async void connect_Click(object sender, EventArgs e)
        {
            if (reader != null)
            {
                reader.CardAdded -= Reader_CardAdded;
                reader.CardRemoved -= Reader_CardRemoved;
                reader = null;
                connect.Enabled = true;
            }
            if (CardReader.GetReaderNames().Contains(comboBox1.SelectedItem.ToString()))
            {
                reader = await CardReader.FindAsync(comboBox1.SelectedItem.ToString());
                if (reader != null)
                {
                    reader.CardAdded += Reader_CardAdded;
                    reader.CardRemoved += Reader_CardRemoved;
                    connect.Enabled = false;
                }
            }
        }

        private void Reader_CardRemoved(object sender, CardRemovedEventArgs e)
        {
            AddText("Reader_CardRemoved");

        }

        private void Reader_CardAdded(object sender, CardAddedEventArgs e)
        {
            AddText(null);
            AddText("Reader_CardAdded");
            try
            {
                card?.Dispose();

                card = e.SmartCard.CreateMiFareCard();

                localCard = card;

                //btn_reRead.PerformClick();
            }
            catch (Exception ex)
            {
                AddText("HandleCard Exception: " + ex.Message);
            }
        }

        private delegate void AddTextDelegate(string Text);

        private void AddText(string Text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AddTextDelegate(AddText), Text);
            }
            else
            {
                if (Text == null)
                    //textBox1.Text = "";
                    textBox1.AppendText("");
                else
                    //textBox1.Text += ((textBox1.Text.Length>0)?"\r\n":"")+ Text;
                    textBox1.AppendText(((textBox1.Text.Length > 0) ? "\r\n" : "") + Text);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (reader != null)
            {
                reader.CardAdded -= Reader_CardAdded;
                reader.CardRemoved -= Reader_CardRemoved;
                reader = null;
                connect.Enabled = true;
                AddText(null);
            }
        }

        private void btn_reRead_Click(object sender, EventArgs e)
        {
            reRead();
        }
        private async void reRead()
        {
            if (localCard == null)
                return;

            try
            {
                textBox1.Clear();

                var cardIdentification = await localCard.GetCardInfo();

                AddText("Connected to card\r\nPC/SC device class: " + cardIdentification.PcscDeviceClass.ToString() +
                        "\r\nCard name: " + cardIdentification.PcscCardName.ToString());

                if (cardIdentification.PcscDeviceClass == MiFare.PcSc.DeviceClass.StorageClass
                    &&
                    (cardIdentification.PcscCardName == CardName.MifareStandard1K ||
                     cardIdentification.PcscCardName == CardName.MifareStandard4K))
                {
                    // Handle MIFARE Standard/Classic
                    AddText($"MIFARE Standard/Classic card detected");

                    var uid = await localCard.GetUid();
                    AddText("UID:  " + BitConverter.ToString(uid));

                    bool card_1_key = uid[0] == 0xBC;

                    // 16 sectors, print out each one
                    for (var sector = 3; sector < 4 && card != null; sector++)
                    {
                        AddText("=========================================================");
                        for (var block = 0; block < 4; block++)
                        {
                            try
                            {
                                //localCard.AddOrUpdateSectorKeySet(new SectorKeySet() { KeyType = KeyType.KeyA, Sector = sector, Key = keyDefault });
                                //var sec = localCard.GetSector(1);
                                //var d = await sec.GetData(0);
                                var data = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

                                if (card_1_key)
                                {
                                    if (sector == 1)
                                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                                        {
                                            KeyType = KeyType.KeyA,
                                            Sector = sector,
                                            Key = _key1Key
                                        });

                                    //localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyA, Sector = sector, Key = keyDefault });
                                    //localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyB, Sector = sector, Key = keyDefault });

                                    var sec = localCard.GetSector(sector);
                                    data = await sec.GetData(block);
                                }
                                else
                                {
                                    if (sector == 3)
                                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                                        {
                                            KeyType = KeyType.KeyA,
                                            Sector = sector,
                                            Key = _keyDefault
                                        });
                                    else
                                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                                        {
                                            KeyType = KeyType.KeyB,
                                            Sector = sector,
                                            Key = _keyMultiKey
                                        });

                                    var sec = localCard.GetSector(sector);
                                    data = await sec.GetData(block);
                                }
                                //else
                                //{
                                //    continue;
                                //}

                                string hexString = "|| ";
                                for (int i = 0; i < data.Length; i++)
                                {
                                    if (i == 6 || i == 10)
                                        hexString += " || ";
                                    hexString += data[i].ToString("X2") + " ";
                                }
                                hexString += " ||";

                                AddText($"Sector '{sector}'[{block}]:{hexString}");
                            }
                            catch (Exception ex)
                            {
                                AddText("Failed to load sector: " + sector + "\r\nEx: " + ex.ToString());
                            }
                        }
                    }
                    AddText("=========================================================");
                }
                return;
            }
            catch (Exception ex)
            {
                AddText("reRead HandleCard Exception: " + ex.Message);
                return;
            }
        }

        private void btn_onlyRead_Click(object sender, EventArgs e)
        {
            onlyRead();
        }
        private async void onlyRead()
        {
            if (localCard == null)
                return;

            try
            {
                var cardIdentification = await localCard.GetCardInfo();

                AddText("Connected to card\r\nPC/SC device class: " + cardIdentification.PcscDeviceClass.ToString() +
                        "\r\nCard name: " + cardIdentification.PcscCardName.ToString());

                if (cardIdentification.PcscDeviceClass == MiFare.PcSc.DeviceClass.StorageClass &&
                    (cardIdentification.PcscCardName == CardName.MifareStandard1K ||
                     cardIdentification.PcscCardName == CardName.MifareStandard4K))
                {
                    // Handle MIFARE Standard/Classic
                    AddText("MIFARE Standard/Classic card detected");

                    var uid = await localCard.GetUid();
                    AddText("UID:  " + BitConverter.ToString(uid));

                    AddText("Configurate sectpr 2 to read from KeyA");

                    bool card1Key = uid[0] == 0xBC;

                    // 16 sectors, print out each one

                    var sector = 3;
                    if (card1Key)
                    {
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyB,
                            Sector = sector,
                            Key = _keyDefault
                        });

                        var sec = localCard.GetSector(sector);
                        sec.Access.DataAreas[0].Read = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[0].Write = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[0].Decrement = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[0].Increment = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[1].Read = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[1].Write = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[1].Decrement = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[1].Increment = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[2].Read = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[2].Write = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[2].Decrement = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[2].Increment = DataAreaAccessCondition.ConditionEnum.KeyAOrB;

                        sec.Access.Trailer.AccessBitsRead = TrailerAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.Trailer.AccessBitsWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                        sec.Access.Trailer.KeyARead = TrailerAccessCondition.ConditionEnum.KeyB;
                        sec.Access.Trailer.KeyAWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                        sec.Access.Trailer.KeyBRead = TrailerAccessCondition.ConditionEnum.KeyB;
                        sec.Access.Trailer.KeyBWrite = TrailerAccessCondition.ConditionEnum.KeyB;


                        sec.KeyA = _keyDefault.ByteArrayToString();
                        sec.KeyB = _key1Key.ByteArrayToString();

                        await sec.Flush();

                        // During init, use the master key for both. This will be changed to the user pin-derived key
                        await sec.FlushTrailer(_keyDefault.ByteArrayToString(), _keyDefault.ByteArrayToString());

                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyA,
                            Sector = sector,
                            Key = _keyDefault
                        });
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyB,
                            Sector = sector,
                            Key = _key1Key
                        });
                    }
                    else
                    {
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyB,
                            Sector = sector,
                            Key = _keyMultiKey
                        });

                        var sec = localCard.GetSector(sector);
                        sec.Access.DataAreas[0].Read = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[0].Write = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[0].Decrement = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[0].Increment = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[1].Read = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[1].Write = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[1].Decrement = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[1].Increment = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[2].Read = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[2].Write = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[2].Decrement = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.DataAreas[2].Increment = DataAreaAccessCondition.ConditionEnum.KeyAOrB;

                        sec.Access.Trailer.AccessBitsRead = TrailerAccessCondition.ConditionEnum.KeyAOrB;
                        sec.Access.Trailer.AccessBitsWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                        sec.Access.Trailer.KeyARead = TrailerAccessCondition.ConditionEnum.KeyB;
                        sec.Access.Trailer.KeyAWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                        sec.Access.Trailer.KeyBRead = TrailerAccessCondition.ConditionEnum.KeyB;
                        sec.Access.Trailer.KeyBWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                        sec.KeyA = _keyDefault.ByteArrayToString();
                        sec.KeyB = _keyMultiKey.ByteArrayToString();

                        await sec.Flush();

                        // During init, use the master key for both. This will be changed to the user pin-derived key
                        await sec.FlushTrailer(_keyDefault.ByteArrayToString(), _keyMultiKey.ByteArrayToString());

                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyA,
                            Sector = sector,
                            Key = _keyDefault
                        });
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyB,
                            Sector = sector,
                            Key = _keyMultiKey
                        });

                    }
                    //localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyA, Sector = sector, Key = keyDefault });
                    //localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyB, Sector = sector, Key = keyDefault });

                }
            }
            catch (Exception ex)
            {
                AddText("btn_onlyRead_Click HandleCard Exception: " + ex.ToString());
                return;
            }
        }

        private void btn_andWrite_Click(object sender, EventArgs e)
        {
            readAndWrite();
        }
        private async void readAndWrite()
        {
            if (localCard == null)
                return;

            try
            {
                var cardIdentification = await localCard.GetCardInfo();

                AddText("Connected to card\r\nPC/SC device class: " + cardIdentification.PcscDeviceClass.ToString() +
                        "\r\nCard name: " + cardIdentification.PcscCardName.ToString());

                if (cardIdentification.PcscDeviceClass == MiFare.PcSc.DeviceClass.StorageClass &&
                    (cardIdentification.PcscCardName == CardName.MifareStandard1K ||
                     cardIdentification.PcscCardName == CardName.MifareStandard4K))
                {
                    // Handle MIFARE Standard/Classic
                    AddText("MIFARE Standard/Classic card detected");

                    var uid = await localCard.GetUid();
                    AddText("UID:  " + BitConverter.ToString(uid));

                    AddText("Configurate sectpr 2 to read from KeyA and write from key A");

                    bool card1Key = uid[0] == 0xBC;

                    // 16 sectors, print out each one

                    var sector = 3;
                    if (card1Key)
                    {
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyB,
                            Sector = sector,
                            Key = _key1Key
                        });

                        var sec = localCard.GetSector(sector);
                        sec.Access.DataAreas[0].Read = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[0].Write = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[0].Decrement = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[0].Increment = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[1].Read = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[1].Write = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[1].Decrement = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[1].Increment = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[2].Read = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[2].Write = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[2].Decrement = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[2].Increment = DataAreaAccessCondition.ConditionEnum.KeyA;

                        sec.Access.Trailer.AccessBitsRead = TrailerAccessCondition.ConditionEnum.KeyA;
                        sec.Access.Trailer.AccessBitsWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                        sec.Access.Trailer.KeyARead = TrailerAccessCondition.ConditionEnum.KeyA;
                        sec.Access.Trailer.KeyAWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                        sec.Access.Trailer.KeyBRead = TrailerAccessCondition.ConditionEnum.KeyA;
                        sec.Access.Trailer.KeyBWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                        sec.KeyA = _keyDefault.ByteArrayToString();
                        sec.KeyB = _key1Key.ByteArrayToString();

                        await sec.Flush();

                        // During init, use the master key for both. This will be changed to the user pin-derived key
                        await sec.FlushTrailer(_keyDefault.ByteArrayToString(), _key1Key.ByteArrayToString());

                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyA,
                            Sector = sector,
                            Key = _keyDefault
                        });
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyB,
                            Sector = sector,
                            Key = _key1Key
                        });
                    }
                    else
                    {
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyB,
                            Sector = sector,
                            Key = _keyMultiKey
                        });

                        var sec = localCard.GetSector(sector);
                        sec.Access.DataAreas[0].Read = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[0].Write = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[0].Decrement = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[0].Increment = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[1].Read = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[1].Write = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[1].Decrement = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[1].Increment = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[2].Read = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[2].Write = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[2].Decrement = DataAreaAccessCondition.ConditionEnum.KeyA;
                        sec.Access.DataAreas[2].Increment = DataAreaAccessCondition.ConditionEnum.KeyA;

                        sec.Access.Trailer.AccessBitsRead = TrailerAccessCondition.ConditionEnum.KeyA;
                        sec.Access.Trailer.AccessBitsWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                        sec.Access.Trailer.KeyARead = TrailerAccessCondition.ConditionEnum.KeyA;
                        sec.Access.Trailer.KeyAWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                        sec.Access.Trailer.KeyBRead = TrailerAccessCondition.ConditionEnum.KeyA;
                        sec.Access.Trailer.KeyBWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                        sec.KeyA = _keyDefault.ByteArrayToString();
                        sec.KeyB = _keyMultiKey.ByteArrayToString();

                        await sec.Flush();

                        // During init, use the master key for both. This will be changed to the user pin-derived key
                        await sec.FlushTrailer(_keyDefault.ByteArrayToString(), _keyMultiKey.ByteArrayToString());

                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyA,
                            Sector = sector,
                            Key = _keyDefault
                        });
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyB,
                            Sector = sector,
                            Key = _keyMultiKey
                        });

                    }
                    //localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyA, Sector = sector, Key = keyDefault });
                    //localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyB, Sector = sector, Key = keyDefault });

                }
            }
            catch (Exception ex)
            {
                AddText("btn_onlyRead_Click HandleCard Exception: " + ex.ToString());
                return;
            }
        }

        private void btn_writeTo_Click(object sender, EventArgs e)
        {
            writeFromB();
        }
        private async void writeFromB()
        {
            if (localCard == null)
                return;

            try
            {
                var cardIdentification = await localCard.GetCardInfo();

                AddText("Connected to card\r\nPC/SC device class: " + cardIdentification.PcscDeviceClass.ToString() +
                        "\r\nCard name: " + cardIdentification.PcscCardName.ToString());

                if (cardIdentification.PcscDeviceClass == MiFare.PcSc.DeviceClass.StorageClass &&
                    (cardIdentification.PcscCardName == CardName.MifareStandard1K ||
                     cardIdentification.PcscCardName == CardName.MifareStandard4K))
                {
                    // Handle MIFARE Standard/Classic
                    AddText("MIFARE Standard/Classic card detected");

                    var uid = await localCard.GetUid();
                    AddText("UID:  " + BitConverter.ToString(uid));

                    AddText("Write to sector 3[0] - 012345");

                    bool card1Key = uid[0] == 0xBC;

                    // 16 sectors, print out each one

                    var sector = 3;
                    if (card1Key)
                    {
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyB, Sector = sector, Key = _keyDefault });

                        var sec = localCard.GetSector(sector);
                        await sec.SetData(new byte[] {0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x05/*, 0x06/*, 0x07, 0x00, 0x00*/ }, 0);
                        await sec.Flush();
                    }
                    else
                    {
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet{KeyType = KeyType.KeyB,Sector = sector,Key = _keyMultiKey});

                        var sec = localCard.GetSector(sector);
                        await sec.SetData(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x05/*, 0x06/*, 0x07, 0x00, 0x00*/ }, 0);
                        await sec.Flush();
                    }
                    //localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyA, Sector = sector, Key = keyDefault });
                    //localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyB, Sector = sector, Key = keyDefault });

                }
            }catch (Exception ex)
            {
                AddText("writeFromB HandleCard Exception: " + ex.ToString());
                return;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            writeFromA();
        }
        private async void writeFromA()
        {
            if (localCard == null)
                return;

            try
            {
                var cardIdentification = await localCard.GetCardInfo();

                AddText("Connected to card\r\nPC/SC device class: " + cardIdentification.PcscDeviceClass.ToString() +
                        "\r\nCard name: " + cardIdentification.PcscCardName.ToString());

                if (cardIdentification.PcscDeviceClass == MiFare.PcSc.DeviceClass.StorageClass &&
                    (cardIdentification.PcscCardName == CardName.MifareStandard1K ||
                     cardIdentification.PcscCardName == CardName.MifareStandard4K))
                {
                    // Handle MIFARE Standard/Classic
                    AddText("MIFARE Standard/Classic card detected");

                    var uid = await localCard.GetUid();
                    AddText("UID:  " + BitConverter.ToString(uid));

                    AddText("Write to sector 3[0] - 012345");

                    bool card1Key = uid[0] == 0xBC;

                    // 16 sectors, print out each one

                    var sector = 3;
                    if (card1Key)
                    {
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyA, Sector = sector, Key = _keyDefault });

                        var sec = localCard.GetSector(sector);
                        await sec.SetData(new byte[] { 0x00, 0x01, 0x02, 0x00, 0x04, 0x05, 0x09/*, 0x06/*, 0x07, 0x00, 0x00*/ }, 0);
                        await sec.Flush();
                    }
                    else
                    {
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyA, Sector = sector, Key = _keyDefault });

                        var sec = localCard.GetSector(sector);
                        await sec.SetData(new byte[] { 0x00, 0x01, 0x02, 0x00, 0x04, 0x05, 0x09/*, 0x06/*, 0x07, 0x00, 0x00*/ }, 0);
                        await sec.Flush();
                    }
                    //localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyA, Sector = sector, Key = keyDefault });
                    //localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyB, Sector = sector, Key = keyDefault });

                }
            }
            catch (Exception ex)
            {
                AddText("writeFromB HandleCard Exception: " + ex.ToString());
                return;
            }
        }

        private void btn_erase_Click(object sender, EventArgs e)
        {
            erase();
        }
        private async void erase()
        {
            if (localCard == null)
                return;

            try
            {
                var cardIdentification = await localCard.GetCardInfo();

                AddText("Connected to card\r\nPC/SC device class: " + cardIdentification.PcscDeviceClass.ToString() +
                        "\r\nCard name: " + cardIdentification.PcscCardName.ToString());

                if (cardIdentification.PcscDeviceClass == MiFare.PcSc.DeviceClass.StorageClass &&
                    (cardIdentification.PcscCardName == CardName.MifareStandard1K ||
                     cardIdentification.PcscCardName == CardName.MifareStandard4K))
                {
                    // Handle MIFARE Standard/Classic
                    AddText("MIFARE Standard/Classic card detected");

                    var uid = await localCard.GetUid();
                    AddText("UID:  " + BitConverter.ToString(uid));

                    AddText("Write to sector 3[0] - 012345");

                    bool card1Key = uid[0] == 0xBC;

                    // 16 sectors, print out each one

                    var sector = 3;
                    if (card1Key)
                    {
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyB, Sector = sector, Key = _keyDefault });

                        var sec = localCard.GetSector(sector);
                        await sec.SetData(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0);
                        await sec.Flush();
                    }
                    else
                    {
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyB, Sector = sector, Key = _keyMultiKey });

                        var sec = localCard.GetSector(sector);
                        await sec.SetData(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0);
                        await sec.Flush();
                    }
                    //localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyA, Sector = sector, Key = keyDefault });
                    //localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyB, Sector = sector, Key = keyDefault });

                }
            }
            catch (Exception ex)
            {
                AddText("writeFromB HandleCard Exception: " + ex.ToString());
                return;
            }
        }
    }
}
