﻿//using ProjectSummer.Repository;
using System;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using Binding = System.ServiceModel.Channels.Binding;

namespace ServioBonus
{
    [ServiceContract()]
    public interface IServioOnlineAPI
    {
        [OperationContract]
        int GetCardType(int IssuerId);
        [OperationContract]
        string GetCardInfo(int IssuerId, string CardNumber, out int CardType);
        [OperationContract]
        int Debit(long TransID, int IssuerId, string CardNumber, int FuelCode, ref decimal Amount, ref decimal Price, ref decimal Quantity, ref string Slip, ref string Error);
        [OperationContract]
        int Commit(long TransID, int IssuerId, string CardNumber, int FuelCode, ref decimal Amount, ref decimal Price, ref decimal Quantity, ref string Slip, ref string Error, bool FullFilling = false);
        [OperationContract]
        decimal GetBonusBalance(int IssuerId, string CardNumber, string Product, int Price);
        [OperationContract]
        int BonusCancel(long TransID, int IssuerId, string CardNumber, ref string Error);
        [OperationContract]
        int BonusDebit(long TransID, int IssuerId, string CardNumber, int FuelCode, decimal Amount, decimal Price, decimal Quantity, decimal Bonus, ref string Error);
        [OperationContract]
        int BonusCommit(long TransID, int IssuerId, string CardNumber, int FuelCode, decimal Amount, decimal Price, decimal Quantity, decimal Bonus, ref string Error);
    }
    public class IServioOnlineAPIClient : ClientBase<IServioOnlineAPI>
    {
        public IServioOnlineAPIClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) { }
    }
    public unsafe class ServioCardsShell
    {
        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int Init([MarshalAs(UnmanagedType.LPWStr)]string ParamsFile, ref IntPtr ObjRef);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int Deinit(ref IntPtr ObjRef);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern IntPtr CardOperation_Create();

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int ReadCard(IntPtr ObjRef, IntPtr SerialNumber, IntPtr CardImage, bool CardInfoOnly);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int Auth(IntPtr ObjRef, IntPtr SerialNumber, IntPtr CardImage, IntPtr AuthOp);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int CardInfo(IntPtr ObjRef, IntPtr SerialNumber, IntPtr CardImage, ref IntPtr AuthOp);
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TCardOperation
        {
            /// <summary>
            /// Размер структуры
            /// </summary>
            public int Size;
            /// <summary>
            /// Номер АЗС
            /// </summary>
            public int POS;

            private IntPtr pOSName;
            /// <summary>
            /// Название АЗС
            /// </summary>
            public string POSName
            {
                get { if (pOSName == IntPtr.Zero) return null; else return Marshal.PtrToStringBSTR(pOSName); }
                set { if (value == null) pOSName = IntPtr.Zero; else pOSName = Marshal.StringToBSTR(value); }
            }
            /// <summary>
            /// OLE Дата/Время проведения транзакции. 0-не заполнено
            /// </summary>
            public double OpTime;
            /// <summary>
            /// Код транзакции POS
            /// </summary>
            public long TransactID;
            /// <summary>
            /// Код операции: 0-продажа, 1-возврат
            /// </summary>
            public byte OpCode;
            /// <summary>
            ///  Операция по терминалу проводится уже после налива. Это накладывает ограничение на применение скидок ("пересчет денежного заказа, например")
            /// </summary>
            public byte IsPostpay;
            /// <summary>
            /// Тип карты 0-платежные карты, 1-карты лояльности
            /// </summary>
            public int CardType;
            /// <summary>
            /// Серийный (аппаратный) номер карты
            /// </summary>
            public SERIALNUMBER_T SerialNumber;
            /// <summary>
            /// Код эмитента
            /// </summary>
            public int IssuerID;

            private IntPtr cardNumber;
            /// <summary>
            /// Код карты VARCHAR(50)
            /// </summary>
            public string CardNumber
            {
                get { if (cardNumber == IntPtr.Zero) return null; else return Marshal.PtrToStringBSTR(cardNumber); }
                set { if (value == null) cardNumber = IntPtr.Zero; else cardNumber = Marshal.StringToBSTR(value); }
            }
            /// <summary>
            /// Клиент
            /// </summary>
            public int HolderID;
            /// <summary>
            /// Добавлять ведущие нули к карте
            /// </summary>
            public byte AddPrefixZeros;
            /// <summary>
            /// Операция проводится без карты
            /// </summary>
            public byte WoCard;
            /// <summary>
            /// ПИН-код проверен
            /// </summary>
            public byte PINChecked;
            /// <summary>
            /// Используется мультиэмитентная карта
            /// </summary>
            public byte UseMultiIssuer;
            /// <summary>
            /// Кол-во позиций
            /// </summary>
            public int ItemCount;
            /// <summary>
            /// Позиции операции
            /// </summary>
            public IntPtr Items;
            public IntPtr checkImage;
            /// <summary>
            /// Образ квитанции VARCHAR(MAX_CHECK_IMAGE_LEN)
            /// </summary>
            public string CheckImage
            {
                get { if (checkImage == IntPtr.Zero) return null; else return Marshal.PtrToStringBSTR(checkImage); }
                set { if (value == null) checkImage = IntPtr.Zero; else checkImage = Marshal.StringToBSTR(value); }
            }
            /// <summary>
            /// Причина отказа по операции
            /// </summary>
            public int DenyReason;
            /// <summary>
            /// Образ карты был изменен в процессе операции. Флаг используется внутри.
            /// </summary>
            public byte CardImageChanged;
            /// <summary>
            ///  Использовать серийный номер в качестве номера карты
            /// </summary>
            public byte SerialNumberAsIdentifier;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        private struct SERIALNUMBER_T
        {
            [FieldOffset(0)]
            public uint FourByteUID;
            [FieldOffset(0)]
            public fixed byte SevenByteUID[7];

            public Tuple <string, string> getSerial()
            {
                var res1 = BitConverter.GetBytes(FourByteUID);
                var res2 = new byte[7];

                fixed (byte* p = SevenByteUID)
                {
                    for (int i = 0; i < 7; ++i)
                    {
                        res2[i] = *(p + i);
                    }
                }
                return new Tuple<string, string>(BitConverter.ToString(res1), BitConverter.ToString(res2));
            }
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        private struct MF1S70_T
        {
            public const int size = 4096;

            [FieldOffset(0)]
            public fixed byte block[size];

            public byte[] getBlocks()
            {
                var res = new byte[size];
                fixed (byte* p = this.block)
                {
                    for (int i = 0; i < size; ++i)
                    {
                        res[i] = *(p + i);
                    }
                }
                return res;
            }

            public void clearBlocks()
            {
                fixed (byte* p = this.block)
                {
                    for (int i = 0; i < size; ++i)
                    {
                        *(p + i) = 0xA0;
                    }
                }
            }
        }

        public struct ServioCardInfo
        {
            public int ErrorCore;
            public string CardNumber;
            public int IssuerID;

            public override string ToString()
            {
                return (ErrorCore != 0) ? $"Ошибка чтения карты: {ErrorCore}" : $"Эмитент: {IssuerID}\r\nНомер карты: {CardNumber}";
            }
        }

        private static string config = @"C:\ServioCardAPI\SDK\Mifaread3.ini";

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int SetCallback(IntPtr Obj, IntPtr CallBack, IntPtr UserData);
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TCustomInteraction
        {
            public int Size;            // Размер структуры
            public int CIType;          // Тип инетрфейса
            public int ReturnCode;      // Результат выполнения: 0-ок, 1-ошибка, 2-отмена
            public IntPtr Msg;             // Сообщение
            public IntPtr Title;           // Заголовок окна
            public int Flags;           // Флаги MessageBox / InputBox и т.д.
            public uint Timeout;         // Таймаут показа диалогового окна
            public int ProgressTotal;   // Макс.значение ProgressBar. Если ProgressTotal <= 0 То нужно просто показать процесс длительной операции, иначе можно ProgressBar.
            public int ProgressCurrent; // Текущее значение ProgressBar. Если ProgressCurrent >= ProgressTotal то ProgressBar можно спрятать (операция завершена) иначе происходит выполнение операции.
            public IntPtr Buffer;          // Строка с резултатом ввода (InputBox)
            public int BufLen;          // Макс.длина буфера
            public int ItemCount;       // Кол-во позиций ListBox
            public IntPtr Items;         // Позиции ListBox
            public int ItemIndex;       // Индекс выбранной позиции
        }
        private static string pin = "";
        private static CustomInteractionCallbackFuncDelegate callback = new CustomInteractionCallbackFuncDelegate(CustomInteractionCallbackFunc);
        private delegate int CustomInteractionCallbackFuncDelegate(IntPtr CI, IntPtr UserData);
        private static int CustomInteractionCallbackFunc(IntPtr CI, IntPtr UserData)
        {

            TCustomInteraction str = (TCustomInteraction)Marshal.PtrToStructure(CI, typeof(TCustomInteraction));

            if (str.CIType == 103)
            {
                System.Windows.Forms.MessageBox.Show($"str.CIType = {str.CIType}, pin = {pin}, str.BufLen = {str.BufLen}");
                var data = Encoding.Unicode.GetBytes(pin + "\0");
                for (int z = 0; z < data.Length && z < str.BufLen; z++)
                    Marshal.WriteByte(str.Buffer, z, data[z]);
                //str.Buffer = Marshal.StringToHGlobalUni(pin);
                //str.BufLen = pin.Length;//data.Length;
                str.ReturnCode = 1;
            }
            Marshal.StructureToPtr(str, CI, true);

            return 0;
        }

        public static ServioCardInfo ReadCardNumber(string PIN)
        {

            ServioCardInfo res = new ServioCardInfo() { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            try
            {
                pin = PIN;
                IntPtr obj = new IntPtr();

                if ((res.ErrorCore = Init(config, ref obj)) != 0)
                    return res;
                SetCallback(obj, Marshal.GetFunctionPointerForDelegate(callback), IntPtr.Zero);
                TCardOperation AuthOp = new TCardOperation();
                SERIALNUMBER_T SerialNumber = new SERIALNUMBER_T();
                IntPtr SerialNumberPtr = Marshal.AllocHGlobal(sizeof(SERIALNUMBER_T));
                IntPtr CardImagePtr = Marshal.AllocHGlobal(sizeof(MF1S70_T));
                IntPtr _authOp = CardOperation_Create();
                AuthOp = (TCardOperation)Marshal.PtrToStructure(_authOp, typeof(TCardOperation));

                if ((res.ErrorCore = ReadCard(obj, SerialNumberPtr, CardImagePtr, true)) != 0)
                    return res;

                AuthOp.POSName = "TestTestTest";
                AuthOp.Size = sizeof(TCardOperation);
                AuthOp.POS = 1;
                AuthOp.OpTime = DateTime.Now.ToOADate();
                AuthOp.IsPostpay = 0;
                AuthOp.CardType = 2;
                AuthOp.SerialNumber = SerialNumber;
                AuthOp.CardNumber = null;
                AuthOp.IssuerID = -1;
                AuthOp.AddPrefixZeros = 0;
                AuthOp.WoCard = 0;
                AuthOp.PINChecked = (byte)((PIN == "") ? 1 : 0);
                AuthOp.ItemCount = 0;
                Marshal.StructureToPtr(AuthOp, _authOp, true);



                AuthOp = (TCardOperation)Marshal.PtrToStructure(_authOp, typeof(TCardOperation));

                if ((res.ErrorCore = Auth(obj, SerialNumberPtr, CardImagePtr, _authOp)) != 0)
                    return res;
                AuthOp = (TCardOperation)Marshal.PtrToStructure(_authOp, typeof(TCardOperation));
                res.IssuerID = AuthOp.IssuerID;
                res.CardNumber = AuthOp.CardNumber.PadLeft(20, '0');
                ///System.Windows.Forms.MessageBox.Show(AuthOp.PINChecked.ToString());

            }
            finally
            {
                pin = "";
            }
            return res;
        }

        public static ServioCardInfo ReadCardNumber()
        {
            ServioCardInfo res = new ServioCardInfo() { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            IntPtr obj = new IntPtr();

            if ((res.ErrorCore = Init(config, ref obj)) != 0)
                return res;

            IntPtr SerialNumberPtr = Marshal.AllocHGlobal(sizeof(SERIALNUMBER_T));
            IntPtr CardImagePtr = Marshal.AllocHGlobal(sizeof(MF1S70_T));
            IntPtr OperationPtr = CardOperation_Create();
            try
            {
                TCardOperation Operation = MarshalHelper.UnMemory<TCardOperation>.ReadInMem(OperationPtr);

                if ((res.ErrorCore = ReadCard(obj, SerialNumberPtr, CardImagePtr, true)) != 0)
                    return res;

                //Operation = MarshalHelper.UnMemory<TCardOperation>.ReadInMem(OperationPtr);
                //var op = Operation.CheckImage;
                //SERIALNUMBER_T SerialNumber = MarshalHelper.UnMemory<SERIALNUMBER_T>.ReadInMem(SerialNumberPtr);
                //var ser = SerialNumber.getSerial();

                //Operation.POSName = "TestTestTest";
                //Operation.Size = sizeof(TCardOperation);
                //Operation.POS = 1;
                //Operation.OpTime = DateTime.Now.ToOADate();
                //Operation.IsPostpay = 0;
                //Operation.CardType = 2;
                //Operation.SerialNumber = SerialNumber;
                //Operation.CardNumber = null;
                //Operation.IssuerID = -1;
                //Operation.AddPrefixZeros = 0;
                //Operation.WoCard = 0;
                //Operation.PINChecked = 1;
                //Operation.ItemCount = 0;
                //MarshalHelper.UnMemory<TCardOperation>.SaveInMem(Operation, ref OperationPtr);

                if ((res.ErrorCore = Auth(obj, SerialNumberPtr, CardImagePtr, OperationPtr)) != 0)
                    return res;

                Operation = MarshalHelper.UnMemory<TCardOperation>.ReadInMem(OperationPtr);
                //op = Operation.CheckImage;
                //SerialNumber = MarshalHelper.UnMemory<SERIALNUMBER_T>.ReadInMem(SerialNumberPtr);
                //ser = SerialNumber.getSerial();

                //Operation = (TCardOperation)Marshal.PtrToStructure(OperationPtr, typeof(TCardOperation));
                res.IssuerID = Operation.IssuerID;
                res.CardNumber = Operation.CardNumber.PadLeft(20, '0');
            }
            catch (Exception ex)
            {
            }
            finally
            {
                MarshalHelper.UnMemory.FreeIntPtr(SerialNumberPtr);
                MarshalHelper.UnMemory.FreeIntPtr(CardImagePtr);
                //MarshalHelper.UnMemory.FreeIntPtr(OperationPtr);
            }
            return res;
        }

        public static byte[] GetCard()
        {
            //ClientLibrary = C:\ServioCardAPI\gds32.dll
            //DBName = C:\ServioCardAPI\SPFrontData.fdb
            //UserName = SYSDBA
            //Password = masterkey
            //RoleName =
            //Charset = UTF8


            // Общий принцип операций с картой такой что сначала образ карты считывается
            // в память, затем ПО его некоторым образом модифицирует
            // (производит уменьшение или увеличение счетков, ставит даты пополнений,
            // включает лимиты и т.п.). Модификация образа происходит одной
            // операцией и в одной транзакции. Затем образ карты необходимо записать
            // обратно, причем сделать это нужно как можно скорее потому что после
            // проведения операции с картой в базе данных остались отметки об
            // обслуживании. Если карту не записать обратно то будет рассогласование
            // базы и карты. Такие случаи в нормальной работе недопустимы. Поэтому
            // между операциями CardInfo/Sale/Return/Credit и операцией записи не должно
            // быть модальных окон и других задерживающих операций.
            /*
                        int ErrorCode;
                        SERIALNUMBER_T* SerialNumber;
                        MF1S70_T* CardImage;
                        TCardOperation* CardInfo;

                        SerialNumber = (SERIALNUMBER_T*)malloc(sizeof(SERIALNUMBER_T));
                        CardImage = (MF1S70_T*)malloc(sizeof(MF1S70_T));
                        CardInfo = Link.CardOperation_Create();
                        try
                        {

                            // Читаем карту
                            ErrorCode = Link.ReadCard(Obj, SerialNumber, CardImage, false);
                            if (ErrorCode != E_SUCCESS)
                            {
                                if (ErrorCode == E_CANCEL)
                                {
                                    return;
                                }
                                else
                                {
                                    ShowMessage("Error: " + IntToStr(ErrorCode));
                                    return;
                                }
                            }

                            CardInfo->Size = sizeof(TCardOperation);
                            CardInfo->POS = 1; // Номер АЗС или терминала
                            CardInfo->OpTime = Now(); // Реальное время с вашего терминала
                            CardInfo->TransactID = 0; // Тут желательно ваш код код транзакции указать
                            CardInfo->CardType = -1; // Карта-идентификатор
                            CardInfo->SerialNumber = *SerialNumber; // Указать серийный номер, считанный с метки
                            CardInfo->IssuerID = -1; // Указать -1, программа сама переопределит. Если указать другой код - программа будет использовать его
                            CardInfo->CardNumber = (BSTR)U""; // Не указывать !!!
                            CardInfo->AddPrefixZeros = false; // Нужна доп. настройка. Аналогичная нашей опции в обработчиках
                            CardInfo->ItemCount = 0; // При аутентификации позиции не заполнять.

                            // Смотрим информацию по карте
                            ErrorCode = Link.CardInfo(Obj, SerialNumber, CardImage, CardInfo);

                            // флаг указывает на то что образ карты был изменен в процессе операции
                            // и нужно записать его обратно.
                            // Нужно для пополнения карты при просмотре информации
                            if (CardInfo->CardImageChanged)
                            {
                                // при ошибке можно попробовать повторить операцию несколько раз 3-10...
                                // если не проходит то это патовая ситуация и для её дальнейшего
                                // разрешения нужно сохранить SerialNumber, CardImage и CardInfo
                                // В теории, сохранённый SerialNumber и CardImage можно попробовать
                                // записать обратно даже при перезапуске ПО, но до следующего
                                // обслуживания по карте. Чтобы решить ситуацию можно сохранить CardInfo
                                // чтобы в офисе уменьшили или увеличили баланс карты на соотв. значение.
                                ErrorCode = Link.WriteCard(Obj, SerialNumber, CardImage);
                            }

                            // Образ квитанции
                            UnicodeString s;
                            s = CardInfo->CheckImage;
                            if (s != "")
                            {
                                edBillImage->Text = CardInfo->CheckImage;
                            }

                            if (ErrorCode != E_SUCCESS)
                            {

                                if (ErrorCode == E_CANCEL)
                                {
                                    return;
                                }
                                else
                                {
                                    ShowMessage("Error: " + IntToStr(ErrorCode));
                                    return;
                                }
                            }

                        } __finally {
                            Link.CardOperation_Free(CardInfo);
                            memset(SerialNumber, 0, sizeof(SERIALNUMBER_T));
                            free(SerialNumber);
                            // Память можно очистить так
                            memset(CardImage, 0, sizeof(MF1S70_T));
                            free(CardImage);
                        }
            */

            /*
                        ServioCardInfo err = new ServioCardInfo() { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
                        IntPtr obj = new IntPtr();

                        if ((err.ErrorCore = Init(config, ref obj)) != 0)
                            return res;

                        //TCardOperation AuthOp = new TCardOperation();
                        //SERIALNUMBER_T SerialNumber = new SERIALNUMBER_T();
                        IntPtr SerialNumberPtr = Marshal.AllocHGlobal(sizeof(SERIALNUMBER_T));
                        IntPtr CardImagePtr = Marshal.AllocHGlobal(sizeof(MF1S70_T));
                        IntPtr OperationPtr = CardOperation_Create();
                        TCardOperation Operation = MarshalHelper.UnMemory<TCardOperation>.ReadInMem(OperationPtr);
                        //(TCardOperation)Marshal.PtrToStructure(OperationPtr, typeof(TCardOperation));

                        if ((err.ErrorCore = ReadCard(obj, SerialNumberPtr, CardImagePtr, true)) != 0)
                            return res;
            */

            byte[] res = null;
            ServioCardInfo err = new ServioCardInfo() { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            IntPtr obj = new IntPtr();

            if ((err.ErrorCore = Init(config, ref obj)) != 0)
            {
                MessageBox.Show("Error init No" + err.ErrorCore);
                return res;
            }

            IntPtr SerialNumberPtr = Marshal.AllocHGlobal(sizeof(SERIALNUMBER_T));
            IntPtr CardImagePtr = Marshal.AllocHGlobal(sizeof(MF1S70_T));
            IntPtr OperationPtr = CardOperation_Create();

            if ((err.ErrorCore = ReadCard(obj, SerialNumberPtr, CardImagePtr, true)) != 0)
            {
                MessageBox.Show("Error read No" + err.ErrorCore);
                return res;
            }

            SERIALNUMBER_T SerialNumber = MarshalHelper.UnMemory<SERIALNUMBER_T>.ReadInMem(SerialNumberPtr);
            var ser = SerialNumber.getSerial();
/*
            TCardOperation Operation = MarshalHelper.UnMemory<TCardOperation>.ReadInMem(OperationPtr);
            Operation.POSName = "TestTestTest";
            Operation.Size = sizeof(TCardOperation);
            Operation.POS = 1;
            Operation.OpTime = DateTime.Now.ToOADate();
            Operation.IsPostpay = 0;
            Operation.TransactID = 0;
            Operation.CardType = -1;
            Operation.SerialNumber = SerialNumber;
            Operation.CardNumber = null;
            Operation.IssuerID = -1;
            Operation.AddPrefixZeros = 0;
            Operation.WoCard = 0;
            Operation.PINChecked = 1;
            Operation.ItemCount = 0;
//            Marshal.StructureToPtr(Operation, OperationPtr, true);
MarshalHelper.UnMemory<TCardOperation>.SaveInMem(Operation, ref OperationPtr);
//            Operation = (TCardOperation)Marshal.PtrToStructure(OperationPtr, typeof(TCardOperation));
*/
            //if ((err.ErrorCore = Auth(obj, SerialNumberPtr, CardImagePtr, OperationPtr)) != 0)
            //{
            //    MessageBox.Show("Error auth No" + err.ErrorCore);
            //    return res;
            //}

            MarshalHelper.UnMemory<byte>.SaveInMemArr(new byte[MF1S70_T.size], ref CardImagePtr);
            if ((err.ErrorCore = ReadCard(obj, SerialNumberPtr, CardImagePtr, false)) != 0)
                return res;
            var blocks = MarshalHelper.UnMemory<MF1S70_T>.ReadInMem(CardImagePtr);
            var card = blocks.getBlocks();
            SerialNumber = MarshalHelper.UnMemory<SERIALNUMBER_T>.ReadInMem(SerialNumberPtr);
            ser = SerialNumber.getSerial();
/*
            Operation.Size = sizeof(TCardOperation);
            Operation.POS = 1;
            Operation.OpTime = DateTime.Now.ToOADate();
            Operation.TransactID = 0;
            Operation.CardType = -1;
            Operation.SerialNumber = SerialNumber;
            Operation.IssuerID = -1;
            Operation.CardNumber = null;
            Operation.AddPrefixZeros = 0;
            Operation.ItemCount = 0;
            Marshal.StructureToPtr(Operation, OperationPtr, true);
*/
            if ((err.ErrorCore = CardInfo(obj, SerialNumberPtr, CardImagePtr, ref OperationPtr)) != 0)
            {
                MessageBox.Show("Error info No" + err.ErrorCore);
                return res;
            }

//            var str = Operation.CheckImage;

            return card;


        }


    }
}
