//using ProjectSummer.Repository;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using MarshalHelper;
using Binding = System.ServiceModel.Channels.Binding;

namespace ServioBonus
{
    public class CardShellException : Exception
    {
        public CardShellException(int code)
        {
            ErrorCode = code;
        }

        public CardShellException(string s, int code) : base(s)
        {
            ErrorCode = code;
        }

        public int ErrorCode { get { return HResult; } set { HResult = value; } }
    }

    [ServiceContract]
    public interface IServioOnlineAPI
    {
        [OperationContract]
        int GetCardType(int issuerId);
        [OperationContract]
        string GetCardInfo(int issuerId, string cardNumber, out int cardType);
        [OperationContract]
        int Debit(long transID, int issuerId, string cardNumber, int fuelCode, ref decimal amount, ref decimal price, ref decimal quantity, ref string slip, ref string error);
        [OperationContract]
        int Commit(long transID, int issuerId, string cardNumber, int fuelCode, ref decimal amount, ref decimal price, ref decimal quantity, ref string slip, ref string error, bool fullFilling = false);
        [OperationContract]
        decimal GetBonusBalance(int issuerId, string cardNumber, string product, int price);
        [OperationContract]
        int BonusCancel(long transID, int issuerId, string cardNumber, ref string error);
        [OperationContract]
        int BonusDebit(long transID, int issuerId, string cardNumber, int fuelCode, decimal amount, decimal price, decimal quantity, decimal bonus, ref string error);
        [OperationContract]
        int BonusCommit(long transID, int issuerId, string cardNumber, int fuelCode, decimal amount, decimal price, decimal quantity, decimal bonus, ref string error);
    }
    public class ServioOnlineAPIClient : ClientBase<IServioOnlineAPI>
    {
        public ServioOnlineAPIClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) { }
    }
    public unsafe class ServioCardsShell
    {
        private static int InitInternal(string paramsFile, ref IntPtr objRef)
        {
            var logDir = @"C:\ServioCardAPI\SDK\Out";
            //InitReader(@"FEIG ELECTRONIC GmbH ID CPR44.xx Slot:CL 481902244", logDir);
            InitReader(@"ACS ACR1281 1S Dual Reader PICC 0", logDir);
            return Init(paramsFile, ref objRef);
        }

        [DllImport(@"C:\ServioCardAPI\SDK\CustomMifare.dll")]
        public static extern string InitReader(string paramsFile, string logDir);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int Init([MarshalAs(UnmanagedType.LPWStr)]string paramsFile, ref IntPtr objRef);

        //[DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        //private static extern int Deinit(ref IntPtr objRef);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern IntPtr CardOperation_Create();

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern void CardOperation_Free(IntPtr operation);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int ReadCard(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage, bool cardInfoOnly);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int WriteCard(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int Auth(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage, IntPtr operation);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int CardInfo(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage, IntPtr operation);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int Sale(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage, IntPtr operation);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int Credit(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage, IntPtr operation);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int Return(IntPtr objRef, IntPtr serialNumber, IntPtr cardImage, IntPtr operation);

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int CustomStringLength(ref CustomString strRef);

        // Копирование строки в буфер
        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern IntPtr CustomStringGet(ref CustomString strRef, IntPtr buffer, int bufLen);

        // Установка нового значения строки. Если DataLen <= 0 то очистка.
        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern void CustomStringSet(ref CustomString strRef, IntPtr data, int dataLen);

        // Очистка строки
        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern void CustomStringClear(ref CustomString strRef);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct CustomString
        {
            public int length;
            public IntPtr str;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct CardOperation
        {
            /// <summary>
            /// Размер структуры
            /// </summary>
            public int Size;
            /// <summary>
            /// Номер АЗС
            /// </summary>
            public int POS;

            private CustomString posName;
            /// <summary>
            /// Название АЗС
            /// </summary>
            public string PosName
            {
                get { return CustomStringToUnicode(posName); }
                set { if (value == null){posName.str = IntPtr.Zero;posName.length = 0;}else{UnicodeStringToCustom(ref posName, value);}}
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
            public SerialnumberT SerialNumber;
            /// <summary>
            /// Код эмитента
            /// </summary>
            public int IssuerID;

            private CustomString cardNumber;
            /// <summary>
            /// Код карты VARCHAR(50)
            /// </summary>
            public string CardNumber
            {
                get { return CustomStringToUnicode(cardNumber); }
                set { if (value == null) { cardNumber.str = IntPtr.Zero; cardNumber.length = 0; } else { UnicodeStringToCustom(ref cardNumber, value); }}
            }
            /// <summary>
            /// Клиент
            /// </summary>
            private readonly int HolderID;
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
            private readonly byte UseMultiIssuer;
            /// <summary>
            /// Кол-во позиций
            /// </summary>
            public int ItemCount;
            /// <summary>
            /// Позиции операции
            /// </summary>
            public IntPtr Items;

            public List<CardOperationItem> ListItems
            {
                get
                {
                    if (Items != IntPtr.Zero)
                    {
                        IntPtr pItemInit = UnMemory<IntPtr>.ReadInMem(Items);
                        CardOperationItem item = UnMemory<CardOperationItem>.ReadInMem(pItemInit);
                        return new List<CardOperationItem> { item };
                    }
                    return new List<CardOperationItem>();
                }
            }

            private readonly CustomString checkImage;
            /// <summary>
            /// Образ квитанции VARCHAR(MAX_CHECK_IMAGE_LEN)
            /// </summary>
            public string CheckImage => CustomStringToUnicode(checkImage);

            /// <summary>
            /// Причина отказа по операции
            /// </summary>
            private readonly int DenyReason;
            /// <summary>
            /// Образ карты был изменен в процессе операции. Флаг используется внутри.
            /// </summary>
            public readonly byte CardImageChanged;
            /// <summary>
            ///  Использовать серийный номер в качестве номера карты
            /// </summary>
            private readonly byte SerialNumberAsIdentifier;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CardOperationItem
        {
            /// <summary>
            /// Размер структуры
            /// </summary>
            public int Size;
            /// <summary>
            /// Тип товара: 0-товары,1-услуги,2-топливо
            /// </summary>
            public byte GoodKind;
            private CustomString goodCode;
            /// <summary>
            /// Код товара
            /// </summary>
            public string GoodCode
            {
                get { return CustomStringToUnicode(goodCode); }
                set { if (value == null) { goodCode.str = IntPtr.Zero; goodCode.length = 0; } else { UnicodeStringToCustom(ref goodCode, value); } }
            }
            /// <summary>
            /// Группа
            /// </summary>
            public int Group;
            private CustomString goodName;
            /// <summary>
            /// Название
            /// </summary>
            public string GoodName
            {
                get { return CustomStringToUnicode(goodName); }
                set { if (value == null) { goodName.str = IntPtr.Zero; goodName.length = 0; } else { UnicodeStringToCustom(ref goodName, value); } }
            }
            /// <summary>
            /// Номер ТРК. 0-магазин
            /// </summary>
            public byte FuellingPoint;
            /// <summary>
            /// Код продукта списания
            /// </summary>
            public int ProductID;
            /// <summary>
            /// Единицы списания с кошелька(!): 0-количество, 1-сумма
            /// </summary>
            public int UnitID;
            /// <summary>
            /// Номер использованного кошелька
            /// </summary>
            public int PurseNumber;
            /// <summary>
            /// Единицы заказа(!): 0-количество, 1-сумма
            /// </summary>
            public byte OrderUnit;
            /// <summary>
            /// Цена
            /// </summary>
            public double Price;
            ///Количество (Объем) 
            public double Quantity;
            /// <summary>
            /// Скидка с цены в %
            /// </summary>
            public double PriceDiscountPercent;
            /// <summary>
            /// Скидка с цены в р.
            /// </summary>
            public double PriceDiscountAbsolute;
            /// <summary>
            /// Скидка с цены в % по факту
            /// </summary>
            public double FactDiscountPercent;
            /// <summary>
            /// Скидка с цены в р. по факту 
            /// </summary>
            public double FactDiscountAbsolute;
            /// <summary>
            /// Причина отказа по позиции
            /// </summary>
            public int DenyReason;
            /// <summary>
            /// Используется постоянная скидка. По факту остается.
            /// </summary>
            public bool ConstantDiscount;
            /// <summary>
            /// Копейки округления
            /// </summary>
            public double RoundOff;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        private struct SerialnumberT
        {
            [FieldOffset(0)]
            private readonly uint FourByteUID;
            [FieldOffset(0)]
            private fixed byte SevenByteUID[7];

            //public Tuple <string, str> GetSerial()
            //{
            //    var res1 = BitConverter.GetBytes(FourByteUID);
            //    var res2 = new byte[7];

            //    fixed (byte* p = SevenByteUID)
            //    {
            //        for (int i = 0; i < 7; ++i)
            //        {
            //            res2[i] = *(p + i);
            //        }
            //    }
            //    return new Tuple<string, str>(BitConverter.ToString(res1), BitConverter.ToString(res2));
            //}
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        private struct Mf1S70T
        {
            public const int Size = 4096;

            [FieldOffset(0)]
            private fixed byte block[Size];

            public byte[] GetBlocks()
            {
                var res = new byte[Size];
                fixed (byte* p = block)
                {
                    for (var i = 0; i < Size; ++i)
                    {
                        res[i] = *(p + i);
                    }
                }
                return res;
            }

            /*
                        public void ClearBlocks()
                        {
                            fixed (byte* p = this.block)
                            {
                                for (int i = 0; i < Size; ++i)
                                {
                                    *(p + i) = 0xA0;
                                }
                            }
                        }
            */
        }

        public struct ServioCardInfo
        {
            public int ErrorCore;
            public string ErrorDescription => ((ErrorCodeDescriptions)ErrorCore).ToString();

            public string CardInfo { get; set; }

            public string CardNumber;
            public int IssuerID;

            public static ServioCardInfo operator +(ServioCardInfo obj1, ServioCardInfo obj2)
            {
                if (obj2.ErrorCore != (int)ErrorCodes.ESuccess)
                    obj1.ErrorCore = obj2.ErrorCore;

                obj1.CardNumber = obj2.CardNumber;
                obj1.IssuerID = obj2.IssuerID;
                obj1.CardInfo += "\r\n\r\n" + obj2.CardInfo;
                return obj1;
            }

            public override string ToString()
            {
                return (ErrorCore != 0) ?
                    $"Ошибка чтения карты: {ErrorCore} {ErrorDescription}" : 
                    $"Номер карты: {IssuerID}\\{CardNumber.PadLeft(20, '0')}";
            }
        }

        private static readonly string _config = @"C:\ServioCardAPI\SDK\Mifaread3.ini";

        [DllImport(@"C:\ServioCardAPI\SDK\Mifaread3.dll")]
        private static extern int SetCallback(IntPtr obj, IntPtr callBack, IntPtr userData);
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct CustomInteraction
        {
            private readonly int Size;            // Размер структуры
            public readonly int CIType;          // Тип инетрфейса
            public int ReturnCode;      // Результат выполнения: 0-ок, 1-ошибка, 2-отмена
            private readonly IntPtr Msg;             // Сообщение
            private readonly IntPtr Title;           // Заголовок окна
            private readonly int Flags;           // Флаги MessageBox / InputBox и т.д.
            private readonly uint Timeout;         // Таймаут показа диалогового окна
            private readonly int ProgressTotal;   // Макс.значение ProgressBar. Если ProgressTotal <= 0 То нужно просто показать процесс длительной операции, иначе можно ProgressBar.
            private readonly int ProgressCurrent; // Текущее значение ProgressBar. Если ProgressCurrent >= ProgressTotal то ProgressBar можно спрятать (операция завершена) иначе происходит выполнение операции.
            public readonly IntPtr Buffer;          // Строка с резултатом ввода (InputBox)
            public readonly int BufLen;          // Макс.длина буфера
            private readonly int ItemCount;       // Кол-во позиций ListBox
            private readonly IntPtr Items;         // Позиции ListBox
            private readonly int ItemIndex;       // Индекс выбранной позиции
        }
        private static string _pin = "";
        private static readonly CustomInteractionCallbackFuncDelegate Callback = CustomInteractionCallbackFunc;
        private delegate int CustomInteractionCallbackFuncDelegate(IntPtr ci, IntPtr userData);
        private static int CustomInteractionCallbackFunc(IntPtr ci, IntPtr userData)
        {

            CustomInteraction str = (CustomInteraction)Marshal.PtrToStructure(ci, typeof(CustomInteraction));

            if (str.CIType == 103)
            {
                MessageBox.Show($@"str.CIType = {str.CIType}, pin = {_pin}, str.BufLen = {str.BufLen}");
                var data = Encoding.Unicode.GetBytes(_pin + "\0");
                for (int z = 0; z < data.Length && z < str.BufLen; z++)
                    Marshal.WriteByte(str.Buffer, z, data[z]);
                //str.Buffer = Marshal.StringToHGlobalUni(pin);
                //str.BufLen = pin.Length;//data.Length;
                str.ReturnCode = 1;
            }
            Marshal.StructureToPtr(str, ci, true);

            return 0;
        }

        public static ServioCardInfo ReadCardNumber(string pin)
        {
            ServioCardInfo res = new ServioCardInfo { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            try
            {
                _pin = pin;
                IntPtr obj = new IntPtr();

                if ((res.ErrorCore = InitInternal(_config, ref obj)) != 0)
                    throw new CardShellException("Error Init No" + res, res.ErrorCore);

                SetCallback(obj, Marshal.GetFunctionPointerForDelegate(Callback), IntPtr.Zero);
                SerialnumberT serialNumber = new SerialnumberT();
                IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
                IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
                IntPtr operationPtr = CardOperation_Create();
                var operation = (CardOperation)Marshal.PtrToStructure(operationPtr, typeof(CardOperation));

                if ((res.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, true)) != 0)
                    throw new CardShellException("Error ReadCard No" + res, res.ErrorCore);

                operation.PosName = "TestTestTest";
                operation.Size = sizeof(CardOperation);
                operation.POS = 1;
                operation.OpTime = DateTime.Now.ToOADate();
                operation.IsPostpay = 0;
                operation.CardType = (int)CardType.CtIdentifierAuthorization;
                operation.SerialNumber = serialNumber;
                operation.CardNumber = null;
                operation.IssuerID = -1;
                operation.AddPrefixZeros = 0;
                operation.WoCard = 0;
                operation.PINChecked = (byte)(pin == "" ? 1 : 0);
                operation.ItemCount = 0;
                Marshal.StructureToPtr(operation, operationPtr, true);

                //Operation = (TCardOperation)Marshal.PtrToStructure(_Operation, typeof(TCardOperation));

                if ((res.ErrorCore = Auth(obj, serialNumberPtr, cardImagePtr, operationPtr)) != 0)
                    throw new CardShellException("Error Auth No" + res, res.ErrorCore);

                operation = (CardOperation)Marshal.PtrToStructure(operationPtr, typeof(CardOperation));
                res.IssuerID = operation.IssuerID;
                res.CardNumber = operation.CardNumber.PadLeft(20, '0');
            }
            finally
            {
                _pin = "";
            }
            return res;
        }
        public static ServioCardInfo ReadCardNumber()
        {
            ServioCardInfo res = new ServioCardInfo { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            IntPtr obj = new IntPtr();

            if ((res.ErrorCore = InitInternal(_config, ref obj)) != 0)
                throw new CardShellException("Error Init No" + res, res.ErrorCore);

            IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
            IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
            IntPtr operationPtr = CardOperation_Create();
            try
            {
                if ((res.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, true)) != 0)
                    throw new CardShellException("Error ReadCard No" + res, res.ErrorCore);
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

                if ((res.ErrorCore = Auth(obj, serialNumberPtr, cardImagePtr, operationPtr)) != 0)
                    throw new CardShellException("Error Auth No" + res, res.ErrorCore);

                var operation = UnMemory<CardOperation>.ReadInMem(operationPtr);
                //var op = Operation.CheckImage;
                //SerialNumber = MarshalHelper.UnMemory<SERIALNUMBER_T>.ReadInMem(SerialNumberPtr);
                //var ser = SerialNumber.getSerial();

                //Operation = (TCardOperation)Marshal.PtrToStructure(OperationPtr, typeof(TCardOperation));
                res.IssuerID = operation.IssuerID;
                res.CardNumber = operation.CardNumber.PadLeft(20, '0');
            }
            finally
            {
                UnMemory.FreeIntPtr(serialNumberPtr);
                UnMemory.FreeIntPtr(cardImagePtr);
                //MarshalHelper.UnMemory.FreeIntPtr(operationPtr);
                CardOperation_Free(operationPtr);
            }
            return res;
        }
        // АВТОРИЗАЦИЯ
        public static ServioCardInfo Authorize()
        {
            ServioCardInfo res = new ServioCardInfo { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            IntPtr obj = new IntPtr();

            IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
            IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
            IntPtr operationPtr = CardOperation_Create();

            try
            {
                if ((res.ErrorCore = InitInternal(_config, ref obj)) != (int) ErrorCodes.ESuccess)
                {
                    if (res.ErrorCore == (int) ErrorCodes.ECancel)
                    {
                        return res;
                    }
                    throw new CardShellException("Error Init No" + res, res.ErrorCore);
                }
                if ((res.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, true)) != (int) ErrorCodes.ESuccess)
                {
                    if (res.ErrorCore == (int) ErrorCodes.ECancel)
                    {
                        return res;
                    }
                    throw new CardShellException("Error ReadCard No" + res, res.ErrorCore);
                }

                var operation = UnMemory<CardOperation>.ReadInMem(operationPtr);
                SerialnumberT serialNumber = UnMemory<SerialnumberT>.ReadInMem(serialNumberPtr);

                operation.Size = sizeof(CardOperation);
                operation.POS = 1; // Номер АЗС или терминала
                operation.OpTime = DateTime.Now.ToOADate(); // Реальное время с вашего терминала
                operation.TransactID = 0; // Тут желательно ваш код код транзакции указать
                operation.OpCode = (byte) OpCode.OpAuthorization;
                operation.IsPostpay = 0; // как есть - если постоплата - указать true
                operation.CardType = (int) CardType.Unknown; // Предположим что тип карты неизвестен
                operation.SerialNumber = serialNumber; // Указать серийный номер, считанный с метки
                operation.IssuerID = -1;
                    // Указать -1, программа сама переопределит. Если указать другой код - программа будет использовать его
                operation.CardNumber = ""; // Не указывать !!!
                operation.AddPrefixZeros = 0; // Нужна доп. настройка. Аналогичная нашей опции в обработчиках
                operation.WoCard = 0; //
                operation.PINChecked = 0;
                    // Если указать false то драйвер запросит форму ввода пароля через callback в случае, если на карту поставили PIN-код
                operation.ItemCount = 0; // Позиции не нужны

                UnMemory<CardOperation>.SaveInMem(operation, ref operationPtr);

                if ((res.ErrorCore = Auth(obj, serialNumberPtr, cardImagePtr, operationPtr)) !=
                    (int) ErrorCodes.ESuccess)
                {
                    if (res.ErrorCore == (int) ErrorCodes.ECancel)
                    {
                        return res;
                    }
                    throw new CardShellException("Error Auth No" + res, res.ErrorCore);
                }
                operation = UnMemory<CardOperation>.ReadInMem(operationPtr);
                res.IssuerID = operation.IssuerID;
                res.CardNumber = operation.CardNumber.PadLeft(20, '0');
                return res;
            }
            catch (CardShellException ex)
            {
                return new ServioCardInfo { ErrorCore = ex.ErrorCode};
            }
            catch
            {
                return new ServioCardInfo {ErrorCore = (int)ErrorCodes.EGeneric, };
            }
            finally
            {
                UnMemory.FreeIntPtr(serialNumberPtr);
                UnMemory.FreeIntPtr(cardImagePtr);
                //MarshalHelper.UnMemory.FreeIntPtr(operationPtr);
                CardOperation_Free(operationPtr);
            }
        }

        public static byte[] GetCard()
        {
            ServioCardInfo res = new ServioCardInfo { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            IntPtr obj = new IntPtr();

            if ((res.ErrorCore = InitInternal(_config, ref obj)) != 0)
                throw new CardShellException("Error Init No" + res, res.ErrorCore);

            IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
            IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
            UnMemory<byte>.SaveInMemArr(new byte[Mf1S70T.Size], ref cardImagePtr);
            if ((res.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, false)) != 0)
                throw new CardShellException("Error ReadCard No" + res, res.ErrorCore);

            var blocks = UnMemory<Mf1S70T>.ReadInMem(cardImagePtr);
            var card = blocks.GetBlocks();

            return card;
        }
        // ИНФОРМАЦИЯ ПО КАРТЕ
        public static ServioCardInfo GetCardInfo()
        {
            ServioCardInfo res = new ServioCardInfo { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            IntPtr obj = new IntPtr();

            IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
            IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
            IntPtr operationPtr = CardOperation_Create();

            try
            {
                if ((res.ErrorCore = InitInternal(_config, ref obj)) != (int)ErrorCodes.ESuccess)
                {
                    if (res.ErrorCore == (int)ErrorCodes.ECancel)
                    {
                        return res;
                    }
                    throw new CardShellException("Error Init No" + res, res.ErrorCore);
                }
                UnMemory<byte>.SaveInMemArr(new byte[Mf1S70T.Size], ref cardImagePtr);
                if ((res.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, false)) != (int)ErrorCodes.ESuccess)
                {
                    if (res.ErrorCore == (int)ErrorCodes.ECancel)
                    {
                        return res;
                    }
                    throw new CardShellException("Error ReadCard No" + res, res.ErrorCore);
                }
                var operation = UnMemory<CardOperation>.ReadInMem(operationPtr);
                SerialnumberT serialNumber = UnMemory<SerialnumberT>.ReadInMem(serialNumberPtr);

                operation.Size = sizeof(CardOperation);
                operation.POS = 1; // Номер АЗС или терминала
                operation.OpTime = DateTime.Now.ToOADate(); // Реальное время с вашего терминала
                operation.TransactID = 0; // Тут желательно ваш код код транзакции указать
                operation.CardType = (int)CardType.Unknown; // Предположим что тип карты неизвестен
                operation.SerialNumber = serialNumber; // Указать серийный номер, считанный с метки
                operation.IssuerID = -1; // Указать -1, программа сама переопределит. Если указать другой код - программа будет использовать его
                operation.CardNumber = ""; // Не указывать !!!
                operation.AddPrefixZeros = 0; // Нужна доп. настройка. Аналогичная нашей опции в обработчиках
                operation.ItemCount = 0; // При аутентификации позиции не заполнять.

                UnMemory<CardOperation>.SaveInMem(operation, ref operationPtr);

                // Смотрим информацию по карте
                if ((res.ErrorCore = CardInfo(obj, serialNumberPtr, cardImagePtr, operationPtr)) != (int)ErrorCodes.ESuccess)
                    throw new CardShellException("Error CardInfo No" + res, res.ErrorCore);

                // флаг указывает на то что образ карты был изменен в процессе операции
                // и нужно записать его обратно.
                // Нужно для пополнения карты при просмотре информации
                operation = UnMemory<CardOperation>.ReadInMem(operationPtr);
                if (operation.CardImageChanged != 0)
                {
                    // при ошибке можно попробовать повторить операцию несколько раз 3-10...
                    // если не проходит то это патовая ситуация и для её дальнейшего
                    // разрешения нужно сохранить SerialNumber, CardImage и CardInfo
                    // В теории, сохранённый SerialNumber и CardImage можно попробовать
                    // записать обратно даже при перезапуске ПО, но до следующего
                    // обслуживания по карте. Чтобы решить ситуацию можно сохранить CardInfo
                    // чтобы в офисе уменьшили или увеличили баланс карты на соотв. значение.
                    if ((res.ErrorCore = WriteCard(obj, serialNumberPtr, cardImagePtr)) != (int)ErrorCodes.ESuccess)
                        throw new CardShellException("Error WriteCard No" + res, res.ErrorCore);
                }

                operation = (CardOperation)Marshal.PtrToStructure(operationPtr, typeof(CardOperation));
                res.CardInfo = operation.CheckImage;
                res.IssuerID = operation.IssuerID;
                res.CardNumber = operation.CardNumber;
                return res;
            }
            finally
            {
                UnMemory.FreeIntPtr(serialNumberPtr);
                UnMemory.FreeIntPtr(cardImagePtr);
                //MarshalHelper.UnMemory.FreeIntPtr(operationPtr);
                CardOperation_Free(operationPtr);
            }

        }

        // ОПЕРАЦИЯ ПО КАРТЕ
        /// <summary>
        /// ОПЕРАЦИЯ ПО КАРТЕ
        /// </summary>
        /// <param name="type">тип операции</param>
        /// <param name="terminal">терминал (азс)</param>
        /// <param name="transact">номер транзакции</param>
        /// <param name="opItem">параметры операции</param>
        /// <returns></returns>
        public static ServioCardInfo CardOperationExecute(CardOperationType type, int terminal, long transact, CardOperationItem opItem)
        {
            ServioCardInfo res = new ServioCardInfo { ErrorCore = -1, CardNumber = "", IssuerID = -1 };
            IntPtr obj = new IntPtr();

            IntPtr serialNumberPtr = Marshal.AllocHGlobal(sizeof(SerialnumberT));
            IntPtr cardImagePtr = Marshal.AllocHGlobal(sizeof(Mf1S70T));
            IntPtr operationPtr = CardOperation_Create();

            try
            {
                if ((res.ErrorCore = InitInternal(_config, ref obj)) != (int)ErrorCodes.ESuccess)
                {
                    if (res.ErrorCore == (int)ErrorCodes.ECancel)
                    {
                        return res;
                    }
                    throw new CardShellException("Error Init No" + res, res.ErrorCore);
                }
                UnMemory<byte>.SaveInMemArr(new byte[Mf1S70T.Size], ref cardImagePtr);
                if ((res.ErrorCore = ReadCard(obj, serialNumberPtr, cardImagePtr, false)) != (int)ErrorCodes.ESuccess)
                {
                    if (res.ErrorCore == (int)ErrorCodes.ECancel)
                    {
                        return res;
                    }
                    throw new CardShellException("Error ReadCard No" + res, res.ErrorCore);
                }
                var operation = UnMemory<CardOperation>.ReadInMem(operationPtr);
                SerialnumberT serialNumber = UnMemory<SerialnumberT>.ReadInMem(serialNumberPtr);

                operation.Size = sizeof(CardOperation);
                operation.OpCode = (byte)OperationSelector(type);
                operation.POS = terminal; // Номер АЗС или терминала
                operation.OpTime = DateTime.Now.ToOADate(); // Реальное время с вашего терминала
                operation.TransactID = transact; // Тут желательно ваш код код транзакции указать
                operation.CardType = (int)CardType.CtPayment; // Предположим что тип карты неизвестен
                operation.SerialNumber = serialNumber; // Указать серийный номер, считанный с метки
                operation.IssuerID = -1; // Указать -1, программа сама переопределит. Если указать другой код - программа будет использовать его
                operation.CardNumber = ""; // Не указывать !!!
                operation.AddPrefixZeros = 0; // Нужна доп. настройка. Аналогичная нашей опции в обработчиках
                operation.ItemCount = 1; // При аутентификации позиции не заполнять.

                IntPtr pItem1 = Marshal.AllocHGlobal(sizeof(CardOperationItem));
                UnMemory<CardOperationItem>.SaveInMem(opItem, ref pItem1);
                IntPtr ppItem1 = Marshal.AllocHGlobal(sizeof(IntPtr));
                UnMemory<IntPtr>.SaveInMem(pItem1, ref ppItem1);

                operation.Items = ppItem1;

                UnMemory<CardOperation>.SaveInMem(operation, ref operationPtr);

                if ((res.ErrorCore = 
                    OperationSelector(type, obj, serialNumberPtr, cardImagePtr, operationPtr))
                    != (int)ErrorCodes.ESuccess)
                    throw new CardShellException("Error Operation No" + res, res.ErrorCore);

                // флаг указывает на то что образ карты был изменен в процессе операции
                // и нужно записать его обратно.
                // Нужно для пополнения карты при просмотре информации
                operation = UnMemory<CardOperation>.ReadInMem(operationPtr);
                if (operation.CardImageChanged != 0)
                {
                    // при ошибке можно попробовать повторить операцию несколько раз 3-10...
                    // если не проходит то это патовая ситуация и для её дальнейшего
                    // разрешения нужно сохранить SerialNumber, CardImage и CardInfo
                    // В теории, сохранённый SerialNumber и CardImage можно попробовать
                    // записать обратно даже при перезапуске ПО, но до следующего
                    // обслуживания по карте. Чтобы решить ситуацию можно сохранить CardInfo
                    // чтобы в офисе уменьшили или увеличили баланс карты на соотв. значение.
                    if ((res.ErrorCore = WriteCard(obj, serialNumberPtr, cardImagePtr)) != (int)ErrorCodes.ESuccess)
                        throw new CardShellException("Error WriteCard No" + res, res.ErrorCore);
                }

                operation = (CardOperation)Marshal.PtrToStructure(operationPtr, typeof(CardOperation));
                res.CardInfo = operation.CheckImage;
                return res;
            }
            finally
            {
                //CardOperation_Free(operationPtr);
                UnMemory.FreeIntPtr(serialNumberPtr);
                UnMemory.FreeIntPtr(cardImagePtr);
                //MarshalHelper.UnMemory.FreeIntPtr(operationPtr);
            }
        }

        public static OpCode OperationSelector(CardOperationType type)
        {
            switch (type)
            {
                case CardOperationType.Credit:
                    return OpCode.OpCredit;
                case CardOperationType.Debit:
                    return OpCode.OpDebit;
                case CardOperationType.Sale:
                    return OpCode.OpDebit;
                case CardOperationType.Refund:
                    return OpCode.OpRefund;
                case CardOperationType.Return:
                    return OpCode.OpAnnulate;
                default:
                    return OpCode.OpAuthorization;
            }
        }
        public static int OperationSelector(CardOperationType type, IntPtr objRef, IntPtr serialNumber, IntPtr cardImage, IntPtr operation)
        {
            switch (type)
            {
                case CardOperationType.Credit:
                    return Credit(objRef, serialNumber, cardImage, operation);
                //case CardOperationType.Debit:
                //    return Debit(objRef, serialNumber, cardImage);
                case CardOperationType.Sale:
                    return Sale(objRef, serialNumber, cardImage, operation);
                //case CardOperationType.Refund:
                //    return Refund(objRef, serialNumber, cardImage/*, operation*/);
                case CardOperationType.Return:
                    return Return(objRef, serialNumber, cardImage, operation);
                default:
                    return -2000;
            }
        }

        private static void UnicodeStringToCustom(ref CustomString customString, string str)
        {
            str += "\0";
            IntPtr s = Marshal.StringToHGlobalUni(str);
            CustomStringSet(ref customString, s, (!string.IsNullOrEmpty(str) && str[0] == '\0') ? 0 : str.Length*2);
            customString.length = str.Length - 1;
        }
        private static string CustomStringToUnicode(CustomString customString)
        {
            int len = CustomStringLength(ref customString);
            if (len <= 0)
            {
                return "";
            }

            IntPtr s = Marshal.AllocHGlobal(len*3);
            CustomStringGet(ref customString, s, len*3);
            //byte[] tmp  = new byte[256];
            //for (int z = 0; z < len + 20; z++)
            //    tmp[z] = Marshal.ReadByte(s, z);
            var r = Marshal.PtrToStringUni(s);
            return r;
        }
    }
}
