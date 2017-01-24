using MiFare;
using MiFare.Classic;
using MiFare.Devices;
using MiFare.PcSc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ServioBonus;

namespace PC_SC_Driver
{
    public partial class Form1 : Form
    {
        SmartCardReader reader;
        private MiFareCard card;
        private MiFareCard localCard;

        #region Keys

        private readonly byte[] _keyNull = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
        private readonly byte[] _key1Key = {0x27, 0xA2, 0x9C, 0x10, 0xF8, 0xC7};
        private readonly byte[] _keyDefault = {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
        private readonly byte[] _keyMultiKeyA = {0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5};
        private readonly byte[] _keyMultiKeyB = {0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5};

        #endregion

        #region Kards

        private static HashSet<Tuple<int, int>> _cardBadSectors = null;

        private static readonly string[,] EmptyCard =
        {
            {
                "",
                "00000000000000000000000000000000", // 1
                "00000000000000000000000000000000", // 2
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 4
                "00000000000000000000000000000000", // 5
                "00000000000000000000000000000000", // 6
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 8
                "00000000000000000000000000000000", // 9
                "00000000000000000000000000000000", // 10
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 12
                "00000000000000000000000000000000", // 13
                "00000000000000000000000000000000", // 14
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 16
                "00000000000000000000000000000000", // 17
                "00000000000000000000000000000000", // 18
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 20
                "00000000000000000000000000000000", // 21
                "00000000000000000000000000000000", // 22
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 24
                "00000000000000000000000000000000", // 25
                "00000000000000000000000000000000", // 26
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 28
                "00000000000000000000000000000000", // 29
                "00000000000000000000000000000000", // 30
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 32
                "00000000000000000000000000000000", // 33
                "00000000000000000000000000000000", // 34
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 36
                "00000000000000000000000000000000", // 37
                "00000000000000000000000000000000", // 38
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 40
                "00000000000000000000000000000000", // 41
                "00000000000000000000000000000000", // 42
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 44
                "00000000000000000000000000000000", // 45
                "00000000000000000000000000000000", // 46
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 48
                "00000000000000000000000000000000", // 49
                "00000000000000000000000000000000", // 50
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 52
                "00000000000000000000000000000000", // 53
                "00000000000000000000000000000000", // 54
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 56
                "00000000000000000000000000000000", // 57
                "00000000000000000000000000000000", // 58
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 60
                "00000000000000000000000000000000", // 61
                "00000000000000000000000000000000", // 62
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            }
        };

        private static readonly string[,] TestCard =
        {
            {
                "",
                "00000000000000000000000000000000", // 1
                "00000000000000000000000000000000", // 2
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "31313431373334323237000000000000", // 4
                "0000000000E803000001000000760006", // 5
                "E7E62FDC14DAE4400000000000000000", // 6
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 8
                "E8030000C409000088130000FF014300", // 9
                "00000000000000000000000000000000", // 10
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 12
                "00000000000000000000000000000000", // 13
                "00000000000000000000000000000000", // 14
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 16
                "00000000000000000000000000000000", // 17
                "00000000000000000000000000000000", // 18
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 20
                "00000000000000000000000000000000", // 21
                "00000000000000000000000000000000", // 22
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 24
                "00000000000000000000000000000000", // 25
                "00000000000000000000000000000000", // 26
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 28
                "00000000000000000000000000000000", // 29
                "00000000000000000000000000000000", // 30
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 32
                "00000000000000000000000000000000", // 33
                "00000000000000000000000000000000", // 34
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 36
                "00000000000000000000000000000000", // 37
                "00000000000000000000000000000000", // 38
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 40
                "00000000000000000000000000000000", // 41
                "00000000000000000000000000000000", // 42
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 44
                "00000000000000000000000000000000", // 45
                "00000000000000000000000000000000", // 46
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 48
                "00000000000000000000000000000000", // 49
                "00000000000000000000000000000000", // 50
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 52
                "00000000000000000000000000000000", // 53
                "00000000000000000000000000000000", // 54
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 56
                "00000000000000000000000000000000", // 57
                "00000000000000000000000000000000", // 58
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            },
            {
                "00000000000000000000000000000000", // 60
                "00000000000000000000000000000000", // 61
                "00000000000000000000000000000000", // 62
                "A0A1A2A3A4A5FF078000B0B1B2B3B4B5"
            }
        };

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var readers = CardReader.GetReaderNames().ToArray();
            comboBox1.Items.AddRange(readers);
            comboBox1.SelectedIndex =
                comboBox1.Items.IndexOf(readers.FirstOrDefault(t => t.Contains("CL")) ??
                                        readers.FirstOrDefault(t => t.Contains("PICC")));

            //TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!
            //connect.PerformClick();
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
            _cardBadSectors?.Clear();

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

                _cardBadSectors = ReadBadsFromFileSector();

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
            if (InvokeRequired)
            {
                Invoke(new AddTextDelegate(AddText), Text);
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

                if (cardIdentification.PcscDeviceClass == DeviceClass.StorageClass
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
                    for (var sector = 0; sector < 16 && card != null; sector++)
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
                                AccessConditions access = null;
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
                                    //if (sector == 3)
                                    //    localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                                    //    {
                                    //        KeyType = KeyType.KeyB,
                                    //        Sector = sector,
                                    //        Key = _key1Key
                                    //    });
                                    //else
                                    //localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                                    //{
                                    //    KeyType = KeyType.KeyA,
                                    //    Sector = sector,
                                    //    Key = _keyMultiKeyA
                                    //});
                                    localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                                    {
                                        KeyType = KeyType.KeyB,
                                        Sector = sector,
                                        Key = _keyMultiKeyB
                                    });
                                    if (_cardBadSectors?.Contains(new Tuple<int, int>(sector, block)) ?? false)
                                        data = ReadFromFileSector(sector, block);
                                    else
                                    {
                                        var sec = localCard.GetSector(sector);
                                        data = await sec.GetData(block);

                                        if (data != null && block == sec.NumDataBlocks - 1)
                                            access = AccessBits.GetAccessConditions(data);
                                        else
                                            access = null;
                                    }
                                }
                                //else
                                //{
                                //    continue;
                                //}

                                if (data == null)
                                {
                                    AddText($"Sector '{sector}'[{block}]: read error!");
                                    return;
                                }

                                string hexString = "|| ";
                                for (int i = 0; i < data.Length; i++)
                                {
                                    if (i == 6 || i == 10)
                                        hexString += " || ";
                                    hexString += data[i].ToString("X2") + " ";
                                }
                                hexString += " ||";

                                AddText($"Sector '{sector}'[{block}]:{hexString}");

                                if (access != null)
                                {
                                    string accStr =
                                        $"Access d0: read - {access.DataAreas[0].Read}, write - {access.DataAreas[0].Read}, decr - {access.DataAreas[0].Decrement}, incr - {access.DataAreas[0].Increment}";
                                    AddText(accStr);
                                    accStr =
                                        $"Access d1: read - {access.DataAreas[1].Read}, write - {access.DataAreas[1].Read}, decr - {access.DataAreas[1].Decrement}, incr - {access.DataAreas[1].Increment}";
                                    AddText(accStr);
                                    accStr =
                                        $"Access d2: read - {access.DataAreas[2].Read}, write - {access.DataAreas[2].Read}, decr - {access.DataAreas[2].Decrement}, incr - {access.DataAreas[2].Increment}";
                                    AddText(accStr);
                                    accStr =
                                        $"Trailer : Acc read - {access.Trailer.AccessBitsRead}, Acc write - {access.Trailer.AccessBitsWrite}, A read - {access.Trailer.KeyARead}, A write - {access.Trailer.KeyAWrite}, B read - {access.Trailer.KeyBRead}, B write - {access.Trailer.KeyBWrite}";
                                    AddText(accStr);
                                }
                            }
                            catch (Exception ex)
                            {
                                AddText("Failed to load sector: " + sector + "\r\nEx: " + ex.ToString());
                            }
                        }
                    }
                    AddText("=========================================================");
                }
            }
            catch (Exception ex)
            {
                AddText("reRead HandleCard Exception: " + ex.Message);
            }
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
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

                if (cardIdentification.PcscDeviceClass == DeviceClass.StorageClass &&
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
                            Key = _keyMultiKeyB
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

                        sec.Access.Trailer.AccessBitsRead = TrailerAccessCondition.ConditionEnum.KeyB;
                        sec.Access.Trailer.AccessBitsWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                        sec.Access.Trailer.KeyARead = TrailerAccessCondition.ConditionEnum.KeyB;
                        sec.Access.Trailer.KeyAWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                        sec.Access.Trailer.KeyBRead = TrailerAccessCondition.ConditionEnum.KeyB;
                        sec.Access.Trailer.KeyBWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                        sec.KeyA = _keyMultiKeyA.ByteArrayToString();
                        sec.KeyB = _keyMultiKeyB.ByteArrayToString();

                        await sec.Flush();

                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyB,
                            Sector = sector,
                            Key = _keyMultiKeyB
                        });

                        // During init, use the master key for both. This will be changed to the user pin-derived key
                        await sec.FlushTrailer(_keyMultiKeyA.ByteArrayToString(), _keyMultiKeyB.ByteArrayToString());

                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyA,
                            Sector = sector,
                            Key = _keyMultiKeyA
                        });
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyB,
                            Sector = sector,
                            Key = _keyMultiKeyB
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

                if (cardIdentification.PcscDeviceClass == DeviceClass.StorageClass &&
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
                            Key = _keyMultiKeyB
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

                        sec.KeyA = _keyMultiKeyA.ByteArrayToString();
                        sec.KeyB = _keyMultiKeyB.ByteArrayToString();

                        await sec.Flush();

                        // During init, use the master key for both. This will be changed to the user pin-derived key
                        await sec.FlushTrailer(_keyDefault.ByteArrayToString(), _keyMultiKeyB.ByteArrayToString());

                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyA,
                            Sector = sector,
                            Key = _keyMultiKeyA
                        });
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyB,
                            Sector = sector,
                            Key = _keyMultiKeyB
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

                if (cardIdentification.PcscDeviceClass == DeviceClass.StorageClass &&
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
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyB,
                            Sector = sector,
                            Key = _keyDefault
                        });

                        var sec = localCard.GetSector(sector);
                        await
                            sec.SetData(
                                new byte[] {0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x05 /*, 0x06/*, 0x07, 0x00, 0x00*/}, 0);
                        await sec.Flush();
                    }
                    else
                    {
                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                        {
                            KeyType = KeyType.KeyB,
                            Sector = sector,
                            Key = _keyMultiKeyB
                        });

                        var sec = localCard.GetSector(sector);
                        await
                            sec.SetData(
                                new byte[] {0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x05 /*, 0x06/*, 0x07, 0x00, 0x00*/}, 0);
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

        private void useAPI_Click(object sender, EventArgs e)
        {
            ServioCardsShell.ServioCardInfo res;
            try
            {
                res = ServioCardsShell.Authorize();
            }
            catch (Exception ex)
            {
                AddText(ex.Message);
                return;
            }
            if (res.ErrorCore == (int) ErrorCodes.ESuccess)
                AddText(res.ToString());
            else
                AddText($"Ошибка карты:\r\n{res.ErrorCore} {res.ErrorDescription}");
        }
        private void readAPI_Click(object sender, EventArgs e)
        {
            byte[] res;
            try
            {
                res = ServioCardsShell.GetCard();
            }
            catch (Exception ex)
            {
                AddText(ex.Message);
                return;
            }


            for (int i = 0; i < res?.Length / 16; ++i)
            {
                if (i > 63)
                    break;

                int size = Math.Min(res.Length - i * 16, 16);
                byte[] block = new byte[size];

                Array.Copy(res, i * 16, block, 0, size);
                AddText($"read block [{i}]:\t{BitConverter.ToString(block)}");
            }
        }
        private void infoAPI_Click(object sender, EventArgs e)
        {
            ServioCardsShell.ServioCardInfo res;
            try
            {
                res = ServioCardsShell.GetCardInfo();
            }
            catch (Exception ex)
            {
                AddText(ex.Message);
                return;
            }

            if (res.ErrorCore == (int)ErrorCodes.ESuccess)
                AddText("Образ чека:\r\n" + res.CardInfo);
            else
                AddText($"Ошибка карты:\r\n{res.ErrorCore} {res.ErrorDescription}");
        }
        private void sale_return_API_Click(object sender, EventArgs e)
        {
            ServioCardsShell.ServioCardInfo res;
            try
            {
                var start = DateTime.Now;
                var item1 = new ServioCardsShell.CardOperationItem();
                item1.GoodKind = (int)GoodKind.GkFuel;
                item1.GoodCode = "6"; // Код для устройств
                item1.GoodName = "АИ-95"; // название
                item1.FuellingPoint = 1; // ТРК №1
                item1.OrderUnit = (int)OrderUnit.UnitQuty;
                item1.Price = 10; // Цена
                item1.Quantity = 2; // Количество
                res = ServioCardsShell.CardOperationExecute(CardOperationType.Sale, 1, 121,  item1);
                AddText((DateTime.Now - start).ToString());
                item1.GoodKind = (int)GoodKind.GkService;
                item1.GoodCode = "1"; // Код для устройств
                item1.GoodName = "Стрижка"; // название
                item1.FuellingPoint = 1; // ТРК №1
                item1.OrderUnit = (int)OrderUnit.UnitMoney;
                item1.Price = 10; // Цена
                item1.Quantity = 2; // Количество                res += ServioCardsShell.CardOperationExecute(CardOperationType.Sale, 3, 122, item1);
                res += ServioCardsShell.CardOperationExecute(CardOperationType.Sale, 1, 123, item1);
                AddText((DateTime.Now - start).ToString());
                item1.Quantity = 1; // Количество
                res += ServioCardsShell.CardOperationExecute(CardOperationType.Return, 1, 124, item1);
                AddText((DateTime.Now - start).ToString());
                res += ServioCardsShell.CardOperationExecute(CardOperationType.Return, 1, 124, item1);
                AddText((DateTime.Now - start).ToString());
                item1.Price = 10; // Цена
                item1.Quantity = 1; // Количество
                //res += ServioCardsShell.CardOperationExecute(CardOperationType.Return, 1, 122, item1);
            }
            catch (Exception ex)
            {
                AddText(ex.Message);
                return;
            }

            if (res.ErrorCore == (int)ErrorCodes.ESuccess)
                AddText("Образ чека:\r\n" + res.CardInfo);
            else
                AddText($"Ошибка карты:\r\n{res.ErrorCore} {res.ErrorDescription}");
        }
        private void credit_API_Click(object sender, EventArgs e)
        {
            ServioCardsShell.ServioCardInfo res;
            try
            {
                var item1 = new ServioCardsShell.CardOperationItem();
                item1.GoodKind = (int)GoodKind.GkFuel;
                item1.GoodCode = "1"; // Код для устройств
                item1.GoodName = "АИ-95"; // название
                item1.FuellingPoint = 2; // ТРК №1
                item1.OrderUnit = (int)OrderUnit.UnitQuty;
                item1.Price = 100; // Цена
                item1.Quantity = 40; // Количество
                res = ServioCardsShell.CardOperationExecute(CardOperationType.Credit, 1, 121, item1);
                item1.GoodCode = "2"; // Код для устройств
                res += ServioCardsShell.CardOperationExecute(CardOperationType.Credit, 2, 122, item1);
                item1.GoodCode = "3"; // Код для устройств
                res += ServioCardsShell.CardOperationExecute(CardOperationType.Credit, 1, 123, item1);
                item1.GoodCode = "4"; // Код для устройств
                res += ServioCardsShell.CardOperationExecute(CardOperationType.Credit, 2, 124, item1);
                item1.GoodCode = "5"; // Код для устройств
                res += ServioCardsShell.CardOperationExecute(CardOperationType.Credit, 1, 125, item1);
                item1.GoodCode = "6"; // Код для устройств
                res += ServioCardsShell.CardOperationExecute(CardOperationType.Credit, 2, 126, item1);
            }
            catch (Exception ex)
            {
                AddText(ex.Message);
                return;
            }

            if (res.ErrorCore == (int)ErrorCodes.ESuccess)
                AddText("Образ чека:\r\n" + res.CardInfo);
            else
                AddText($"Ошибка карты:\r\n{res.ErrorCore} {res.ErrorDescription}");
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

                if (cardIdentification.PcscDeviceClass == DeviceClass.StorageClass &&
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
            if (localCard == null || card == null)
                return;

            WriteCard(EmptyCard);
        }
        private void button_writeTest_Click(object sender, EventArgs e)
        {
            if (localCard == null || card == null)
                return;

            WriteCard(TestCard);
        }

        private async Task<int> WriteCard(string[,] src_card)
        {
            if (src_card == null)
                return -1;

            try
            {
                var cardIdentification = await localCard.GetCardInfo();

                AddText("Connected to card\r\nPC/SC device class: " + cardIdentification.PcscDeviceClass.ToString() +
                        "\r\nCard name: " + cardIdentification.PcscCardName.ToString());

                if (cardIdentification.PcscDeviceClass == DeviceClass.StorageClass &&
                    (cardIdentification.PcscCardName == CardName.MifareStandard1K ||
                     cardIdentification.PcscCardName == CardName.MifareStandard4K))
                {
                    // Handle MIFARE Standard/Classic
                    AddText("MIFARE Standard/Classic card detected");

                    var uid = await localCard.GetUid();
                    AddText("UID:  " + BitConverter.ToString(uid));

                    AddText("WRITE ALL CARD");

                    bool card1Key = uid[0] == 0xBC;

                    // 16 sectors, print out each one
                    for (var sector = 0; sector < src_card.GetLength(0); sector++)
                    {
                        AddText("=========================================================");
                        for (var block = 0; block < src_card.GetLength(1); block++)
                        {
                            if (sector == 0 && block == 0)
                                continue;

                            try
                            {
                                if (card1Key)
                                {
                                    if (sector == 1)
                                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                                        {
                                            KeyType = KeyType.KeyB,
                                            Sector = sector,
                                            Key = _keyNull
                                        });
                                }
                                else
                                {
                                    //if (sector == 0)
                                    //    localCard.AddOrUpdateSectorKeySet(new SectorKeySet { KeyType = KeyType.KeyB, Sector = sector, Key = _keyMultiKeyB });
                                    //else
                                    localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                                    {
                                        KeyType = KeyType.KeyB,
                                        Sector = sector,
                                        Key = _keyMultiKeyB
                                    });
                                }

                                var dataSrc = src_card[sector, block].StringToByteArray();

                                //TODO убрать
                                if (_cardBadSectors?.Contains(new Tuple<int, int>(sector, block)) ?? false)
                                {
                                    WriteOrReplaceToFileSector(sector, block, dataSrc);
                                }
                                else
                                {
                                    var sec = localCard.GetSector(sector);
                                    if (block == sec.NumDataBlocks - 1)
                                    {
                                        var access = AccessBits.GetAccessConditions(dataSrc);
                                        sec.Access.DataAreas[0] = access.DataAreas[0];
                                        sec.Access.DataAreas[1] = access.DataAreas[1];
                                        sec.Access.DataAreas[2] = access.DataAreas[2];
                                        sec.Access.Trailer = access.Trailer;
                                        byte[] keyAData = new byte[6];
                                        byte[] keyBData = new byte[6];
                                        Array.Copy(dataSrc, 0, keyAData, 0, 6);
                                        Array.Copy(dataSrc, 10, keyBData, 0, 6);
                                        sec.KeyA = keyAData.ByteArrayToString();
                                        sec.KeyB = keyBData.ByteArrayToString();
                                    }
                                    else
                                        await sec.SetData(dataSrc, block);

                                    await sec.Flush();
                                    if (block == sec.NumDataBlocks - 1)
                                    {
                                        localCard.AddOrUpdateSectorKeySet(new SectorKeySet
                                        {
                                            KeyType = KeyType.KeyB,
                                            Sector = sector,
                                            Key = _keyMultiKeyB
                                        });
                                        await
                                            sec.FlushTrailer(_keyMultiKeyA.ByteArrayToString(),
                                                _keyMultiKeyB.ByteArrayToString());
                                    }
                                }
                                string hexString = "|| ";
                                for (int i = 0; i < dataSrc.Length; i++)
                                {
                                    if (i == 6 || i == 10)
                                        hexString += " || ";
                                    hexString += dataSrc[i].ToString("X2") + " ";
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
                return 0;
            }
            catch (Exception ex)
            {
                AddText("writeFromB HandleCard Exception: " + ex.ToString());
                return -1;
            }
        }

        private void WriteOrReplaceToFileSector(int sector, int block, byte[] data)
        {
            //bool logIsError = text.Contains("ERROR!!!");
            //bool writeToLog = logIsError;
            //#if DEBUG
            //        writeToLog = true;
            //#endif

            //if (!writeToLog)
            //    return;
            var uid = localCard?.GetUid().Result;
            if (uid == null)
                throw new Exception("Попытка записи данных в неинициализированную карты");

            string path = @"Out\file_sector_" + uid.ByteArrayToString() + ".dat";
            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (var f = File.CreateText(path))
                {
                }
            }
            var fileList = File.ReadAllLines(path).ToList();
            var ind = fileList.FindIndex(s => s.Contains($"{sector},{block}:"));
            if (ind >= 0)
            {
                fileList[ind] = $"{sector},{block}:{data?.ByteArrayToString()}";
                File.WriteAllLines(path, fileList);
            }
            else
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.Write($"{sector},{block}:{data?.ByteArrayToString()}{Environment.NewLine}");
                }
            //        if (showMsg)
            //MessageBox.Show(data);
        }
        private byte[] ReadFromFileSector(int sector, int block)
        {
            //bool logIsError = text.Contains("ERROR!!!");
            //bool writeToLog = logIsError;
            //#if DEBUG
            //        writeToLog = true;
            //#endif

            //if (!writeToLog)
            //    return;

            var uid = localCard?.GetUid().Result;
            if (uid == null)
                throw new Exception("Попытка чтения данных с неинициализированной карты");

            string path = @"Out\file_sector_" + uid.ByteArrayToString() + ".dat";
            var sectors = new Dictionary<Tuple<int, int>, byte[]>();
            if (File.Exists(path))
            {
                var fileLise = File.ReadAllLines(path).ToList();
                fileLise.ForEach(s =>
                {
                    var arr = s.Split(':', ',');
                    if (arr.Length == 3)
                    {
                        int sec = Convert.ToInt32(arr[0]);
                        int bl = Convert.ToInt32(arr[1]);
                        var dat = arr[2].StringToByteArray();
                        sectors[new Tuple<int, int>(sec, bl)] = dat;
                    }
                });
            }
            //        if (showMsg)
            //MessageBox.Show(data);

            byte[] data;
            if (sectors.TryGetValue(new Tuple<int, int>(sector, block), out data))
                return data;

            return null;
        }
        private HashSet<Tuple<int, int>> ReadBadsFromFileSector()
        {
            //bool logIsError = text.Contains("ERROR!!!");
            //bool writeToLog = logIsError;
            //#if DEBUG
            //        writeToLog = true;
            //#endif

            //if (!writeToLog)
            //    return;

            var uid = localCard?.GetUid().Result;
            if (uid == null)
                throw new Exception("Попытка чтения данных с неинициализированной карты");

            string path = @"Out\file_sector_" + uid.ByteArrayToString() + ".dat";

            var result = new HashSet<Tuple<int, int>>();

            if (File.Exists(path))
            {
                using (var file = new StreamReader(path))
                {
                    var header = file.ReadLine();
                    if (!string.IsNullOrWhiteSpace(header))
                    {
                        var bads = header.Split(';');
                        foreach (var bad in bads)
                        {
                            var pair = bad.Split(',');
                            if (pair.Length == 2)
                                result.Add(
                                    new Tuple<int, int>(
                                        Convert.ToInt32(pair[0]),
                                        Convert.ToInt32(pair[1])));
                        }
                    }
                }
            }
            //        if (showMsg)
            //MessageBox.Show(data);

            return result;
        }


    }
}
